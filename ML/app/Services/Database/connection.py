from sqlalchemy import create_engine
from sqlalchemy.engine import Engine

# PostgreSQL database credentials
DB_USER = "postgres"
DB_PASSWORD = "postgres"
DB_HOST = "localhost"
DB_PORT = "35432"
DB_NAME = "QueryPilot"

# Database connection URL
DATABASE_URL = (
    f"postgresql+psycopg://{DB_USER}:{DB_PASSWORD}"
    f"@{DB_HOST}:{DB_PORT}/{DB_NAME}"
)

# Create SQLAlchemy engine
engine: Engine = create_engine(
    DATABASE_URL,
    pool_pre_ping=True,
    echo=False,
)