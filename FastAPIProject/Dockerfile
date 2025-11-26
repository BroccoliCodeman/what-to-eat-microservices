FROM python:3.12.7

# Set the working directory
WORKDIR /app

# Install dependencies
RUN pip install --no-cache-dir fastapi uvicorn python-multipart torch torchvision ultralytics opencv-python-headless albumentations numpy Pillow


# Copy the rest of the code
COPY . .

# Open the port on which FastAPI will run
EXPOSE 8000

# Run FastAPI with Uvicorn
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
