import uuid
import cv2
import numpy as np
import os
import structlog
from typing import List
from contextlib import asynccontextmanager

from fastapi import FastAPI, File, UploadFile, HTTPException, Depends
from fastapi.staticfiles import StaticFiles

# --- Імпорти наших модулів ---
from CONFIG import Config
from dependencies import container
from middleware import RequestLoggingMiddleware
from fastapi.middleware.cors import CORSMiddleware
from logger_config import setup_logging
from PydanticModels import (
    PredictionResponse,
    FeedbackRequest,
    FeedbackResponse,
    PoolResponse,
    PoolItem,
    RetrainResponse
)

# 1. Налаштовуємо глобальний логер ПЕРЕД стартом додатку
logger = setup_logging()


# --- Lifespan (Керування запуском/зупинкою) ---
@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("system_startup", status="initializing_services")

    # Ініціалізація сервісів (Redis connection, Model loading)
    await container.initialize()

    yield

    # Очистка ресурсів при зупинці
    if container.redis_client:
        await container.redis_client.close()

    logger.info("system_shutdown", status="completed")


# Створення додатку
app = FastAPI(title="Food Recognition AI", version="3.3.0", lifespan=lifespan)


# Налаштування CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 2. Підключаємо Middleware для логування кожного запиту
# Це автоматично логує: method, path, status_code, duration, client_ip, та JSON body
app.add_middleware(RequestLoggingMiddleware)

# 3. Налаштування статики (для доступу до картинок по URL)
# Створюємо папку, якщо її немає, щоб уникнути помилки при старті
os.makedirs(Config.UNCERTAIN_DIR, exist_ok=True)
os.makedirs(Config.LABELED_DIR, exist_ok=True)

# Монтуємо папку data. Тепер файли доступні за адресою /data/...
app.mount("/data", StaticFiles(directory="data"), name="data")


# --- Dependencies (Inversion of Control) ---
def get_al_service():
    if not container.al_service:
        # 503 Service Unavailable, якщо Redis лежить або ініціалізація не пройшла
        raise HTTPException(503, "Active Learning Service not initialized")
    return container.al_service


def get_model_service():
    if not container.model_service:
        raise HTTPException(503, "Model Service not initialized")
    return container.model_service


# --- Endpoints ---

@app.post("/predict", response_model=PredictionResponse, tags=["Inference"])
async def predict(
        file: UploadFile = File(...),
        model_service=Depends(get_model_service),
        al_service=Depends(get_al_service)
):
    # Логер контексту (для бізнес-логіки всередині ендпоінту)
    log = structlog.get_logger()

    # 1. Читання та декодування зображення
    try:
        contents = await file.read()
        nparr = np.frombuffer(contents, np.uint8)
        image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

        if image is None:
            raise ValueError("Decoded image is None")

    except Exception as e:
        log.warning("image_decode_failed", filename=file.filename, error=str(e))
        raise HTTPException(400, "Invalid image file provided")

    # 2. Генерація ID запиту
    request_id = str(uuid.uuid4())

    # 3. Інференс (ModelService)
    # Повертає список словників з детекціями
    raw_results = model_service.predict_pipeline(image)

    processed_results = []
    al_triggered = False

    # 4. Обробка Active Learning
    for res in raw_results:
        # process_uncertainty робить наступне:
        # - Генерує глобальний 'id' (наприклад "reqUUID_0.jpg")
        # - Перевіряє confidence
        # - Якщо low confidence -> зберігає кроп на диск
        processed = al_service.process_uncertainty(request_id, res)

        if processed['status'] == 'uncertain':
            al_triggered = True

        processed_results.append(processed)

    # Логуємо бізнес-метрику (не технічну)
    log.info("prediction_completed",
             total_objects=len(processed_results),
             al_triggered=al_triggered)

    return {
        "request_id": request_id,
        "total_objects": len(processed_results),
        "active_learning_triggered": al_triggered,
        "results": processed_results
    }


@app.get("/active-learning/pool", response_model=PoolResponse, tags=["Active Learning"])
async def get_uncertain_pool(
        limit: int = 20,
        al_service=Depends(get_al_service)
):
    """
    Повертає список "невпевнених" зображень для адмін-панелі.
    """
    # Отримуємо список імен файлів
    files = al_service.get_uncertain_samples(limit)

    pool_items = []
    for filename in files:
        # Формуємо об'єкти для відповіді
        pool_items.append(PoolItem(
            id=filename,  # ID - це ім'я файлу
            url=f"/data/active_learning/uncertain/{filename}"  # URL для відображення
        ))

    return {
        "count": len(pool_items),
        "items": pool_items
    }


@app.post("/feedback", response_model=FeedbackResponse, tags=["Active Learning"])
async def submit_feedback(
        request: FeedbackRequest,
        al_service=Depends(get_al_service)
):
    """
    Приймає фідбек від користувача (виправлення класу).
    Якщо накопичиться достатньо даних -> тригерить ретрейнінг.
    """
    try:
        # al_service.handle_feedback повертає словник (dict)
        result = await al_service.handle_feedback(request.image_id, request.correct_label)

        # Формуємо зрозуміле повідомлення для клієнта на основі результату
        action = result.get('action', 'unknown')
        q_size = result.get('queue_size', 0)
        q_needed = result.get('samples_needed', '?')

        msg = f"Success. Action: {action}. Queue: {q_size}/{q_needed}"

        return {
            "status": "success",
            "message": msg
        }

    except FileNotFoundError:
        raise HTTPException(404, "Image not found (maybe already labeled)")
    except ValueError as e:
        raise HTTPException(400, str(e))  # Наприклад, неправильна назва класу
    except Exception as e:
        # Middleware залогує повний трейсбек, тут просто віддаємо 500
        raise HTTPException(500, "Internal processing error")


@app.post("/retrain/trigger", response_model=RetrainResponse, tags=["Active Learning"])
async def trigger_retrain(
        al_service=Depends(get_al_service)
):
    """
    Мануально запускає процес донавчання на накопичених даних.
    Ігнорує поріг MIN_RETRAIN_SAMPLES.
    """
    try:
        result = await al_service.force_retraining()

        return {
            "status": result["status"],
            "message": result["message"],
            "samples_count": result.get("samples_count", 0)
        }

    except Exception as e:
        # Логер middleware перехопить деталі, тут віддаємо помилку клієнту
        raise HTTPException(500, f"Failed to trigger retraining: {str(e)}")

@app.get("/classes", tags=["Info"])
async def get_classes():
    """Повертає список всіх відомих класів продуктів"""
    return {"classes": Config.CLASS_NAMES}


@app.get("/health", tags=["System"])
async def health():
    """Healthcheck для Kubernetes/Docker"""
    model_ok = container.model_service is not None
    redis_ok = container.redis_client is not None

    status = "healthy" if model_ok and redis_ok else "degraded"

    return {
        "status": status,
        "components": {
            "model": "up" if model_ok else "down",
            "redis": "up" if redis_ok else "down"
        },
        "model_version": getattr(container.model_service, "model_version", 0) if model_ok else 0
    }


# ... імпорти ...
from PydanticModels import ModelStatusResponse
import torch  # Якщо використовуєш torch для визначення девайсу


@app.get("/model/status", response_model=ModelStatusResponse, tags=["System"])
async def get_model_status():
    """Повертає загальну статистику системи."""

    # ... (код підрахунку файлів залишається без змін) ...
    uncertain_count = len(os.listdir(Config.UNCERTAIN_DIR)) if os.path.exists(Config.UNCERTAIN_DIR) else 0
    labeled_count = len(os.listdir(Config.LABELED_DIR)) if os.path.exists(Config.LABELED_DIR) else 0

    classes = getattr(container.model_service, "classes", [])
    device = "cuda" if torch.cuda.is_available() else "cpu"

    # ОТРИМУЄМО ВЕРСІЮ
    raw_version = getattr(container.model_service, "model_version", "unknown")

    return ModelStatusResponse(
        status="online",
        # ВИПРАВЛЕННЯ ТУТ: обгортаємо в str(), щоб гарантувати тип string
        model_version=str(raw_version),
        uncertain_count=uncertain_count,
        labeled_count=labeled_count,
        total_classes=len(classes),
        device=device
    )

from fastapi.responses import FileResponse


@app.get("/active-learning/image/{image_id}", tags=["Active Learning"])
async def get_uncertain_image(image_id: str):
    """
    Повертає зображення з папки uncertain за його ID.

    Args:
        image_id: Ім'я файлу (наприклад, "uuid_0.jpg")

    Returns:
        FileResponse: Зображення у форматі JPEG/PNG

    Raises:
        404: Якщо файл не знайдено
        400: Якщо ім'я файлу містить небезпечні символи
    """
    # Безпека: перевіряємо, що image_id не містить шляхів типу "../"
    if ".." in image_id or "/" in image_id or "\\" in image_id:
        raise HTTPException(400, "Invalid image ID format")

    # Формуємо повний шлях до файлу
    file_path = os.path.join(Config.UNCERTAIN_DIR, image_id)

    # Перевіряємо існування файлу
    if not os.path.exists(file_path):
        raise HTTPException(404, f"Image '{image_id}' not found in uncertain pool")

    # Визначаємо media type на основі розширення
    media_type = "image/jpeg"
    if image_id.lower().endswith(".png"):
        media_type = "image/png"

    # Повертаємо файл
    return FileResponse(
        path=file_path,
        media_type=media_type,
        filename=image_id
    )

if __name__ == "__main__":
    import uvicorn

    # Запуск сервера
    uvicorn.run(app, host="0.0.0.0", port=8000)