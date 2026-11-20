#!/bin/sh
# wait (up to ~60s) for a Gemini download from the last 3 minutes, then ingest it as $1
python -P - <<'PY' || exit 1
import glob, os, time, sys
d = os.path.expanduser("~/Downloads")
for _ in range(30):
    fs = [f for f in glob.glob(os.path.join(d, "Gemini_Generated_Image_*")) if not f.endswith(".crdownload")
          and time.time() - os.path.getmtime(f) < 180]
    if fs:
        time.sleep(1); sys.exit(0)
    time.sleep(2)
print("no fresh download"); sys.exit(1)
PY
[ -f "icons_ai/raw/$1.png" ] && mv "icons_ai/raw/$1.png" "icons_ai/raw/$1_prev.png"
INGEST_OLDEST=1 python -P icons_ai/pixel_icons.py ingest "$1"
