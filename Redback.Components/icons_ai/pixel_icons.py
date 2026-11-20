"""Redback pixel-art icons, made with Gemini (Nano Banana). Adapted from the ai-icons kit.

Differences from the kit:
  - Blue key (icons.json "key") rather than green, so icons can use green (trees etc.). Keep pure blue out of icons.
  - Gemini gets a grid template (grid.png: (size - 2*margin)^2 cells, two-tone key-colour checker) to draw on.
  - Build votes one colour per grid cell from the palette (no smooth resize), gives binary alpha, and pads with a
    transparent `margin` border so nothing touches the 24x24 edge.

Usage (run from Redback.Components, Python 3 + pillow + numpy):
  python icons_ai/pixel_icons.py grid                     -> icons_ai/grid.png (upload it to Gemini with the prompt)
  python icons_ai/pixel_icons.py prompt "<what to draw>"  -> prints the Gemini prompt
  python icons_ai/pixel_icons.py ingest <name>            -> newest ~/Downloads/Gemini_Generated_Image*.(png|jpg|jpeg)
                                                             -> icons_ai/raw/<name>.png
  python icons_ai/pixel_icons.py build <name>[:badge] ... -> icons_ai/review/<name>.png, <name>_x8.png, review_sheet.png
  python icons_ai/pixel_icons.py approve <name> ...       -> copies reviewed icons into Resources/
"""
import glob, json, os, shutil, sys
import numpy as np
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
CFG = json.load(open(os.path.join(ROOT, "icons.json"), encoding="utf-8"))
SIZE, MARGIN = CFG["size"], CFG["margin"]
INNER = SIZE - 2 * MARGIN
WORK = os.path.join(ROOT, CFG["work_dir"])
OUT = os.path.join(ROOT, CFG["output_dir"])


def rgb(h):
    return np.array([int(h[k:k + 2], 16) for k in (1, 3, 5)], dtype=np.float32)


KEY = rgb(CFG["key"])
NAMES = list(CFG["palette"])
COLS = np.stack([rgb(v) for v in CFG["palette"].values()])


def grid():
    cell = 44
    s = cell * INNER
    a = np.zeros((s, s, 3), np.uint8)
    a[:] = KEY
    yy, xx = np.mgrid[0:s, 0:s]
    a[((yy // cell) + (xx // cell)) % 2 == 1] = (KEY * 0.86).astype(np.uint8)
    p = os.path.join(WORK, "grid.png")
    Image.fromarray(a).save(p)
    print(p, f"{INNER}x{INNER} cells of {cell}px")


SMOOTH = CFG.get("mode") == "smooth"


def prompt(desc):
    if SMOOTH:
        print(
            f"Generate an image: {desc.strip().rstrip('.')}. {CFG['style']}. "
            f"Colours drawn from: {', '.join(f'{n} {v}' for n, v in CFG['palette'].items())}. "
            "A single icon, centred, filling most of the canvas with a small even margin. "
            "Do not use blue anywhere in the icon itself. No drop shadow or glow on the background, no frame or tile. "
            f"Background: solid flat pure blue {CFG['key']} filling the whole square canvas. Square 1:1."
        )
        return
    print(
        f"Using the attached image as the canvas: it is a {INNER}x{INNER} grid of blue checker cells. "
        f"Draw pixel art of {desc.strip().rstrip('.')} on it, filling whole cells only, exactly one flat colour per "
        f"cell, keeping the grid size and alignment and the exact same canvas size. {CFG['style']}. "
        f"Palette only: {', '.join(f'{n} {v}' for n, v in CFG['palette'].items())}. "
        "No anti-aliasing, gradients, dithering, shadows or glow. Do not use blue anywhere in the icon. "
        "Leave every unused cell exactly as the original blue checker. Centre the icon and use most of the grid. "
        "Output a square image."
    )


def ingest(name):
    dl = os.path.join(os.path.expanduser("~"), "Downloads")
    import time
    fs = [f for e in ("png", "jpg", "jpeg") for f in glob.glob(os.path.join(dl, f"Gemini_Generated_Image*.{e}"))]
    fs = [f for f in fs if time.time() - os.path.getmtime(f) < 600]   # never grab an old, unrelated download
    if not fs:
        sys.exit("no Gemini_Generated_Image* from the last 10 minutes in Downloads (download not finished?)")
    # oldest fresh file first, so several downloads clicked in order are ingested in that order
    f = min(fs, key=os.path.getmtime) if os.environ.get("INGEST_OLDEST") else max(fs, key=os.path.getmtime)
    raw = os.path.join(WORK, "raw")
    os.makedirs(os.path.join(raw, "originals"), exist_ok=True)
    Image.open(f).convert("RGB").save(os.path.join(raw, name + ".png"))
    shutil.move(f, os.path.join(raw, "originals", name + os.path.splitext(f)[1]))   # move, never delete
    print("ingested", name, "from", os.path.basename(f))


def labels(img):
    """-1 for key-colour background, else palette index."""
    keyness = (img[..., 2] - np.maximum(img[..., 0], img[..., 1])) / max(KEY[2] - max(KEY[0], KEY[1]), 1)
    lab = ((img[..., None, :] - COLS[None, None]) ** 2).sum(-1).argmin(-1)
    return np.where(keyness > 0.5, -1, lab)


def vote(block):
    """Majority colour of a cell (the style has no outlines, so no colour gets priority)."""
    v, n = np.unique(block, return_counts=True)
    return v[n.argmax()]


BADGES = {   # hand-drawn 7x7 badges, bottom-right inside the border. M magenta, D dark magenta, W white, . clear
    "list": ["MMMMMMM",
             "MWMWWWM",
             "MMMMMMM",
             "MWMWWWM",
             "MMMMMMM",
             "MWMWWWM",
             "MMMMMMM"],
}
BADGE_COLS = {"M": "magenta", "D": "darkmagenta", "W": "white"}


def apply_badge(out, badge):
    rows = BADGES[badge]
    bh, bw = len(rows), len(rows[0])
    ox, oy = SIZE - MARGIN - bw, SIZE - MARGIN - bh
    # clear a 1-cell gap above and left of the badge so it separates from the subject without an outline
    out[oy - 1, ox - 1:ox + bw, 3] = 0
    out[oy - 1:oy + bh, ox - 1, 3] = 0
    for j, row in enumerate(rows):
        for i, ch in enumerate(row):
            if ch == ".":
                continue
            out[oy + j, ox + i, :3] = COLS[NAMES.index(BADGE_COLS[ch])].astype(np.uint8)
            out[oy + j, ox + i, 3] = 255


def key_blue(img):
    """Kit's green keyer, for a blue key: alpha + un-mixed colour (anti-aliased edges lose the blue fringe)."""
    h, w = img.shape[:2]
    k = max(4, min(h, w) // 64)
    bg = np.median(np.concatenate([img[:k, :k], img[:k, -k:], img[-k:, :k], img[-k:, -k:]]).reshape(-1, 3), 0)
    full = max(bg[2] - max(bg[0], bg[1]), 1.0)
    spill = np.clip((img[..., 2] - np.maximum(img[..., 0], img[..., 1])) / full, 0, 1)
    alpha = np.clip((1.0 - spill - 0.12) / 0.80, 0, 1)
    a = np.maximum(alpha, 1e-3)[..., None]
    return alpha, np.clip((img - (1 - a) * bg) / a, 0, 255)


def build_smooth(name, badge=None):
    img = np.asarray(Image.open(os.path.join(WORK, "raw", name + ".png")).convert("RGB"), np.float32)
    alpha, fg = key_blue(img)
    im = Image.fromarray(np.dstack([fg, alpha * 255]).astype(np.uint8), "RGBA")
    box = Image.fromarray((alpha > 0.1).astype(np.uint8) * 255).getbbox()
    im = im.crop(box)
    w, h = im.size
    side = max(w, h)
    sq = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    sq.paste(im, ((side - w) // 2, (side - h) // 2))
    # premultiplied resize so transparent edges don't pull in dark colour
    sq = sq.convert("RGBa").resize((INNER, INNER), Image.LANCZOS).convert("RGBA")
    out = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    out.paste(sq, (MARGIN, MARGIN))
    arr = np.asarray(out).copy()
    if badge:
        apply_badge(arr, badge)
    os.makedirs(os.path.join(WORK, "review"), exist_ok=True)
    out = Image.fromarray(arr, "RGBA")
    out.save(os.path.join(WORK, "review", name + ".png"))
    out.resize((SIZE * 8,) * 2, Image.NEAREST).save(os.path.join(WORK, "review", name + "_x8.png"))
    print(f"{name}: smooth build from {img.shape[1]}x{img.shape[0]}")


def build(name, badge=None):
    if SMOOTH:
        return build_smooth(name, badge)
    img = np.asarray(Image.open(os.path.join(WORK, "raw", name + ".png")).convert("RGB"), np.float32)
    lab = labels(img)
    h, w = lab.shape
    # Gemini keeps the template's grid if it behaved: cut the whole canvas into INNER x INNER cells
    ye = np.linspace(0, h, INNER + 1).round().astype(int)
    xe = np.linspace(0, w, INNER + 1).round().astype(int)
    g = np.array([[vote(lab[ye[r]:ye[r + 1], xe[c]:xe[c + 1]].ravel()) for c in range(INNER)] for r in range(INNER)])
    # alignment check: how uniform each cell's centre region is (1.0 = perfectly on grid)
    pure = []
    for r in range(INNER):
        for c in range(INNER):
            y0, y1, x0, x1 = ye[r], ye[r + 1], xe[c], xe[c + 1]
            m = lab[y0 + (y1 - y0) // 4:y1 - (y1 - y0) // 4, x0 + (x1 - x0) // 4:x1 - (x1 - x0) // 4].ravel()
            if (g[r, c] >= 0) and m.size:
                pure.append((m == g[r, c]).mean())
    print(f"{name}: {w}x{h} raw, grid fit {np.mean(pure):.2f} (below ~0.85 means Gemini drifted off the grid)")
    edge = np.concatenate([g[0], g[-1], g[:, 0], g[:, -1]])
    if (edge >= 0).any():
        print("  note: icon touches the drawable edge (still inside the 1px transparent border)")

    out = np.zeros((SIZE, SIZE, 4), np.uint8)
    sub = out[MARGIN:MARGIN + INNER, MARGIN:MARGIN + INNER]
    m = g >= 0
    sub[m, :3] = COLS[g[m]].astype(np.uint8)
    sub[m, 3] = 255
    if badge:
        apply_badge(out, badge)
    os.makedirs(os.path.join(WORK, "review"), exist_ok=True)
    im = Image.fromarray(out, "RGBA")
    im.save(os.path.join(WORK, "review", name + ".png"))
    im.resize((SIZE * 8,) * 2, Image.NEAREST).save(os.path.join(WORK, "review", name + "_x8.png"))


def sheet(names):
    """Per icon: current | new, on light and dark, at 4x / 2x / 1x (nearest)."""
    scales = (4, 2, 1)
    cw = sum(SIZE * k + 8 for k in scales) + 8
    rh = SIZE * 4 + 32
    img = Image.new("RGB", (cw * 4, rh * len(names)), "#888888")
    d = ImageDraw.Draw(img)
    for row, n in enumerate(names):
        y = row * rh
        d.text((4, y + 2), n + "   current | new   on light, then dark", fill="#000")
        srcs = [os.path.join(OUT, n + ".png"), os.path.join(WORK, "review", n + ".png")]
        for ci, (bg, src) in enumerate([(b, s) for b in ("#d4d4d4", "#2b2b2b") for s in srcs]):
            x0 = ci * cw
            d.rectangle([x0 + 2, y + 16, x0 + cw - 2, y + rh - 2], fill=bg)
            if os.path.exists(src):
                ic = Image.open(src).convert("RGBA")
                x = x0 + 8
                for k in scales:
                    b = ic.resize((SIZE * k,) * 2, Image.NEAREST)
                    img.paste(b, (x, y + 22 + (SIZE * 4 - SIZE * k) // 2), b)
                    x += SIZE * k + 8
    p = os.path.join(WORK, "review", "review_sheet.png")
    img.save(p)
    print("sheet", p)


def main(a):
    cmd = a[0] if a else ""
    if cmd == "grid":
        grid()
    elif cmd == "prompt":
        prompt(a[1])
    elif cmd == "ingest":
        ingest(a[1])
    elif cmd == "build":
        names = [x.split(":")[0] for x in a[1:]]
        for x in a[1:]:                       # name or name:badge, e.g. Icon-List2Tree:list
            n, _, b = x.partition(":")
            build(n, b or None)
        sheet(names)
    elif cmd == "approve":
        for n in a[1:]:
            shutil.copy2(os.path.join(WORK, "review", n + ".png"), os.path.join(OUT, n + ".png"))
            print("approved", n)
    else:
        print(__doc__)


if __name__ == "__main__":
    main(sys.argv[1:])
