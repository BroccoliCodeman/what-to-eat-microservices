import torch
import torch.nn as nn
from torchvision import models

class FoodClassifier(nn.Module):
    def __init__(self, num_classes):
        super().__init__()
        # Ваги=None, тому що ми завантажуємо свої custom weights
        self.backbone = models.efficientnet_b2(weights=None)
        in_features = self.backbone.classifier[1].in_features
        self.backbone.classifier = nn.Sequential(
            nn.Dropout(p=0.3, inplace=True),
            nn.Linear(in_features, num_classes)
        )

    def forward(self, x):
        return self.backbone(x)