import torch
import cv2
import numpy as np
import albumentations as A
from albumentations.pytorch import ToTensorV2
from ultralytics import YOLO
from FoodClassifier import FoodClassifier
from CONFIG import Config


class ModelService:
    def __init__(self):
        self.device = 'cuda' if torch.cuda.is_available() else 'cpu'
        print(f"🚀 Initializing ModelService on {self.device}...")

        # 1. Load YOLO
        self.yolo = YOLO(Config.YOLO_MODEL_PATH)

        # 2. Load Classifier
        self.classifier = FoodClassifier(num_classes=len(Config.CLASS_NAMES))
        self._load_classifier_weights()
        self.classifier.to(self.device)
        self.classifier.eval()

        # 3. Preprocessing
        self.transform = A.Compose([
            A.Resize(height=Config.IMG_SIZE, width=Config.IMG_SIZE),
            A.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
            ToTensorV2()
        ])

    def _load_classifier_weights(self):
        try:
            checkpoint = torch.load(Config.CLASSIFIER_MODEL_PATH, map_location=self.device)
            state_dict = checkpoint['model_state_dict'] if 'model_state_dict' in checkpoint else checkpoint
            self.classifier.load_state_dict(state_dict)
            print("✅ Classifier weights loaded.")
        except FileNotFoundError:
            print("⚠️ Weights not found. Using random init (for testing).")

    def predict_pipeline(self, image: np.ndarray) -> list:
        """Повний цикл: Детекція -> Кроп -> Класифікація"""
        image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        # A. YOLO Detection
        yolo_results = self.yolo(image_rgb, conf=Config.YOLO_CONFIDENCE, verbose=False)[0]
        final_results = []

        if yolo_results.boxes is None:
            return []

        boxes = yolo_results.boxes.xyxy.cpu().numpy()

        for i, box in enumerate(boxes):
            # B. Crop
            crop = self._crop_object(image_rgb, box)
            if crop.size == 0: continue

            # C. Classify
            pred_class, conf = self._classify_crop(crop)

            final_results.append({
                "object_id": i,
                "bbox": box.tolist(),
                "prediction": pred_class,
                "confidence": conf,
                "crop_img": crop  # Повертаємо саме зображення для AL сервісу
            })

        return final_results

    def _classify_crop(self, crop: np.ndarray):
        transformed = self.transform(image=crop)
        img_tensor = transformed['image'].unsqueeze(0).to(self.device)

        with torch.no_grad():
            outputs = self.classifier(img_tensor)
            probs = torch.softmax(outputs, dim=1)[0]

        conf, idx = torch.max(probs, 0)
        return Config.CLASS_NAMES[idx.item()], float(conf.item())

    def _crop_object(self, image, bbox, padding=10):
        h, w = image.shape[:2]
        x1, y1, x2, y2 = map(int, bbox)
        x1 = max(0, x1 - padding);
        y1 = max(0, y1 - padding)
        x2 = min(w, x2 + padding);
        y2 = min(h, y2 + padding)
        return image[y1:y2, x1:x2]