import uuid
import cv2
import numpy as np
from fastapi import FastAPI, File, UploadFile, HTTPException, Depends
from PydanticModels import PredictionResponse, FeedbackRequest, FeedbackResponse
from ModelService import ModelService
from ActiveLearningService import ActiveLearningService
from CONFIG import Config


# --- Dependency Container ---
class Container:
    def __init__(self):
        self.model_service = ModelService()
        self.al_service = ActiveLearningService()


container = Container()

app = FastAPI(title="Food Recognition Microservice", version="2.0.0")


# --- Dependencies ---
def get_model_service():
    return container.model_service


def get_al_service():
    return container.al_service


# --- Endpoints ---

@app.post("/predict", response_model=PredictionResponse, tags=["Inference"])
async def predict(
        file: UploadFile = File(...),
        model_service: ModelService = Depends(get_model_service),
        al_service: ActiveLearningService = Depends(get_al_service)
):
    # 1. Read Image
    contents = await file.read()
    nparr = np.frombuffer(contents, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    request_id = str(uuid.uuid4())

    # 2. Get Predictions (Raw)
    raw_results = model_service.predict_pipeline(image)

    # 3. Process Active Learning (Check confidence)
    processed_results = []
    al_triggered = False

    for res in raw_results:
        processed = al_service.process_uncertainty(request_id, res)
        if processed['status'] == 'uncertain':
            al_triggered = True
        processed_results.append(processed)

    return {
        "request_id": request_id,
        "total_objects": len(processed_results),
        "active_learning_triggered": al_triggered,
        "results": processed_results
    }


@app.post("/feedback", response_model=FeedbackResponse, tags=["Active Learning"])
async def feedback(
        request: FeedbackRequest,
        al_service: ActiveLearningService = Depends(get_al_service)
):
    try:
        al_service.handle_feedback(request.image_id, request.correct_label)
        return {"status": "success", "message": "Model retrained (simulated) & data moved."}
    except FileNotFoundError:
        raise HTTPException(status_code=404, detail="Image not found in uncertainty pool")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8000)