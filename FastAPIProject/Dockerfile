# Використовуємо slim версію
FROM python:3.10-slim

WORKDIR /app

# Змінні оточення
ENV PYTHONUNBUFFERED=1 \
    OPENCV_VIDEOIO_PRIORITY_MSMF=0 \
    PIP_DEFAULT_TIMEOUT=100

# 1. Спочатку встановлюємо найважчі бібліотеки окремим шаром.
# Це створить закешований шар. Якщо ви зміните requirements.txt (додасте дрібну лібу),
# цей шар не буде перезбиратися, і Torch не буде качатися заново.
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install torch torchvision ultralytics --extra-index-url https://download.pytorch.org/whl/cpu

# 2. Копіюємо лише файл залежностей
COPY requirements.txt .

# 3. Встановлюємо решту залежностей
# Використовуємо кеш pip, щоб не качати те, що вже є
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install -r requirements.txt

# 4. І лише тепер копіюємо код.
# Це найчастіша зміна, тому вона має бути в кінці.
COPY . .

# Створюємо папки
RUN mkdir -p data/uploads \
    data/active_learning/uncertain \
    data/active_learning/labeled

EXPOSE 8000

# Запуск
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000", "--workers", "1"]