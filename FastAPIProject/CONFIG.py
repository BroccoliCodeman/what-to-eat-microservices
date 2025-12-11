import os
import json
import sys


class Config:
    # --- Paths ---
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    YOLO_MODEL_PATH = "yolov8s-world.pt"
    CLASSIFIER_MODEL_PATH = os.path.join(BASE_DIR, "best_model.pth")
    CLASSES_JSON_PATH = os.path.join(BASE_DIR, "final_filtered_data.json")
    DB_PATH = os.path.join(BASE_DIR, "data/logs.sqlite")

    # Directories
    UPLOAD_DIR = os.path.join(BASE_DIR, "data/uploads")
    UNCERTAIN_DIR = os.path.join(BASE_DIR, "data/active_learning/uncertain")
    LABELED_DIR = os.path.join(BASE_DIR, "data/active_learning/labeled")
    BACKUP_DIR = os.path.join(BASE_DIR, "data/backups")

    # --- Model Settings ---
    YOLO_CONFIDENCE = 0.10
    IMG_SIZE = 224
    CLASSIFICATION_THRESHOLD = 0.40  # Підвищено для надійності

    # --- Active Learning & Retraining ---
    ENABLE_ACTIVE_LEARNING = True
    UNCERTAINTY_THRESHOLD = 0.70

    # Safety First Strategy
    MIN_RETRAIN_SAMPLES = 10  # Мінімум нових фото для запуску навчання
    RETRAIN_EPOCHS = 3  # Ultra-conservative
    RETRAIN_LR = 1e-5  # Low Learning Rate
    MAX_BASELINE_DEGRADATION = 0.05  # Допустиме падіння точності (5%)
    BASELINE_SAMPLE_SIZE = 50  # Кількість фото для перевірки регресії

    # --- Redis ---
    REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
    REDIS_PORT = 6379
    REDIS_DB = 0

    # --- Dynamic Class Loading ---
    CLASS_NAMES = []

    @classmethod
    def load_classes(cls):
        # ... (Код без змін з вашого файлу) ...
        if not os.path.exists(cls.CLASSES_JSON_PATH):
            sys.exit(1)
        try:
            with open(cls.CLASSES_JSON_PATH, 'r', encoding='utf-8') as f:
                data = json.load(f)
            cls.CLASS_NAMES = [
                cat_info['name'] for cat_id, cat_info in
                sorted(data['categories'].items(), key=lambda x: int(x[0]))
            ]
        except Exception:
            sys.exit(1)

    @classmethod
    def setup_directories(cls):
        for d in [cls.UPLOAD_DIR, cls.UNCERTAIN_DIR, cls.LABELED_DIR, cls.BACKUP_DIR]:
            os.makedirs(d, exist_ok=True)


Config.setup_directories()
Config.load_classes()