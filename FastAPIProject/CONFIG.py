import os


class Config:
    # --- Paths ---
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    YOLO_MODEL_PATH = "yolov8n.pt"
    CLASSIFIER_MODEL_PATH = os.path.join(BASE_DIR, "best_model.pth")

    # Directories for Active Learning
    UPLOAD_DIR = os.path.join(BASE_DIR, "data/uploads")
    UNCERTAIN_DIR = os.path.join(BASE_DIR, "data/active_learning/uncertain")
    LABELED_DIR = os.path.join(BASE_DIR, "data/active_learning/labeled")

    # --- Model Settings ---
    CLASS_NAMES = [
    # Фрукти та овочі
    'Apple', 'Banana', 'Broccoli', 'Cabbage', 'Carrot',
    'Cauliflower', 'Celery', 'Chayote', 'Corn', 'Cucumber',
    'Dates', 'Dragon Fruit', 'Eggplant', 'Grapes', 'Guava',
    'Jackfruit', 'Watermelon',

    # М'ясо, риба, яйця
    'Beef', 'Chicken Egg', 'Cow Trotters', 'Duck Egg',
    'Egg', 'Fish', 'Intestines',

    # Спеції та приправи
    'Bay Leaf', 'Coriander Seeds', 'Cumin Seed', 'Galangal',
    'Garlic', 'Ginger', 'Green Chilli', 'Green Paprika', 'Turmeric',

    # Молочні продукти та інші інгредієнти
    'Cheese', 'Coconut Milk Powder', 'Grated Coconut',
    'Honey', 'Wheat', 'Wheat Flour', 'Yogurt', 'Bread'
]
    YOLO_CONFIDENCE = 0.25
    IMG_SIZE = 224

    # --- Active Learning Settings ---
    ENABLE_ACTIVE_LEARNING = True
    UNCERTAINTY_THRESHOLD = 0.65  # Якщо впевненість менша за це — зберігаємо для навчання

    # --- Redis ---
    REDIS_HOST = "localhost"
    REDIS_PORT = 6379
    REDIS_DB = 0

    @classmethod
    def setup_directories(cls):
        for d in [cls.UPLOAD_DIR, cls.UNCERTAIN_DIR, cls.LABELED_DIR]:
            os.makedirs(d, exist_ok=True)


Config.setup_directories()