from typing import List, Optional
from pydantic import BaseModel, Field

class DetectedObject(BaseModel):
    object_id: int
    bbox: List[float] = Field(..., description="[x1, y1, x2, y2]")
    prediction: str
    confidence: float
    status: str = Field(..., description="'confident' or 'uncertain'")
    crop_ref: Optional[str] = Field(None, description="Filename of the crop if uncertain")

class PredictionResponse(BaseModel):
    request_id: str
    total_objects: int
    active_learning_triggered: bool
    results: List[DetectedObject]

class FeedbackRequest(BaseModel):
    image_id: str = Field(..., description="Filename from uncertain folder")
    correct_label: str = Field(..., description="Correct class name")

class FeedbackResponse(BaseModel):
    status: str
    message: str