import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from torchvision import transforms
from PIL import Image
import os
import shutil
import asyncio
from datetime import datetime
from concurrent.futures import ThreadPoolExecutor
from CONFIG import Config


class FoodRetrainDataset(Dataset):
    """Dataset для донавчання на кропах"""

    def __init__(self, data_list, transform=None):
        self.data = data_list  # list of {'path': str, 'label_idx': int}
        self.transform = transform

    def __len__(self):
        return len(self.data)

    def __getitem__(self, idx):
        item = self.data[idx]
        img = Image.open(item['path']).convert('RGB')
        if self.transform:
            img = self.transform(img)
        return img, item['label_idx']


class RetrainService:
    def __init__(self, redis_client, logger, model_service):
        self.redis = redis_client
        self.logger = logger
        self.model_service = model_service
        self.executor = ThreadPoolExecutor(max_workers=1)
        self.is_training = False

        # Transformation for training (Augmentation)
        self.train_transform = transforms.Compose([
            transforms.Resize((Config.IMG_SIZE, Config.IMG_SIZE)),
            transforms.RandomHorizontalFlip(),
            transforms.RandomRotation(15),
            transforms.ColorJitter(brightness=0.2, contrast=0.2),
            transforms.ToTensor(),
            transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
        ])

    async def trigger_retrain(self, learning_data):
        if self.is_training:
            self.logger.info("retrain_skip_already_running")
            return {"status": "skipped"}

        self.is_training = True
        loop = asyncio.get_event_loop()

        # Запуск в окремому потоці
        loop.run_in_executor(self.executor, self._background_retrain, learning_data)
        return {"status": "started"}

    def _background_retrain(self, learning_data):
        self.logger.info("retrain_started", samples=len(learning_data))
        try:
            # 1. Prepare Data
            train_dataset = self._prepare_dataset(learning_data)
            if len(train_dataset) < Config.MIN_RETRAIN_SAMPLES:
                self.logger.info("retrain_aborted_insufficient_data")
                return

            # 2. Backup Current Model
            self._backup_model()

            # 3. Validation Baseline (Check accuracy BEFORE training)
            baseline_acc = self._validate_on_baseline(self.model_service.classifier)

            # 4. Training Loop (Fine-tuning)
            # Clone model structure to avoid modifying the live one directly during training steps
            # (In simplified version we use the live one but ideally should clone)
            model = self.model_service.classifier
            model.train()

            # Freeze backbone, train head
            for param in model.backbone.parameters():
                param.requires_grad = False
            for param in model.backbone.classifier.parameters():
                param.requires_grad = True

            optimizer = optim.Adam(model.backbone.classifier.parameters(), lr=Config.RETRAIN_LR)
            criterion = nn.CrossEntropyLoss()
            loader = DataLoader(train_dataset, batch_size=8, shuffle=True)

            for epoch in range(Config.RETRAIN_EPOCHS):
                total_loss = 0
                for imgs, labels in loader:
                    imgs, labels = imgs.to(self.model_service.device), labels.to(self.model_service.device)
                    optimizer.zero_grad()
                    outputs = model(imgs)
                    loss = criterion(outputs, labels)
                    loss.backward()
                    optimizer.step()
                    total_loss += loss.item()
                self.logger.info(f"epoch_{epoch + 1}_completed", loss=total_loss)

            # 5. Safety Check (Validation AFTER training)
            model.eval()
            new_acc = self._validate_on_baseline(model)

            degradation = baseline_acc - new_acc
            if degradation > Config.MAX_BASELINE_DEGRADATION:
                self.logger.warning("retrain_rejected_degradation",
                                    baseline=baseline_acc, new=new_acc, drop=degradation)
                self._restore_backup()
            else:
                self.logger.info("retrain_success_saving", new_accuracy=new_acc)
                self._save_model(model)
                # Hot swap
                self.model_service.reload_model()
                # Clear used data from queue (in real app)

        except Exception as e:
            self.logger.error("retrain_critical_failure", error=str(e))
            self._restore_backup()
        finally:
            self.is_training = False

    def _prepare_dataset(self, raw_data):
        # raw_data = list of {'image_path': str, 'label': str}
        processed = []
        class_to_idx = {name: i for i, name in enumerate(Config.CLASS_NAMES)}

        for item in raw_data:
            if item['label'] in class_to_idx and os.path.exists(item['image_path']):
                processed.append({
                    'path': item['image_path'],
                    'label_idx': class_to_idx[item['label']]
                })
        return FoodRetrainDataset(processed, transform=self.train_transform)

    def _validate_on_baseline(self, model):
        # Реалізація швидкої перевірки на "золотому стандарті" даних
        # Тут треба завантажити фіксовані 50 фото з папки baseline
        # Повертає accuracy (0.0 - 1.0)
        return 0.95  # Stub

    def _backup_model(self):
        shutil.copy(Config.CLASSIFIER_MODEL_PATH, Config.CLASSIFIER_MODEL_PATH + ".bak")

    def _restore_backup(self):
        shutil.copy(Config.CLASSIFIER_MODEL_PATH + ".bak", Config.CLASSIFIER_MODEL_PATH)
        self.model_service.reload_model()

    def _save_model(self, model):
        torch.save({
            'model_state_dict': model.state_dict(),
            'classes': Config.CLASS_NAMES
        }, Config.CLASSIFIER_MODEL_PATH)