import logging
import sys
import structlog


def setup_logging():
    """
    Налаштовує логування так, щоб structlog обробляв і свої логи,
    і логи стандартних бібліотек (uvicorn, fastapi) без конфліктів типів.
    """

    # 1. Спільні процесори (Shared Processors)
    # Вони лише збагачують словник даних, але НЕ перетворюють його в рядок.
    shared_processors = [
        structlog.stdlib.add_log_level,
        structlog.stdlib.add_logger_name,
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.processors.StackInfoRenderer(),
        structlog.processors.format_exc_info,
        structlog.processors.UnicodeDecoder(),
    ]

    # 2. Конфігурація Structlog
    # Використовується, коли ви викликаєте logger.info() у своєму коді.
    structlog.configure(
        processors=shared_processors + [
            # ВАЖЛИВО: Замість рендера ми використовуємо обгортку.
            # Вона передає словник у стандартний logging "як є", не ламаючи типи.
            structlog.stdlib.ProcessorFormatter.wrap_for_formatter,
        ],
        logger_factory=structlog.stdlib.LoggerFactory(),
        wrapper_class=structlog.stdlib.BoundLogger,
        cache_logger_on_first_use=True,
    )

    # 3. Конфігурація Formatter для Standard Library Logging
    # Цей форматер буде обробляти логи від Uvicorn та FastAPI.
    formatter = structlog.stdlib.ProcessorFormatter(
        # Ці процесори запустяться для логів, що прийшли від Uvicorn (foreign logs)
        foreign_pre_chain=shared_processors,

        # А цей рендер запуститься ОСТАННІМ для ВСІХ логів
        processor=structlog.processors.LogfmtRenderer(
            key_order=["timestamp", "level", "event", "request_id"],
            drop_missing=True
        ),
    )

    # 4. Налаштування хендлера
    handler = logging.StreamHandler(sys.stdout)
    handler.setFormatter(formatter)

    # 5. Налаштування кореневого логера
    root_logger = logging.getLogger()
    root_logger.handlers = []  # Видаляємо старі хендлери (щоб уникнути дублювання)
    root_logger.addHandler(handler)
    root_logger.setLevel(logging.INFO)

    # 6. Тюнінг бібліотек
    # Вимикаємо прості текстові логи uvicorn, щоб вони йшли через наш JSON/Logfmt форматер
    logging.getLogger("uvicorn.access").handlers = []
    logging.getLogger("uvicorn.access").propagate = False  # Не дублювати access log

    logging.getLogger("uvicorn.error").handlers = []
    logging.getLogger("uvicorn.error").propagate = True  # Помилки нехай йдуть в root

    # Фільтрація шуму
    logging.getLogger("multipart").setLevel(logging.WARNING)
    logging.getLogger("PIL").setLevel(logging.WARNING)

    return structlog.get_logger()