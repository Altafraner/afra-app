
FROM python:3.11-slim

# Set env vars
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Create app directory
WORKDIR /app

# Install deps
COPY requirements.txt /app/requirements.txt
RUN pip install --no-cache-dir -r /app/requirements.txt

# Create data dir for persistence
RUN mkdir -p /data
VOLUME ["/data"]

# Copy app
COPY app /app/app

# Default env (override in compose)
ENV FILE_DIR=/data
ENV BASIC_AUTH_USER=admin
ENV BASIC_AUTH_PASS=admin
ENV RETENTION_SECONDS=86400
ENV SWEEP_INTERVAL_SECONDS=300

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
