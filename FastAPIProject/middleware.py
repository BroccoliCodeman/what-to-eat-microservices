import time
import uuid
from fastapi import Request, Response
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.types import Message
import structlog

logger = structlog.get_logger()


class RequestLoggingMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        request_id = str(uuid.uuid4())
        start_time = time.time()

        # Контекстний логер з ID запиту (щоб зв'язати вхід і вихід)
        log = logger.bind(request_id=request_id)

        # 1. Логування ВХІДНИХ даних (Request)
        path = request.url.path
        method = request.method
        client_ip = request.client.host

        # Спроба отримати тіло запиту (якщо це JSON)
        # Для multipart/form-data (файли) ми не читаємо тіло тут, бо це заблокує потік
        body_log = "binary/multipart"
        content_type = request.headers.get("content-type", "")

        if "application/json" in content_type:
            try:
                # Читаємо тіло безпечно
                body_bytes = await request.body()
                body_log = body_bytes.decode("utf-8")

                # ВАЖЛИВО: Відновлюємо потік, щоб FastAPI міг його прочитати знову
                async def receive() -> Message:
                    return {"type": "http.request", "body": body_bytes}

                request._receive = receive
            except Exception:
                body_log = "error_reading_body"

        log.info(
            "http_request_started",
            method=method,
            path=path,
            client_ip=client_ip,
            content_type=content_type,
            body=body_log  # Тільки JSON тіло, файли ігноруємо
        )

        # 2. Виконання запиту
        try:
            response = await call_next(request)

            # 3. Логування ВИХІДНИХ даних (Response)
            process_time = time.time() - start_time

            log.info(
                "http_request_completed",
                method=method,
                path=path,
                status_code=response.status_code,
                duration=f"{process_time:.4f}s"
            )

            return response

        except Exception as e:
            # 4. Логування ПОМИЛОК (Detailed Crash Report)
            process_time = time.time() - start_time
            log.error(
                "http_request_failed",
                method=method,
                path=path,
                error=str(e),
                duration=f"{process_time:.4f}s",
                exc_info=True  # Це додасть повний Traceback у лог
            )
            raise e