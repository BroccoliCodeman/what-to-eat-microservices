from typing import List, Optional
from pydantic import BaseModel, Field

class DetectedObject(BaseModel):
    object_id: int = Field(..., description="Локальний індекс об'єкта на фото (0, 1, 2...)")
    id: str = Field(..., description="Глобальний ID (ім'я файлу) для Active Learning") # <--- НОВЕ ПОЛЕ
    bbox: List[float] = Field(..., description="[x1, y1, x2, y2]")
    prediction: str
    confidence: float
    status: str = Field(..., description="'confident' or 'uncertain'")
    crop_ref: Optional[str] = Field(None, description="Посилання на файл, якщо він збережений")

class PredictionResponse(BaseModel):
    request_id: str
    total_objects: int
    active_learning_triggered: bool
    results: List[DetectedObject]

class FeedbackRequest(BaseModel):
    image_id: str = Field(..., description="ID файлу (співпадає з полем 'id' у DetectedObject)")
    correct_label: str

class FeedbackResponse(BaseModel):
    status: str
    message: str

class PoolItem(BaseModel):
    id: str
    url: str

class PoolResponse(BaseModel):
    count: int
    items: List[PoolItem]

class RetrainResponse(BaseModel):
    status: str
    message: str
    samples_count: int = 0

class ModelStatusResponse(BaseModel):
    status: str
    model_version: str
    uncertain_count: int
    labeled_count: int
    total_classes: int
    device: str