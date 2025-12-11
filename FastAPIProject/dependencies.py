import asyncio
import redis.asyncio as aioredis
from typing import Optional
from ModelService import ModelService
from ActiveLearningService import ActiveLearningService
from RetrainService import RetrainService
from CONFIG import Config
# Імпортуємо нове налаштування
from logger_config import setup_logging


class ServiceContainer:
    def __init__(self):
        self.model_service: Optional[ModelService] = None
        self.al_service: Optional[ActiveLearningService] = None
        self.retrain_service: Optional[RetrainService] = None
        self.redis_client = None
        # Ініціалізуємо логер одразу
        self.logger = setup_logging()
        self.stats = {'predictions': 0, 'retrains': 0}

    async def initialize(self):
        self.logger.info("System startup initiated...")

        # 1. Redis
        await self._init_redis()

        # 2. Model Service
        try:
            self.model_service = ModelService(self.logger)
            self.logger.info("Model Service ready")
        except Exception as e:
            self.logger.error("Model Service failed", error=str(e))

        # 3. Retrain Service
        self.retrain_service = RetrainService(
            self.redis_client, self.logger, self.model_service
        )

        # 4. Active Learning
        self.al_service = ActiveLearningService(
            self.redis_client, self.logger, self.retrain_service
        )

        self.logger.info("System startup completed successfully!")

    async def _init_redis(self):
        try:
            self.redis_client = aioredis.Redis(
                host=Config.REDIS_HOST,
                port=Config.REDIS_PORT,
                decode_responses=True
            )
            await self.redis_client.ping()
            self.logger.info("Redis connected", host=Config.REDIS_HOST)
        except Exception as e:
            self.logger.error("Redis connection failed", error=str(e))
            # Не крашимо додаток, бо можемо працювати без Redis (обмежено)


container = ServiceContainer()