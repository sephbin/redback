using Rhino.Geometry;

namespace Redback.Render
{
    /// <summary>A <see cref="FlatTri"/> tagged with the index of the input mesh it came from.</summary>
    public struct IdTri
    {
        /// <summary>Triangle geometry and face normal in world space.</summary>
        public FlatTri Tri;
        /// <summary>Index of the source mesh in the list passed to <c>IdBufferCore.CollectTriangles</c>.</summary>
        public int Owner;
    }

    /// <summary>Output of one 360° ID-buffer render (six 90° cube faces).</summary>
    public class IdBufferResult
    {
        /// <summary>Camera position the render was taken from.</summary>
        public Point3d Camera;
        /// <summary>Pixels per cube-face edge.</summary>
        public int Resolution;
        /// <summary>Basis of each cube face: +X, -X, +Y, -Y, +Z, -Z.</summary>
        public OrthoView[] Faces;
        /// <summary>Per face, per pixel: index into the triangle list, or -1 for background.</summary>
        public int[][] TriIndex;
        /// <summary>Per face, per pixel: view-space depth along the face axis.</summary>
        public float[][] Depth;
        /// <summary>Per input mesh: fraction of the full sphere it covers (0–1).</summary>
        public double[] SphereShare;
        /// <summary>Per input mesh: fraction of the non-background view it covers (0–1).</summary>
        public double[] HitShare;
        /// <summary>Fraction of the sphere with no hit (open sky, or beyond the max distance).</summary>
        public double BackgroundShare;
    }

    /// <summary>Debug image types for <c>IdBufferCore.PassImage</c>.</summary>
    public enum IdPass
    {
        /// <summary>Flat unique colour per input mesh.</summary>
        Id,
        /// <summary>Greyscale distance (near = white).</summary>
        Depth,
        /// <summary>World normal mapped to RGB.</summary>
        Normal,
        /// <summary>World XYZ mapped to RGB, normalised to the bounds of the visible hits.</summary>
        Position,
    }
}
