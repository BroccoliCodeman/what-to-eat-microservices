import os
import json
import sys


class Config:
    # --- Paths ---
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    YOLO_MODEL_PATH = "yolov8s-world.pt"
    CLASSIFIER_MODEL_PATH = os.path.join(BASE_DIR, "best_model.pth")

    # Шлях до файлу з класами (має бути в корені проекту)
    CLASSES_JSON_PATH = os.path.join(BASE_DIR, "final_filtered_data.json")

    # Directories for Active Learning
    UPLOAD_DIR = os.path.join(BASE_DIR, "data/uploads")
    UNCERTAIN_DIR = os.path.join(BASE_DIR, "data/active_learning/uncertain")
    LABELED_DIR = os.path.join(BASE_DIR, "data/active_learning/labeled")

    # --- Model Settings ---
    YOLO_CONFIDENCE = 0.10
    IMG_SIZE = 224
    CLASSIFICATION_THRESHOLD = 0.10
    # --- Active Learning Settings ---
    ENABLE_ACTIVE_LEARNING = True
    UNCERTAINTY_THRESHOLD = 0.70

    # --- Redis ---
    REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
    REDIS_PORT = 6379
    REDIS_DB = 0

    # --- Dynamic Class Loading ---
    CLASS_NAMES = []

    @classmethod
    def load_classes(cls):
        """Завантажує класи з JSON файлу, зберігаючи порядок тренування"""
        print(f"📂 Завантаження класів з: {cls.CLASSES_JSON_PATH}")

        if not os.path.exists(cls.CLASSES_JSON_PATH):
            print(f"❌ ПОМИЛКА: Файл {cls.CLASSES_JSON_PATH} не знайдено!")

            sys.exit(1)

        try:
            with open(cls.CLASSES_JSON_PATH, 'r', encoding='utf-8') as f:
                data = json.load(f)

            # Відновлюємо список імен, сортуючи за ID (ключ словника categories)
            # Структура JSON: "categories": { "0": {"id": 0, "name": "Additives"}, ... }
            cls.CLASS_NAMES = [
                cat_info['name'] for cat_id, cat_info in
                sorted(data['categories'].items(), key=lambda x: int(x[0]))
            ]

            print(f"✅ Успішно завантажено {len(cls.CLASS_NAMES)} класів.")

        except Exception as e:
            print(f"❌ Помилка читання JSON: {e}")
            sys.exit(1)

    @classmethod
    def setup_directories(cls):
        for d in [cls.UPLOAD_DIR, cls.UNCERTAIN_DIR, cls.LABELED_DIR]:
            os.makedirs(d, exist_ok=True)


# Ініціалізація при імпорті
Config.setup_directories()
Config.load_classes()