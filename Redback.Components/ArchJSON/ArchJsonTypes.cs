using System;
using System.IO;
using System.Linq;
using Rhino.Geometry;

namespace Redback.Components.ArchJSON
{
    public class ArchJsonText
    {
        public Plane Location { get; }
        public string Content { get; }

        public ArchJsonText(Plane location, string content)
        {
            Location = location;
            Content = content ?? "";
        }

        public override string ToString() => "<ArchJSON Text>: " + Content;
    }

    public class ArchJsonImage
    {
        public Plane Location { get; }
        public double Width { get; }
        public double Height { get; }
        public string ImagePath { get; }
        public string FileName { get; }
        public string FileDirectory { get; }

        public ArchJsonImage(Curve rect, string imagePath)
        {
            ImagePath = imagePath ?? "";
            FileName = Path.GetFileName(ImagePath);
            FileDirectory = Path.GetDirectoryName(ImagePath) ?? "";

            if (rect == null) return;

            var poly = new Polyline();
            if (!rect.TryGetPolyline(out poly) || poly.Count < 3) return;

            var pts = poly.ToArray();
            Location = new Plane(pts[0], pts[1] - pts[0], pts[Math.Min(2, pts.Length - 1)] - pts[0]);

            // Measure extents by projecting all points onto the plane
            double minU = double.MaxValue, maxU = double.MinValue;
            double minV = double.MaxValue, maxV = double.MinValue;
            foreach (var pt in pts)
            {
                Location.ClosestParameter(pt, out double u, out double v);
                if (u < minU) minU = u;
                if (u > maxU) maxU = u;
                if (v < minV) minV = v;
                if (v > maxV) maxV = v;
            }
            Width = maxU - minU;
            Height = maxV - minV;
        }

        public override string ToString() => "<ArchJSON Image>: " + FileName;
    }
}
