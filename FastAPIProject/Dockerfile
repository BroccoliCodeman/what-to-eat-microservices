FROM python:3.10-slim

# Set working directory
WORKDIR /app

# Install system dependencies required for OpenCV
RUN pip install --no-cache-dir fastapi uvicorn python-multipart torch torchvision ultralytics opencv-python-headless albumentations numpy Pillow


# Copy application code
COPY . .

# Create necessary directories
RUN mkdir -p data/uploads \
    data/active_learning/uncertain \
    data/active_learning/labeled

# Set environment variables
ENV PYTHONUNBUFFERED=1
ENV OPENCV_VIDEOIO_PRIORITY_MSMF=0

# Expose port
EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import requests; requests.get('http://localhost:8000/docs', timeout=5)" || exit 1

# Run the application
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000", "--workers", "1"]