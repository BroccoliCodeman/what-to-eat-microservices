import os
import cv2
import json
import uuid
from CONFIG import Config


class ActiveLearningService:
    def __init__(self):
        # Тут можна додати підключення до Redis, якщо потрібно
        pass

    def process_uncertainty(self, request_id: str, detection_result: dict) -> dict:
        """
        Перевіряє впевненість. Якщо низька -> зберігає кроп на диск.
        Повертає оновлений словник із статусом.
        """
        conf = detection_result['confidence']
        crop_img = detection_result.pop('crop_img')  # Забираємо картинку з результату

        if conf < Config.UNCERTAINTY_THRESHOLD:
            # Логіка збереження для AL
            filename = f"{request_id}_obj_{detection_result['object_id']}.jpg"
            save_path = os.path.join(Config.UNCERTAIN_DIR, filename)

            # Конвертуємо назад в BGR для збереження через cv2
            cv2.imwrite(save_path, cv2.cvtColor(crop_img, cv2.COLOR_RGB2BGR))

            detection_result['status'] = 'uncertain'
            detection_result['crop_ref'] = filename
        else:
            detection_result['status'] = 'confident'
            detection_result['crop_ref'] = None

        return detection_result

    def handle_feedback(self, image_id: str, correct_label: str):
        """Переміщує файл з uncertain в labeled"""
        source = os.path.join(Config.UNCERTAIN_DIR, image_id)
        if not os.path.exists(source):
            raise FileNotFoundError("Image not found")

        # Формуємо нове ім'я: ClassName_UUID.jpg
        new_name = f"{correct_label}_{image_id}"
        dest = os.path.join(Config.LABELED_DIR, new_name)

        os.rename(source, dest)

        # Зберігаємо метадані
        meta = {"original_id": image_id, "label": correct_label, "timestamp": str(uuid.uuid4())}
        with open(dest + ".json", "w") as f:
            json.dump(meta, f)

        return True