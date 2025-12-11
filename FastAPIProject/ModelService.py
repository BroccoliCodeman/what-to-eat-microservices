import torch
import cv2
import numpy as np
import albumentations as A
from albumentations.pytorch import ToTensorV2
from ultralytics import YOLO
from FoodClassifier import FoodClassifier
from CONFIG import Config


class ModelService:
    def __init__(self, logger):
        self.logger = logger
        self.device = 'cuda' if torch.cuda.is_available() else 'cpu'
        self.model_version = 1

        # Init components
        self.yolo = YOLO(Config.YOLO_MODEL_PATH)
        self.yolo.to(self.device)

        self.classifier = FoodClassifier(num_classes=len(Config.CLASS_NAMES))
        self.load_weights(Config.CLASSIFIER_MODEL_PATH)
        self.classifier.to(self.device)
        self.classifier.eval()

        self.transform = A.Compose([
            A.Resize(height=Config.IMG_SIZE, width=Config.IMG_SIZE),
            A.CLAHE(clip_limit=2.0, p=0.5),
            A.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
            ToTensorV2()
        ])

    def load_weights(self, path):
        """Безпечне завантаження ваг"""
        try:
            checkpoint = torch.load(path, map_location=self.device)
            state_dict = checkpoint['model_state_dict'] if 'model_state_dict' in checkpoint else checkpoint
            self.classifier.load_state_dict(state_dict)
            self.model_version += 1
            if self.logger:
                self.logger.info("weights_loaded", path=path, version=self.model_version)
        except Exception as e:
            if self.logger:
                self.logger.error("weights_load_failed", error=str(e))
            # Fallback handled by PyTorch initialization (random weights)

    def reload_model(self):
        """Метод для виклику після ретрейнінгу"""
        self.logger.info("hot_swapping_model_weights")
        self.load_weights(Config.CLASSIFIER_MODEL_PATH)
        self.classifier.eval()

    def predict_pipeline(self, image: np.ndarray) -> list:
        # ... (Код інференсу залишається майже без змін, лише додаємо логування) ...
        image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        yolo_results = self.yolo.predict(image_rgb, conf=Config.YOLO_CONFIDENCE, verbose=False)[0]

        if yolo_results.boxes is None: return []

        final_results = []
        boxes = yolo_results.boxes.xyxy.cpu().numpy()

        for i, box in enumerate(boxes):
            crop = self._crop_object(image_rgb, box)
            if not self._is_valid_crop(crop): continue

            pred_class, conf = self._classify_crop(crop)

            status = "uncertain" if conf < Config.UNCERTAINTY_THRESHOLD else "confident"

            final_results.append({
                "object_id": i,
                "bbox": box.tolist(),
                "prediction": pred_class,
                "confidence": conf,
                "status": status,
                "crop_img": crop  # RGB crop
            })

        return final_results

    # ... допоміжні методи (_crop_object, _classify_crop) без змін ...
    def _crop_object(self, image, bbox, padding=15):
        h, w = image.shape[:2]
        x1, y1, x2, y2 = map(int, bbox)
        x1 = max(0, x1 - padding)
        y1 = max(0, y1 - padding)
        x2 = min(w, x2 + padding)
        y2 = min(h, y2 + padding)
        return image[y1:y2, x1:x2]

    def _is_valid_crop(self, crop: np.ndarray) -> bool:
        h, w = crop.shape[:2]
        if h < 32 or w < 32: return False
        if np.mean(crop) < 10: return False
        return True

    def _classify_crop(self, crop: np.ndarray):
        transformed = self.transform(image=crop)
        img_tensor = transformed['image'].unsqueeze(0).to(self.device)
        with torch.no_grad():
            outputs = self.classifier(img_tensor)
            probs = torch.softmax(outputs, dim=1)[0]
        conf, idx = torch.max(probs, 0)
        return Config.CLASS_NAMES[idx.item()], float(conf.item())