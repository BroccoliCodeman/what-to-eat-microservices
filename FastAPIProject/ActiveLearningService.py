import os
import json
import uuid
import cv2
import asyncio
from datetime import datetime
from typing import List, Dict, Any, Optional
import structlog

from CONFIG import Config


class ActiveLearningService:
    def __init__(self, redis_client, logger, retrain_service):
        """
        :param redis_client: Асинхронний клієнт Redis
        :param logger: structlog логер
        :param retrain_service: Інстанс RetrainService для запуску навчання
        """
        self.redis = redis_client
        self.logger = logger
        self.retrain_service = retrain_service

    def process_uncertainty(self, request_id: str, detection_result: Dict[str, Any]) -> Dict[str, Any]:
        """
        Обробляє результат одного передбачення під час інференсу.
        1. Генерує унікальний global_id.
        2. Перевіряє confidence.
        3. Якщо confidence < порогу -> зберігає фото на диск.
        4. Повертає модифікований словник результату.
        """
        conf = detection_result.get('confidence', 0.0)
        object_id = detection_result.get('object_id', 0)

        # Вилучаємо масив зображення, щоб він не потрапив у JSON-відповідь API.
        # Crop приходить у форматі RGB (так як ModelService працює з RGB).
        crop_img = detection_result.pop('crop_img', None)

        # 1. ГЕНЕРАЦІЯ ГЛОБАЛЬНОГО ID
        # Формат: {UUID_запиту}_{Номер_об'єкта}.jpg
        # Цей ID буде ключем і для фронтенду, і для файлової системи.
        global_id = f"{request_id}_{object_id}.jpg"

        # Додаємо ID у результат, щоб фронтенд міг його використати
        detection_result['id'] = global_id

        # 2. ЛОГІКА НЕВПЕВНЕНОСТІ (Uncertainty Sampling)
        if conf < Config.UNCERTAINTY_THRESHOLD:
            # Формуємо шлях збереження
            save_path = os.path.join(Config.UNCERTAIN_DIR, global_id)

            # Зберігаємо фізичний файл, якщо картинка є
            if crop_img is not None:
                try:
                    # OpenCV очікує BGR, а модель віддає RGB. Конвертуємо:
                    crop_bgr = cv2.cvtColor(crop_img, cv2.COLOR_RGB2BGR)
                    cv2.imwrite(save_path, crop_bgr)
                except Exception as e:
                    if self.logger:
                        self.logger.error("failed_to_save_crop", error=str(e), id=global_id)

            # Оновлюємо статус
            detection_result['status'] = 'uncertain'
            detection_result['crop_ref'] = global_id

            if self.logger:
                self.logger.info("al_sample_captured", id=global_id, confidence=conf)
        else:
            # Якщо впевнені — просто ставимо статус
            detection_result['status'] = 'confident'
            detection_result['crop_ref'] = None

        return detection_result

    def get_uncertain_samples(self, limit: int = 50) -> List[str]:
        """
        Повертає список файлів (ID), які чекають на розмітку в папці uncertain.
        Використовується для адмін-панелі (/active-learning/pool).
        """
        try:
            if not os.path.exists(Config.UNCERTAIN_DIR):
                return []

            files = os.listdir(Config.UNCERTAIN_DIR)
            # Фільтруємо тільки зображення
            images = [f for f in files if f.lower().endswith(('.jpg', '.png', '.jpeg'))]
            # Сортуємо, щоб нові були (або старі) в певному порядку, тут просто ліміт
            return images[:limit]
        except Exception as e:
            if self.logger:
                self.logger.error("get_pool_failed", error=str(e))
            return []

    async def handle_feedback(self, image_id: str, correct_label: str) -> Dict[str, Any]:
        """
        Обробляє фідбек від користувача:
        1. Валідує клас.
        2. Переміщує файл з uncertain -> labeled.
        3. Додає запис у Redis чергу.
        4. Перевіряє розмір черги і тригерить донавчання.
        """
        # 1. Валідація класу
        if correct_label not in Config.CLASS_NAMES:
            raise ValueError(f"Label '{correct_label}' is not in valid class list")

        # 2. Переміщення файлу (Data Management)
        source_path = os.path.join(Config.UNCERTAIN_DIR, image_id)
        if not os.path.exists(source_path):
            # Можливо, файл вже розмітили або його не існує
            raise FileNotFoundError(f"Image {image_id} not found in uncertain pool")

        # Генеруємо нове ім'я для dataset: ClassName_RandomUUID.jpg
        new_filename = f"{correct_label}_{uuid.uuid4().hex[:8]}.jpg"
        dest_path = os.path.join(Config.LABELED_DIR, new_filename)

        try:
            os.rename(source_path, dest_path)
        except OSError as e:
            self.logger.error("file_move_failed", src=source_path, dest=dest_path, error=str(e))
            raise e

        # 3. Формування об'єкта для черги навчання
        learning_sample = {
            "image_path": dest_path,
            "label": correct_label,
            "timestamp": datetime.now().isoformat(),
            "origin": "user_feedback",
            "original_id": image_id
        }

        # 4. Робота з Redis (Queue Management)
        queue_size = 0
        status_msg = "accumulating"

        if self.redis:
            # Додаємо sample в кінець черги
            await self.redis.rpush("training_queue", json.dumps(learning_sample))
            queue_size = await self.redis.llen("training_queue")

            self.logger.info("feedback_processed", label=correct_label, queue_size=queue_size)

            # 5. Тригер донавчання (Triggering Logic)
            if queue_size >= Config.MIN_RETRAIN_SAMPLES:
                self.logger.info("retrain_threshold_reached",
                                 current=queue_size,
                                 limit=Config.MIN_RETRAIN_SAMPLES)

                # Атомарно забираємо всі елементи з черги для батчу
                training_batch = []

                # Вичитуємо чергу
                while True:
                    item = await self.redis.lpop("training_queue")
                    if not item:
                        break
                    training_batch.append(json.loads(item))

                if training_batch:
                    # Запускаємо процес навчання у фоні (fire and forget для API запиту)
                    # Використовуємо asyncio.create_task, щоб не блокувати відповідь клієнту
                    asyncio.create_task(self.retrain_service.trigger_retrain(training_batch))

                    status_msg = "retraining_triggered"
                else:
                    self.logger.warning("training_queue_empty_after_check")

        return {
            "status": "success",
            "action": status_msg,
            "queue_size": queue_size,
            "samples_needed": Config.MIN_RETRAIN_SAMPLES
        }


    async def force_retraining(self) -> Dict[str, Any]:
        """
        Мануальний запуск перетренування.
        Вичитує всю чергу Redis і запускає RetrainService, ігноруючи ліміти.
        """
        if not self.redis:
            return {"status": "error", "message": "Redis not connected", "samples_count": 0}

        # 1. Атомарно вичитуємо всю чергу
        training_batch = []
        while True:
            item = await self.redis.lpop("training_queue")
            if not item:
                break
            try:
                training_batch.append(json.loads(item))
            except json.JSONDecodeError:
                continue

        count = len(training_batch)

        # 2. Якщо даних немає
        if count == 0:
            return {
                "status": "skipped",
                "message": "Queue is empty. Nothing to train on.",
                "samples_count": 0
            }

        # 3. Запускаємо навчання у фоні
        self.logger.info("manual_retrain_triggered", samples=count)

        # Використовуємо asyncio.create_task, щоб не блокувати відповідь на HTTP запит
        asyncio.create_task(self.retrain_service.trigger_retrain(training_batch))

        return {
            "status": "success",
            "message": "Retraining process started in background.",
            "samples_count": count
        }