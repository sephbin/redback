using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Redback.Render
{
    /// <summary>
    /// CPU 360° ID-buffer renderer. A camera is placed at a point and six 90° perspective
    /// faces (a cube map) are rasterised with a flat per-mesh ID instead of shading. Counting
    /// pixels per ID, weighted by solid angle, gives the share of the view each mesh covers.
    /// All methods are pure static so they can be tested without Grasshopper.
    /// </summary>
    public static class IdBufferCore
    {
        // Cube face order: +X, -X, +Y, -Y, +Z, -Z.
        private static readonly OrthoView[] _faces = BuildFaces();

        private static OrthoView[] BuildFaces()
        {
            var fwd = new[]
            {
                new Vector3d( 1, 0, 0), new Vector3d(-1, 0, 0),
                new Vector3d( 0, 1, 0), new Vector3d( 0,-1, 0),
                new Vector3d( 0, 0, 1), new Vector3d( 0, 0,-1),
            };
            var faces = new OrthoView[6];
            for (int i = 0; i < 6; i++)
            {
                // Horizontal faces keep world Z up; the Z faces use world Y up.
                Vector3d up = i < 4 ? Vector3d.ZAxis : Vector3d.YAxis;
                Vector3d right = Vector3d.CrossProduct(fwd[i], up);
                faces[i] = new OrthoView { Right = right, Up = up, Forward = fwd[i] };
            }
            return faces;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Triangle collection
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Flattens meshes to triangles tagged with the mesh's index in <paramref name="meshes"/>.
        /// Null meshes are skipped but still occupy an index, so IDs stay aligned.
        /// </summary>
        public static List<IdTri> CollectTriangles(IList<Mesh> meshes)
        {
            var result = new List<IdTri>();
            var single = new Mesh[1];
            for (int i = 0; i < meshes.Count; i++)
            {
                if (meshes[i] == null) continue;
                single[0] = meshes[i];
                foreach (var t in ShadowMapCore.CollectTriangles(single))
                    result.Add(new IdTri { Tri = t, Owner = i });
            }
            return result;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Render
        // ──────────────────────────────────────────────────────────────────────

        /// <param name="tris">Triangles from <see cref="CollectTriangles"/>.</param>
        /// <param name="meshCount">Number of input meshes (size of the share arrays).</param>
        /// <param name="cam">Camera position.</param>
        /// <param name="res">Pixels per cube-face edge.</param>
        /// <param name="maxDistance">Hits further than this count as background; &lt;= 0 disables.</param>
        /// <param name="near">Near clip distance in scene units.</param>
        public static IdBufferResult Render(
            List<IdTri> tris, int meshCount, Point3d cam,
            int res, double maxDistance, double near = 1e-3)
        {
            var triBufs   = new int[6][];
            var depthBufs = new float[6][];

            Parallel.For(0, 6, f =>
            {
                var tb = new int[res * res];
                var db = new float[res * res];
                for (int i = 0; i < tb.Length; i++) { tb[i] = -1; db[i] = float.MaxValue; }

                var view = _faces[f];
                for (int i = 0; i < tris.Count; i++)
                    RasterizeFace(tris[i].Tri, i, view, cam, res, near, tb, db);

                triBufs[f]   = tb;
                depthBufs[f] = db;
            });

            var share = new double[meshCount];
            double bg = 0;
            double pixelArea = (2.0 / res) * (2.0 / res);
            double fourPi = 4.0 * Math.PI;

            for (int f = 0; f < 6; f++)
            {
                var tb = triBufs[f];
                var db = depthBufs[f];
                for (int py = 0; py < res; py++)
                {
                    double v = ((py + 0.5) / res - 0.5) * -2.0;
                    for (int px = 0; px < res; px++)
                    {
                        double u = ((px + 0.5) / res - 0.5) * 2.0;
                        double r2 = 1.0 + u * u + v * v;
                        double w = pixelArea / (r2 * Math.Sqrt(r2)) / fourPi;

                        int idx = py * res + px;
                        if (tb[idx] >= 0 && maxDistance > 0 && db[idx] * Math.Sqrt(r2) > maxDistance)
                        {
                            tb[idx] = -1;
                            db[idx] = float.MaxValue;
                        }

                        if (tb[idx] < 0) bg += w;
                        else share[tris[tb[idx]].Owner] += w;
                    }
                }
            }

            var hit = new double[meshCount];
            double hitTotal = 1.0 - bg;
            if (hitTotal > 1e-12)
                for (int i = 0; i < meshCount; i++) hit[i] = share[i] / hitTotal;

            return new IdBufferResult
            {
                Camera          = cam,
                Resolution      = res,
                Faces           = _faces,
                TriIndex        = triBufs,
                Depth           = depthBufs,
                SphereShare     = share,
                HitShare        = hit,
                BackgroundShare = bg,
            };
        }

        // ──────────────────────────────────────────────────────────────────────
        // Debug pass image (equirectangular)
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resamples the cube map into an equirectangular ARGB image (width 2×res, height res).
        /// Background pixels are black.
        /// </summary>
        public static int[] PassImage(IdBufferResult r, List<IdTri> tris, IdPass pass,
                                      out int width, out int height)
        {
            int res = r.Resolution;
            width  = res * 2;
            height = res;
            var pixels = new int[width * height];
            var dist   = pass == IdPass.Depth ? new float[pixels.Length] : null;
            float maxDist = 0f;
            var pos    = pass == IdPass.Position ? new Point3d[pixels.Length] : null;
            var hasPos = pass == IdPass.Position ? new bool[pixels.Length] : null;
            var lo = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
            var hi = new Point3d(double.MinValue, double.MinValue, double.MinValue);

            for (int y = 0; y < height; y++)
            {
                double lat = Math.PI / 2 - (y + 0.5) / height * Math.PI;
                double cl = Math.Cos(lat), sl = Math.Sin(lat);
                for (int x = 0; x < width; x++)
                {
                    double lon = (x + 0.5) / width * 2 * Math.PI - Math.PI;
                    var d = new Vector3d(cl * Math.Cos(lon), cl * Math.Sin(lon), sl);

                    SampleCube(r, d, out int face, out int pidx, out double rayScale);
                    int ti = r.TriIndex[face][pidx];
                    int o  = y * width + x;
                    if (ti < 0) { pixels[o] = unchecked((int)0xFF000000); continue; }

                    switch (pass)
                    {
                        case IdPass.Id:
                            pixels[o] = OwnerColour(tris[ti].Owner);
                            break;
                        case IdPass.Normal:
                            var n = tris[ti].Tri.Normal;
                            pixels[o] = Pack(
                                (float)(n.X * 0.5 + 0.5), (float)(n.Y * 0.5 + 0.5), (float)(n.Z * 0.5 + 0.5));
                            break;
                        case IdPass.Depth:
                            float dd = (float)(r.Depth[face][pidx] * rayScale);
                            dist[o] = dd;
                            if (dd > maxDist) maxDist = dd;
                            break;
                        case IdPass.Position:
                            double len = r.Depth[face][pidx] * rayScale;
                            var p = new Point3d(
                                r.Camera.X + d.X * len, r.Camera.Y + d.Y * len, r.Camera.Z + d.Z * len);
                            pos[o] = p;
                            hasPos[o] = true;
                            if (p.X < lo.X) lo.X = p.X; if (p.X > hi.X) hi.X = p.X;
                            if (p.Y < lo.Y) lo.Y = p.Y; if (p.Y > hi.Y) hi.Y = p.Y;
                            if (p.Z < lo.Z) lo.Z = p.Z; if (p.Z > hi.Z) hi.Z = p.Z;
                            break;
                    }
                }
            }

            if (pass == IdPass.Depth)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (dist[i] <= 0f) { pixels[i] = unchecked((int)0xFF000000); continue; }
                    float g = maxDist > 0f ? 1f - dist[i] / maxDist : 0f;
                    pixels[i] = Pack(g, g, g);
                }
            }
            if (pass == IdPass.Position)
            {
                double sx = hi.X - lo.X, sy = hi.Y - lo.Y, sz = hi.Z - lo.Z;
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (!hasPos[i]) { pixels[i] = unchecked((int)0xFF000000); continue; }
                    pixels[i] = Pack(
                        sx > 0 ? (float)((pos[i].X - lo.X) / sx) : 0f,
                        sy > 0 ? (float)((pos[i].Y - lo.Y) / sy) : 0f,
                        sz > 0 ? (float)((pos[i].Z - lo.Z) / sz) : 0f);
                }
            }
            return pixels;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Private helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Finds the cube face and pixel for a world direction.</summary>
        private static void SampleCube(IdBufferResult r, Vector3d d,
                                       out int face, out int pidx, out double rayScale)
        {
            double ax = Math.Abs(d.X), ay = Math.Abs(d.Y), az = Math.Abs(d.Z);
            if (ax >= ay && ax >= az)      face = d.X >= 0 ? 0 : 1;
            else if (ay >= az)             face = d.Y >= 0 ? 2 : 3;
            else                           face = d.Z >= 0 ? 4 : 5;

            var v = r.Faces[face];
            double z = d * v.Forward;
            double u = (d * v.Right) / z;
            double w = (d * v.Up) / z;
            rayScale = Math.Sqrt(1.0 + u * u + w * w);

            int res = r.Resolution;
            int px = Math.Min(res - 1, Math.Max(0, (int)((u * 0.5 + 0.5) * res)));
            int py = Math.Min(res - 1, Math.Max(0, (int)((0.5 - w * 0.5) * res)));
            pidx = py * res + px;
        }

        private static void RasterizeFace(
            in FlatTri t, int triIndex, in OrthoView v, Point3d cam,
            int res, double near, int[] triBuf, float[] depthBuf)
        {
            ToView(t.A, cam, v, out double x0, out double y0, out double z0);
            ToView(t.B, cam, v, out double x1, out double y1, out double z1);
            ToView(t.C, cam, v, out double x2, out double y2, out double z2);

            // Cull triangles wholly outside one of the four side planes |x| = z, |y| = z.
            if (x0 > z0 && x1 > z1 && x2 > z2) return;
            if (-x0 > z0 && -x1 > z1 && -x2 > z2) return;
            if (y0 > z0 && y1 > z1 && y2 > z2) return;
            if (-y0 > z0 && -y1 > z1 && -y2 > z2) return;

            bool in0 = z0 >= near, in1 = z1 >= near, in2 = z2 >= near;
            int inside = (in0 ? 1 : 0) + (in1 ? 1 : 0) + (in2 ? 1 : 0);
            if (inside == 0) return;

            if (inside == 3)
            {
                Raster(x0, y0, z0, x1, y1, z1, x2, y2, z2, triIndex, res, triBuf, depthBuf);
                return;
            }

            // Reorder so the inside vertices come first.
            if (inside == 1)
            {
                if (in1) { Swap(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1); }
                else if (in2) { Swap(ref x0, ref y0, ref z0, ref x2, ref y2, ref z2); }
                // v0 inside; clip edges v0-v1 and v0-v2.
                Clip(x0, y0, z0, x1, y1, z1, near, out double ax, out double ay, out double az);
                Clip(x0, y0, z0, x2, y2, z2, near, out double bx, out double by, out double bz);
                Raster(x0, y0, z0, ax, ay, az, bx, by, bz, triIndex, res, triBuf, depthBuf);
            }
            else
            {
                // Two inside: move the outside vertex to slot 2.
                if (!in0) { Swap(ref x0, ref y0, ref z0, ref x2, ref y2, ref z2); }
                else if (!in1) { Swap(ref x1, ref y1, ref z1, ref x2, ref y2, ref z2); }
                // v0, v1 inside; clip edges v0-v2 and v1-v2 to form a quad.
                Clip(x0, y0, z0, x2, y2, z2, near, out double ax, out double ay, out double az);
                Clip(x1, y1, z1, x2, y2, z2, near, out double bx, out double by, out double bz);
                Raster(x0, y0, z0, x1, y1, z1, bx, by, bz, triIndex, res, triBuf, depthBuf);
                Raster(x0, y0, z0, bx, by, bz, ax, ay, az, triIndex, res, triBuf, depthBuf);
            }
        }

        private static void Raster(
            double x0, double y0, double z0,
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            int triIndex, int res, int[] triBuf, float[] depthBuf)
        {
            // Perspective projection for a 90° face, y flipped for screen space.
            double iz0 = 1.0 / z0, iz1 = 1.0 / z1, iz2 = 1.0 / z2;
            double sx0 = (x0 * iz0 * 0.5 + 0.5) * res, sy0 = (0.5 - y0 * iz0 * 0.5) * res;
            double sx1 = (x1 * iz1 * 0.5 + 0.5) * res, sy1 = (0.5 - y1 * iz1 * 0.5) * res;
            double sx2 = (x2 * iz2 * 0.5 + 0.5) * res, sy2 = (0.5 - y2 * iz2 * 0.5) * res;

            double area = (sx1 - sx0) * (sy2 - sy0) - (sx2 - sx0) * (sy1 - sy0);
            if (Math.Abs(area) < 1e-12) return;

            int xMin = Math.Max(0,       (int)Math.Floor(Math.Min(sx0, Math.Min(sx1, sx2))));
            int xMax = Math.Min(res - 1, (int)Math.Ceiling(Math.Max(sx0, Math.Max(sx1, sx2))));
            int yMin = Math.Max(0,       (int)Math.Floor(Math.Min(sy0, Math.Min(sy1, sy2))));
            int yMax = Math.Min(res - 1, (int)Math.Ceiling(Math.Max(sy0, Math.Max(sy1, sy2))));

            bool ccw = area > 0;

            for (int py = yMin; py <= yMax; py++)
            {
                for (int px = xMin; px <= xMax; px++)
                {
                    double pcx = px + 0.5, pcy = py + 0.5;
                    double e0 = (sx1 - sx0) * (pcy - sy0) - (sy1 - sy0) * (pcx - sx0);
                    double e1 = (sx2 - sx1) * (pcy - sy1) - (sy2 - sy1) * (pcx - sx1);
                    double e2 = (sx0 - sx2) * (pcy - sy2) - (sy0 - sy2) * (pcx - sx2);

                    bool covered = ccw
                        ? (e0 >= 0 && e1 >= 0 && e2 >= 0)
                        : (e0 <= 0 && e1 <= 0 && e2 <= 0);
                    if (!covered) continue;

                    // Perspective-correct depth: 1/z is linear in screen space.
                    double invZ = (e1 * iz0 + e2 * iz1 + e0 * iz2) / area;
                    if (invZ <= 0) continue;
                    float z = (float)(1.0 / invZ);

                    int idx = py * res + px;
                    if (z < depthBuf[idx])
                    {
                        depthBuf[idx] = z;
                        triBuf[idx]   = triIndex;
                    }
                }
            }
        }

        private static void ToView(Point3d p, Point3d cam, in OrthoView v,
                                   out double x, out double y, out double z)
        {
            double dx = p.X - cam.X, dy = p.Y - cam.Y, dz = p.Z - cam.Z;
            x = dx * v.Right.X   + dy * v.Right.Y   + dz * v.Right.Z;
            y = dx * v.Up.X      + dy * v.Up.Y      + dz * v.Up.Z;
            z = dx * v.Forward.X + dy * v.Forward.Y + dz * v.Forward.Z;
        }

        /// <summary>Point where the segment from an inside vertex to an outside one crosses z = near.</summary>
        private static void Clip(double xi, double yi, double zi, double xo, double yo, double zo,
                                 double near, out double x, out double y, out double z)
        {
            double t = (near - zi) / (zo - zi);
            x = xi + (xo - xi) * t;
            y = yi + (yo - yi) * t;
            z = near;
        }

        private static void Swap(ref double ax, ref double ay, ref double az,
                                 ref double bx, ref double by, ref double bz)
        {
            double tx = ax, ty = ay, tz = az;
            ax = bx; ay = by; az = bz;
            bx = tx; by = ty; bz = tz;
        }

        private static int OwnerColour(int owner)
        {
            // Golden-ratio hue steps keep neighbouring IDs visually distinct.
            double h = (owner * 0.61803398875) % 1.0;
            double s = 0.65, vv = 0.95;
            double c = vv * s;
            double hp = h * 6.0;
            double xx = c * (1.0 - Math.Abs(hp % 2.0 - 1.0));
            double m = vv - c;
            double r, g, b;
            if      (hp < 1) { r = c;  g = xx; b = 0; }
            else if (hp < 2) { r = xx; g = c;  b = 0; }
            else if (hp < 3) { r = 0;  g = c;  b = xx; }
            else if (hp < 4) { r = 0;  g = xx; b = c; }
            else if (hp < 5) { r = xx; g = 0;  b = c; }
            else             { r = c;  g = 0;  b = xx; }
            return Pack((float)(r + m), (float)(g + m), (float)(b + m));
        }

        private static int Pack(float r, float g, float b)
        {
            int ri = (int)(Math.Max(0f, Math.Min(1f, r)) * 255f);
            int gi = (int)(Math.Max(0f, Math.Min(1f, g)) * 255f);
            int bi = (int)(Math.Max(0f, Math.Min(1f, b)) * 255f);
            return (255 << 24) | (ri << 16) | (gi << 8) | bi;
        }
    }
}
