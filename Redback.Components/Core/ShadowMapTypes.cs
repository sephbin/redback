using Rhino.Geometry;

namespace Redback.Render
{
    /// <summary>Flattened triangle with a pre-computed face normal.</summary>
    public struct FlatTri
    {
        /// <summary>First vertex in world space.</summary>
        public Point3d A;
        /// <summary>Second vertex in world space.</summary>
        public Point3d B;
        /// <summary>Third vertex in world space.</summary>
        public Point3d C;
        /// <summary>Unit face normal in world space.</summary>
        public Vector3d Normal;
    }

    /// <summary>Right-handed orthonormal camera or light basis.</summary>
    public struct OrthoView
    {
        /// <summary>Rightward axis (screen +X).</summary>
        public Vector3d Right;
        /// <summary>Upward axis (screen +Y).</summary>
        public Vector3d Up;
        /// <summary>View-space depth axis (direction the camera/light looks).</summary>
        public Vector3d Forward;
    }

    /// <summary>Axis-aligned orthographic frustum expressed in view-space coordinates.</summary>
    public struct Frustum
    {
        /// <summary>Minimum view-space X (left edge).</summary>
        public float MinX;
        /// <summary>Maximum view-space X (right edge).</summary>
        public float MaxX;
        /// <summary>Minimum view-space Y (bottom edge).</summary>
        public float MinY;
        /// <summary>Maximum view-space Y (top edge).</summary>
        public float MaxY;
        /// <summary>Minimum view-space depth.</summary>
        public float MinZ;
        /// <summary>Maximum view-space depth.</summary>
        public float MaxZ;

        /// <summary>Width of the frustum in view space.</summary>
        public float RangeX => MaxX - MinX;
        /// <summary>Height of the frustum in view space.</summary>
        public float RangeY => MaxY - MinY;
        /// <summary>Depth extent of the frustum in view space.</summary>
        public float RangeZ => MaxZ - MinZ;
    }
}
