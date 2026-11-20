// Runs inside the Gemini page: takes the newest generated image, keys out the blue background
// (same maths as pixel_icons.key_blue), trims, squares, downsizes in halving steps to 22px and pads to 24px.
// Returns the 24x24 PNG as a data URL. Used when Chrome downloads are unavailable.
const img = [...document.querySelectorAll('img')].filter(i => i.naturalWidth > 300 && i.complete).pop();
if (!img) throw new Error('no generated image yet');
const N = 512, SIZE = 24, MARGIN = 1, INNER = SIZE - 2 * MARGIN;
const c = Object.assign(document.createElement('canvas'), { width: N, height: N });
const x = c.getContext('2d', { willReadFrequently: true });
x.imageSmoothingQuality = 'high';
x.drawImage(img, 0, 0, N, N);
const id = x.getImageData(0, 0, N, N), d = id.data;
const k = 8, cs = [];
for (const [ox, oy] of [[0, 0], [N - k, 0], [0, N - k], [N - k, N - k]])
  for (let yy = oy; yy < oy + k; yy++) for (let xx = ox; xx < ox + k; xx++) { const i = (yy * N + xx) * 4; cs.push([d[i], d[i + 1], d[i + 2]]); }
const med = j => cs.map(p => p[j]).sort((a, b) => a - b)[cs.length >> 1];
const bg = [med(0), med(1), med(2)], full = Math.max(bg[2] - Math.max(bg[0], bg[1]), 1);
let x0 = N, y0 = N, x1 = -1, y1 = -1;
for (let yy = 0; yy < N; yy++) for (let xx = 0; xx < N; xx++) {
  const i = (yy * N + xx) * 4, r = d[i], g = d[i + 1], b = d[i + 2];
  const spill = Math.min(Math.max((b - Math.max(r, g)) / full, 0), 1);
  const a = Math.min(Math.max((1 - spill - 0.12) / 0.8, 0), 1), aa = Math.max(a, 1e-3);
  for (let j = 0; j < 3; j++) d[i + j] = Math.min(Math.max((d[i + j] - (1 - aa) * bg[j]) / aa, 0), 255);
  d[i + 3] = Math.round(a * 255);
  if (a > 0.1) { if (xx < x0) x0 = xx; if (xx > x1) x1 = xx; if (yy < y0) y0 = yy; if (yy > y1) y1 = yy; }
}
x.putImageData(id, 0, 0);
const w = x1 - x0 + 1, h = y1 - y0 + 1, side = Math.max(w, h);
let cur = Object.assign(document.createElement('canvas'), { width: side, height: side });
cur.getContext('2d').drawImage(c, x0, y0, w, h, (side - w) >> 1, (side - h) >> 1, w, h);
while (cur.width / 2 >= INNER) {               // halving steps keep the downscale smooth
  const s = Math.max(Math.round(cur.width / 2), INNER);
  const n = Object.assign(document.createElement('canvas'), { width: s, height: s });
  const nx = n.getContext('2d'); nx.imageSmoothingQuality = 'high'; nx.drawImage(cur, 0, 0, s, s); cur = n;
}
const out = Object.assign(document.createElement('canvas'), { width: SIZE, height: SIZE });
const ox2 = out.getContext('2d'); ox2.imageSmoothingQuality = 'high';
ox2.drawImage(cur, MARGIN, MARGIN, INNER, INNER);
out.toDataURL('image/png');
