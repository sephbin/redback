using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Rhino.Geometry;

namespace Redback.Render
{
    /// <summary>
    /// CPU-based orthographic shadow-map renderer.
    /// All methods are pure static so they can be called from unit tests without
    /// any Grasshopper or display dependency.
    /// </summary>
    public static class ShadowMapCore
    {
        // ──────────────────────────────────────────────────────────────────────
        // Step 1 — Orthonormal basis
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a right-handed orthonormal basis from a forward (view / light) direction.
        /// Uses the world Y axis as the secondary up reference when the forward direction
        /// is nearly parallel to world Z.
        /// </summary>
        public static OrthoView BuildBasis(Vector3d forward)
        {
            forward.Unitize();
            Vector3d worldUp = Math.Abs(forward * Vector3d.ZAxis) < 0.99
                ? Vector3d.ZAxis
                : Vector3d.YAxis;

            Vector3d right = Vector3d.CrossProduct(forward, worldUp);
            right.Unitize();

            Vector3d up = Vector3d.CrossProduct(right, forward);
            up.Unitize();

            return new OrthoView { Right = right, Up = up, Forward = forward };
        }

        // ──────────────────────────────────────────────────────────────────────
        // Step 2 — Triangle collection
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a sequence of meshes into a flat list of triangles with face normals.
        /// Null meshes and degenerate face indices are silently skipped.
        /// </summary>
        public static List<FlatTri> CollectTriangles(IEnumerable<Mesh> meshes)
        {
            var tris = new List<FlatTri>();
            foreach (var m in meshes)
            {
                if (m == null) continue;
                var dup = m.DuplicateMesh();
                dup.Faces.ConvertQuadsToTriangles();
                dup.FaceNormals.ComputeFaceNormals();

                var verts   = dup.Vertices;
                var faces   = dup.Faces;
                var normals = dup.FaceNormals;
                int vCount  = verts.Count;

                for (int i = 0; i < faces.Count; i++)
                {
                    var f = faces[i];
                    if (f.A < 0 || f.A >= vCount) continue;
                    if (f.B < 0 || f.B >= vCount) continue;
                    if (f.C < 0 || f.C >= vCount) continue;

                    var pa = verts[f.A];
                    var pb = verts[f.B];
                    var pc = verts[f.C];
                    var fn = normals[i];

                    tris.Add(new FlatTri
                    {
                        A      = new Point3d(pa.X, pa.Y, pa.Z),
                        B      = new Point3d(pb.X, pb.Y, pb.Z),
                        C      = new Point3d(pc.X, pc.Y, pc.Z),
                        Normal = new Vector3d(fn.X, fn.Y, fn.Z),
                    });
                }
            }
            return tris;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Step 3 — Frustum
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the camera frustum from an explicit world-space centre point and
        /// physical frame dimensions.  The X/Y extents are fixed by <paramref name="camPos"/>
        /// and the frame size; Z extents are scanned from the geometry so the depth
        /// buffer has the tightest useful range.
        /// </summary>
        public static Frustum ComputeCameraFrustum(
            List<FlatTri> tris, OrthoView view,
            Point3d camPos, float frameWidth, float frameHeight)
        {
            float cx = Dot(camPos, view.Right);
            float cy = Dot(camPos, view.Up);

            float minZ = float.MaxValue, maxZ = float.MinValue;
            foreach (var tri in tris)
            {
                float az = Dot(tri.A, view.Forward);
                float bz = Dot(tri.B, view.Forward);
                float cz = Dot(tri.C, view.Forward);
                if (az < minZ) minZ = az; if (az > maxZ) maxZ = az;
                if (bz < minZ) minZ = bz; if (bz > maxZ) maxZ = bz;
                if (cz < minZ) minZ = cz; if (cz > maxZ) maxZ = cz;
            }

            return new Frustum
            {
                MinX = cx - frameWidth  / 2,
                MaxX = cx + frameWidth  / 2,
                MinY = cy - frameHeight / 2,
                MaxY = cy + frameHeight / 2,
                MinZ = minZ,
                MaxZ = maxZ,
            };
        }

        /// <summary>
        /// Projects all triangle vertices into <paramref name="view"/> space and returns
        /// a tight axis-aligned frustum with proportional <paramref name="paddingFrac"/>
        /// padding on X and Y.  Used for the light frustum.
        /// </summary>
        public static Frustum ComputeFrustum(List<FlatTri> tris, OrthoView view, float paddingFrac)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (var tri in tris)
            {
                ProjectVertex(tri.A, view, out float ax, out float ay, out float az);
                ProjectVertex(tri.B, view, out float bx, out float by, out float bz);
                ProjectVertex(tri.C, view, out float cx, out float cy, out float cz);

                if (ax < minX) minX = ax; if (ax > maxX) maxX = ax;
                if (bx < minX) minX = bx; if (bx > maxX) maxX = bx;
                if (cx < minX) minX = cx; if (cx > maxX) maxX = cx;

                if (ay < minY) minY = ay; if (ay > maxY) maxY = ay;
                if (by < minY) minY = by; if (by > maxY) maxY = by;
                if (cy < minY) minY = cy; if (cy > maxY) maxY = cy;

                if (az < minZ) minZ = az; if (az > maxZ) maxZ = az;
                if (bz < minZ) minZ = bz; if (bz > maxZ) maxZ = bz;
                if (cz < minZ) minZ = cz; if (cz > maxZ) maxZ = cz;
            }

            float px = (maxX - minX) * paddingFrac;
            float py = (maxY - minY) * paddingFrac;

            return new Frustum
            {
                MinX = minX - px, MaxX = maxX + px,
                MinY = minY - py, MaxY = maxY + py,
                MinZ = minZ,      MaxZ = maxZ,
            };
        }

        // ──────────────────────────────────────────────────────────────────────
        // Step 4 — Rasterisation
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Rasterizes <paramref name="tri"/> into <paramref name="depthBuf"/> (and
        /// optionally <paramref name="normalBuf"/>) using an orthographic projection
        /// defined by <paramref name="view"/> and <paramref name="f"/>.
        /// </summary>
        /// <remarks>
        /// <para>Screen-space mapping (Y-flipped so higher world-Y = lower screen-Y):</para>
        /// <code>
        /// screenX = (dot(v, Right) − minX) / rangeX × width
        /// screenY = (1 − (dot(v, Up) − minY) / rangeY) × height
        /// depth   = dot(v, Forward)
        /// </code>
        /// <para>Pass <paramref name="normalBuf"/> = <c>null</c> for the shadow pass.</para>
        /// </remarks>
        public static void RasterizeTri(
            in FlatTri tri,
            in OrthoView view,
            in Frustum f,
            int width, int height,
            float[] depthBuf,
            Vector3d[] normalBuf)
        {
            float rangeX = f.RangeX;
            float rangeY = f.RangeY;
            if (rangeX <= 0 || rangeY <= 0) return;

            // Project to screen space
            float x0 = ToScreenX(tri.A, view, f, width);
            float y0 = ToScreenY(tri.A, view, f, height);
            float z0 = Dot(tri.A, view.Forward);

            float x1 = ToScreenX(tri.B, view, f, width);
            float y1 = ToScreenY(tri.B, view, f, height);
            float z1 = Dot(tri.B, view.Forward);

            float x2 = ToScreenX(tri.C, view, f, width);
            float y2 = ToScreenY(tri.C, view, f, height);
            float z2 = Dot(tri.C, view.Forward);

            // Signed 2-D area (positive = CCW)
            float area = (x1 - x0) * (y2 - y0) - (x2 - x0) * (y1 - y0);
            if (Math.Abs(area) < 1e-8f) return;

            // Screen-space bounding box clamped to image
            int xMin = Math.Max(0,         (int)Math.Floor(Min3(x0, x1, x2)));
            int xMax = Math.Min(width  - 1, (int)Math.Ceiling(Max3(x0, x1, x2)));
            int yMin = Math.Max(0,         (int)Math.Floor(Min3(y0, y1, y2)));
            int yMax = Math.Min(height - 1, (int)Math.Ceiling(Max3(y0, y1, y2)));

            bool ccw = area > 0;

            for (int py = yMin; py <= yMax; py++)
            {
                for (int px = xMin; px <= xMax; px++)
                {
                    float pcx = px + 0.5f;
                    float pcy = py + 0.5f;

                    // Edge functions — e0 opposite v2, e1 opposite v0, e2 opposite v1
                    float e0 = (x1 - x0) * (pcy - y0) - (y1 - y0) * (pcx - x0);
                    float e1 = (x2 - x1) * (pcy - y1) - (y2 - y1) * (pcx - x1);
                    float e2 = (x0 - x2) * (pcy - y2) - (y0 - y2) * (pcx - x2);

                    bool inside = ccw
                        ? (e0 >= 0f && e1 >= 0f && e2 >= 0f)
                        : (e0 <= 0f && e1 <= 0f && e2 <= 0f);

                    if (!inside) continue;

                    // Barycentric weights: w0 → A, w1 → B, w2 → C
                    float w0 = e1 / area;
                    float w1 = e2 / area;
                    float w2 = e0 / area;

                    float depth = w0 * z0 + w1 * z1 + w2 * z2;
                    int idx = py * width + px;

                    if (depth < depthBuf[idx])
                    {
                        depthBuf[idx] = depth;
                        if (normalBuf != null)
                            normalBuf[idx] = tri.Normal;
                    }
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Steps 5–7 — Full render pipeline
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Renders <paramref name="tris"/> to a PNG at <paramref name="outputPath"/> and
        /// returns the world-space rectangle suitable for a Rhino PictureFrame.
        /// </summary>
        /// <param name="camPos">
        /// World-space centre of the output frame.  The frame extends
        /// ±<paramref name="frameWidth"/>/2 along <c>camView.Right</c> and
        /// ±<paramref name="frameHeight"/>/2 along <c>camView.Up</c>.
        /// </param>
        /// <param name="frameWidth">Physical frame width in scene units.</param>
        /// <param name="frameHeight">Physical frame height in scene units.</param>
        /// <param name="pixelWidth">
        /// Output image width in pixels.  Height is auto-computed as
        /// <c>round(pixelWidth × frameHeight / frameWidth)</c> so the pixel
        /// aspect ratio always matches the scene frame — no stretching.
        /// </param>
        public static Rectangle3d Render(
            List<FlatTri> tris,
            OrthoView camView,
            Point3d camPos,
            double frameWidth,
            double frameHeight,
            OrthoView lightView,
            int pixelWidth,
            int shadowRes,
            float ambient,
            string outputPath)
        {
            int pixelHeight = Math.Max(1, (int)Math.Round(pixelWidth * frameHeight / frameWidth));

            // ── Frustums ──────────────────────────────────────────────────────
            // Camera: X/Y fixed by camPos + frame dims; Z scanned from geometry.
            var camFrustum   = ComputeCameraFrustum(tris, camView, camPos,
                                   (float)frameWidth, (float)frameHeight);
            // Light: tight fit around all geometry with a small safety margin.
            var lightFrustum = ComputeFrustum(tris, lightView, 0.005f);

            // ── Shadow pass ───────────────────────────────────────────────────
            var shadowDepth = new float[shadowRes * shadowRes];
            Fill(shadowDepth, float.MaxValue);
            foreach (var tri in tris)
                RasterizeTri(tri, lightView, lightFrustum, shadowRes, shadowRes, shadowDepth, null);

            // ── Camera pass ───────────────────────────────────────────────────
            var camDepth   = new float[pixelWidth * pixelHeight];
            var camNormals = new Vector3d[pixelWidth * pixelHeight];
            Fill(camDepth, float.MaxValue);
            foreach (var tri in tris)
                RasterizeTri(tri, camView, camFrustum, pixelWidth, pixelHeight, camDepth, camNormals);

            // ── Shading ───────────────────────────────────────────────────────
            float bias      = lightFrustum.RangeZ * 0.002f;
            float lRangeX   = lightFrustum.RangeX;
            float lRangeY   = lightFrustum.RangeY;
            float lMinX     = lightFrustum.MinX;
            float lMinY     = lightFrustum.MinY;
            float camRangeX = camFrustum.RangeX;
            float camRangeY = camFrustum.RangeY;
            float camMinX   = camFrustum.MinX;
            float camMinY   = camFrustum.MinY;

            var pixels = new int[pixelWidth * pixelHeight];  // pre-allocated; no alloc in per-pixel loop

            for (int py = 0; py < pixelHeight; py++)
            {
                for (int px = 0; px < pixelWidth; px++)
                {
                    int idx = py * pixelWidth + px;

                    // Step 5a — background pixel → transparent
                    if (camDepth[idx] == float.MaxValue)
                    {
                        pixels[idx] = 0;
                        continue;
                    }

                    // Step 5b — reconstruct world position from camera pixel + stored depth
                    float camX = camMinX + (px + 0.5f) / pixelWidth  * camRangeX;
                    float camY = camMinY + (1.0f - (py + 0.5f) / pixelHeight) * camRangeY;
                    float camD = camDepth[idx];

                    double wx = camX * camView.Right.X + camY * camView.Up.X + camD * camView.Forward.X;
                    double wy = camX * camView.Right.Y + camY * camView.Up.Y + camD * camView.Forward.Y;
                    double wz = camX * camView.Right.Z + camY * camView.Up.Z + camD * camView.Forward.Z;

                    // Step 5c — project world point into light space
                    float lx = (float)(wx * lightView.Right.X   + wy * lightView.Right.Y   + wz * lightView.Right.Z);
                    float ly = (float)(wx * lightView.Up.X      + wy * lightView.Up.Y      + wz * lightView.Up.Z);
                    float lz = (float)(wx * lightView.Forward.X + wy * lightView.Forward.Y + wz * lightView.Forward.Z);

                    float su = (lx - lMinX) / lRangeX * shadowRes;
                    float sv = (1.0f - (ly - lMinY) / lRangeY) * shadowRes;

                    // Step 5d — 3×3 PCF soft shadow
                    int suBase = (int)Math.Floor(su);
                    int svBase = (int)Math.Floor(sv);
                    float litCount = 0f;

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int sx = suBase + dx;
                            int sy = svBase + dy;
                            if (sx < 0 || sx >= shadowRes || sy < 0 || sy >= shadowRes)
                            {
                                litCount += 1f;  // outside shadow frustum → assume lit
                                continue;
                            }
                            if (lz <= shadowDepth[sy * shadowRes + sx] + bias)
                                litCount += 1f;
                        }
                    }

                    float litPercent = litCount / 9.0f;

                    // Step 5e — diffuse + ambient shading
                    var normal = camNormals[idx];
                    normal.Unitize();
                    float nDotL  = (float)(normal.X * lightView.Forward.X
                                         + normal.Y * lightView.Forward.Y
                                         + normal.Z * lightView.Forward.Z);
                    float diffuse = Math.Max(0f, -nDotL);
                    float shade   = ambient + (1f - ambient) * diffuse * litPercent;
                    shade         = Math.Max(0f, Math.Min(1f, shade));

                    int g = (int)(shade * 255f);
                    pixels[idx] = (255 << 24) | (g << 16) | (g << 8) | g;
                }
            }

            // ── Step 6 — stride-safe bitmap write ────────────────────────────
            WriteBitmap(pixels, pixelWidth, pixelHeight, outputPath);

            // ── Step 7 — world-space output rectangle ─────────────────────────
            // camPos is the centre; the rectangle's origin is the bottom-left corner.
            double hw = frameWidth  / 2;
            double hh = frameHeight / 2;
            var origin = new Point3d(
                camPos.X - hw * camView.Right.X - hh * camView.Up.X,
                camPos.Y - hw * camView.Right.Y - hh * camView.Up.Y,
                camPos.Z - hw * camView.Right.Z - hh * camView.Up.Z);
            var plane = new Plane(origin, camView.Right, camView.Up);
            return new Rectangle3d(plane,
                new Interval(0, frameWidth),
                new Interval(0, frameHeight));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Private helpers
        // ──────────────────────────────────────────────────────────────────────

        private static void WriteBitmap(int[] pixels, int width, int height, string path)
        {
            var bmp     = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            for (int row = 0; row < height; row++)
                Marshal.Copy(pixels, row * width,
                    IntPtr.Add(bmpData.Scan0, row * bmpData.Stride), width);
            bmp.UnlockBits(bmpData);
            bmp.Save(path, ImageFormat.Png);
            bmp.Dispose();
        }

        private static void ProjectVertex(
            Point3d p, in OrthoView v,
            out float vx, out float vy, out float vz)
        {
            vx = (float)(p.X * v.Right.X   + p.Y * v.Right.Y   + p.Z * v.Right.Z);
            vy = (float)(p.X * v.Up.X      + p.Y * v.Up.Y      + p.Z * v.Up.Z);
            vz = (float)(p.X * v.Forward.X + p.Y * v.Forward.Y + p.Z * v.Forward.Z);
        }

        private static float ToScreenX(Point3d p, in OrthoView v, in Frustum f, int width) =>
            (float)((p.X * v.Right.X + p.Y * v.Right.Y + p.Z * v.Right.Z - f.MinX)
                    / f.RangeX * width);

        private static float ToScreenY(Point3d p, in OrthoView v, in Frustum f, int height) =>
            (float)((1.0 - (p.X * v.Up.X + p.Y * v.Up.Y + p.Z * v.Up.Z - f.MinY)
                    / f.RangeY) * height);

        private static float Dot(Point3d p, Vector3d v) =>
            (float)(p.X * v.X + p.Y * v.Y + p.Z * v.Z);

        private static void Fill(float[] buf, float value)
        {
            for (int i = 0; i < buf.Length; i++) buf[i] = value;
        }

        private static float Min3(float a, float b, float c) =>
            a < b ? (a < c ? a : c) : (b < c ? b : c);

        private static float Max3(float a, float b, float c) =>
            a > b ? (a > c ? a : c) : (b > c ? b : c);
    }
}
