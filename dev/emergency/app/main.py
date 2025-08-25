
import os
import time
from datetime import datetime, timezone
from pathlib import Path
from typing import Optional
from zoneinfo import ZoneInfo

from fastapi import FastAPI, File, UploadFile, Depends, HTTPException, Query
from fastapi.responses import HTMLResponse, FileResponse, JSONResponse
from fastapi.security import HTTPBasic, HTTPBasicCredentials
from starlette.status import HTTP_401_UNAUTHORIZED, HTTP_415_UNSUPPORTED_MEDIA_TYPE
import secrets
import asyncio

app = FastAPI(title="HTML Ingest Service", version="1.0.2")

security = HTTPBasic()

# Configuration via environment variables
STORAGE_DIR = Path(os.getenv("FILE_DIR", "/data"))
STORAGE_DIR.mkdir(parents=True, exist_ok=True)

USERNAME = os.getenv("BASIC_AUTH_USER", "admin")
PASSWORD = os.getenv("BASIC_AUTH_PASS", "admin")
RETENTION_SECONDS = int(os.getenv("RETENTION_SECONDS", str(24 * 3600)))
SWEEP_INTERVAL_SECONDS = int(os.getenv("SWEEP_INTERVAL_SECONDS", "60"))  # default 60s for tighter enforcement

# Timezone and formatter for display
TZ = ZoneInfo("Europe/Berlin")
def _format_dt(ts: float) -> str:
    return datetime.fromtimestamp(ts, tz=TZ).strftime("%d.%m.%Y %H:%M")

def _auth(credentials: HTTPBasicCredentials = Depends(security)):
    correct_username = secrets.compare_digest(credentials.username, USERNAME)
    correct_password = secrets.compare_digest(credentials.password, PASSWORD)
    if not (correct_username and correct_password):
        raise HTTPException(
            status_code=HTTP_401_UNAUTHORIZED,
            detail="Invalid authentication credentials",
            headers={"WWW-Authenticate": "Basic"},
        )
    return credentials.username

def _now_ts() -> float:
    # Store mtimes in UTC; display in Europe/Berlin
    return datetime.now(timezone.utc).timestamp()

def _is_html(upload: UploadFile) -> bool:
    ct = (upload.content_type or "").lower()
    if ct.startswith("text/html") or ct == "application/xhtml+xml":
        return True
    name = (upload.filename or "").lower()
    return name.endswith(".html") or name.endswith(".htm")

def _safe_name(filename: str) -> str:
    return Path(filename).name

def sweep_expired() -> int:
    """Delete files older than RETENTION_SECONDS. Returns count deleted."""
    deleted = 0
    threshold = _now_ts() - RETENTION_SECONDS
    for p in STORAGE_DIR.iterdir():
        if p.is_file():
            try:
                mtime = p.stat().st_mtime
                if mtime < threshold:
                    p.unlink(missing_ok=True)
                    deleted += 1
            except FileNotFoundError:
                pass
    return deleted

async def sweep_loop():
    """Run a periodic sweeper so files expire even if there are no requests."""
    while True:
        try:
            sweep_expired()
        finally:
            await asyncio.sleep(SWEEP_INTERVAL_SECONDS)

@app.on_event("startup")
async def on_startup():
    # Sweep once at startup to handle files that aged while service was down
    STORAGE_DIR.mkdir(parents=True, exist_ok=True)
    sweep_expired()
    # Start background sweeper
    asyncio.create_task(sweep_loop())

def _recent_files() -> list[dict]:
    now = _now_ts()
    files = []
    for p in STORAGE_DIR.iterdir():
        if p.is_file():
            st = p.stat()
            age = now - st.st_mtime
            if age <= RETENTION_SECONDS:
                files.append({
                    "name": p.name,
                    "size": st.st_size,
                    "mtime": int(st.st_mtime),
                    "age_seconds": int(age),
                    "url": f"/files/{p.name}"
                })
    files.sort(key=lambda x: x["mtime"], reverse=True)
    return files

def _render_list(items: list[dict]) -> HTMLResponse:
    rows = "".join([
        f'<tr><td><a href="{it["url"]}">{it["name"]}</a></td><td>{it["size"]}</td>'
        f'<td>{_format_dt(it["mtime"])}</td></tr>'
        for it in items
    ])
    html = f"""
    <html>
      <head><title>Afra-App Notfallübersicht</title></head>
      <body style="font-family: sans-serif; max-width: 900px; margin: 2rem auto;">
        <h1>Afra-App Notfallübersicht</h1>
        <table border="1" cellpadding="6" cellspacing="0">
          <thead><tr><th>Name</th><th>Größe (bytes)</th><th>Zuletzt geändert (TZ: Europe/Berlin)</th></tr></thead>
          <tbody>{rows or '<tr><td colspan="3"><i>Nichts vorhanden</i></td></tr>'}</tbody>
        </table>
      </body>
    </html>
    """
    return HTMLResponse(html)

@app.get("/", response_class=HTMLResponse)
async def root(user: str = Depends(_auth)):
    sweep_expired()
    items = _recent_files()
    return _render_list(items)

@app.get("/files")
async def list_files(format: Optional[str] = Query(default="html"), user: str = Depends(_auth)):
    sweep_expired()
    items = _recent_files()
    if (format or "").lower() == "json":
        return JSONResponse(items)
    return _render_list(items)

@app.get("/files/{name}")
async def get_file(name: str, user: str = Depends(_auth)):
    safe = _safe_name(name)
    path = STORAGE_DIR / safe
    if not path.exists() or not path.is_file():
        raise HTTPException(status_code=404, detail="File not found")

    age = _now_ts() - path.stat().st_mtime
    if age > RETENTION_SECONDS:
        try:
            path.unlink(missing_ok=True)
        except FileNotFoundError:
            pass
        raise HTTPException(status_code=404, detail="File expired")

    return FileResponse(path, media_type="text/html", headers={"Content-Disposition": f'inline; filename="{safe}"'})

@app.post("/upload")
async def upload(file: UploadFile = File(...), user: str = Depends(_auth)):
    if not _is_html(file):
        raise HTTPException(status_code=HTTP_415_UNSUPPORTED_MEDIA_TYPE, detail="Only HTML files are accepted")
    if not file.filename:
        raise HTTPException(status_code=400, detail="Missing filename")

    filename = _safe_name(file.filename)
    target = STORAGE_DIR / filename

    content = await file.read()
    with open(target, "wb") as f:
        f.write(content)

    # Set mtime to now (UTC); display uses Europe/Berlin
    now_ts = _now_ts()
    os.utime(target, (now_ts, now_ts))

    sweep_expired()

    return JSONResponse({"status": "ok", "filename": filename, "size": len(content)})
