using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Redback.Render;
using Rhino.Geometry;

namespace Redback.Components.Render
{
    /// <summary>
    /// Grasshopper component that performs CPU-based orthographic shadow-map rendering
    /// entirely in the background and writes the result as a PNG to disk.
    /// </summary>
    public class ShadowMapRendererComponent : RedbackGHBase
    {
        public ShadowMapRendererComponent()
            : base("Shadow Map Renderer", "ShadowMap",
                   "CPU orthographic shadow-map renderer. Renders meshes to a greyscale PNG " +
                   "with soft PCF shadows. The camera is positioned explicitly and the frame " +
                   "dimensions are in scene units, so there is no pixel stretching.",
                   "Render") { }

        public override Guid ComponentGuid => new Guid("a94b3e1c-7f25-4d68-90be-c12a3b4d5e6f");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // 0 — Meshes (required)
            pManager.AddMeshParameter(
                "Meshes", "M",
                "Geometry to render.",
                GH_ParamAccess.list);

            // 1 — CameraPos (optional; default = world origin)
            pManager.AddPointParameter(
                "CameraPos", "CP",
                "Centre of the output frame in world space.",
                GH_ParamAccess.item);
            pManager[1].Optional = true;

            // 2 — CameraDir (optional; default = top view)
            pManager.AddVectorParameter(
                "CameraDir", "CD",
                "Direction the camera looks (e.g. 0,0,-1 = top view).",
                GH_ParamAccess.item);
            pManager[2].Optional = true;

            // 3 — FrameWidth (required; scene units)
            pManager.AddNumberParameter(
                "FrameWidth", "FW",
                "Width of the rendered frame in scene units.",
                GH_ParamAccess.item);

            // 4 — FrameHeight (required; scene units)
            pManager.AddNumberParameter(
                "FrameHeight", "FH",
                "Height of the rendered frame in scene units.",
                GH_ParamAccess.item);

            // 5 — LightDir (optional; default = diagonal from upper-left)
            pManager.AddVectorParameter(
                "LightDir", "LD",
                "Direction FROM the light source TOWARD the scene.",
                GH_ParamAccess.item);
            pManager[5].Optional = true;

            // 6 — PixelWidth (optional; default 1024)
            pManager.AddIntegerParameter(
                "PixelWidth", "PW",
                "Output image width in pixels. " +
                "Height is auto-computed as round(PW × FrameHeight / FrameWidth) " +
                "so the pixel grid always matches the frame aspect ratio.",
                GH_ParamAccess.item, 1024);

            // 7 — FilePath (required)
            pManager.AddTextParameter(
                "FilePath", "P",
                "Output .png file path.",
                GH_ParamAccess.item);

            // 8 — ShadowRes (optional; default 1024)
            pManager.AddIntegerParameter(
                "ShadowRes", "SR",
                "Shadow-map resolution in pixels (higher = sharper shadow edges).",
                GH_ParamAccess.item, 1024);

            // 9 — Ambient (optional; default 0.25)
            pManager.AddNumberParameter(
                "Ambient", "AL",
                "Ambient light factor in [0, 1]. 0 = pitch black in shadow, 1 = no shadows.",
                GH_ParamAccess.item, 0.25);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(
                "SavedPath", "SP",
                "Confirmed path of the saved PNG image.",
                GH_ParamAccess.item);

            pManager.AddRectangleParameter(
                "ViewRect", "VR",
                "World-space frame rectangle for Rhino PictureFrame placement. " +
                "Exactly matches the rendered image in position and proportion.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                // ── Meshes ────────────────────────────────────────────────────
                var meshGoos = new List<GH_Mesh>();
                if (!DA.GetDataList(0, meshGoos) || meshGoos.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No meshes provided.");
                    return;
                }
                var meshes = meshGoos.Where(g => g?.Value != null).Select(g => g.Value).ToList();
                if (meshes.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "All meshes are null.");
                    return;
                }

                // ── CameraPos ─────────────────────────────────────────────────
                var camPosGoo = new GH_Point();
                bool hasCamPos = DA.GetData(1, ref camPosGoo);
                Point3d camPos = (hasCamPos && camPosGoo != null)
                    ? camPosGoo.Value
                    : Point3d.Origin;

                // ── CameraDir ─────────────────────────────────────────────────
                var camDirGoo = new GH_Vector();
                bool hasCamDir = DA.GetData(2, ref camDirGoo);
                Vector3d camDir = (hasCamDir && camDirGoo != null)
                    ? camDirGoo.Value
                    : new Vector3d(0, 0, -1);
                if (!camDir.Unitize()) camDir = new Vector3d(0, 0, -1);

                // ── FrameWidth ────────────────────────────────────────────────
                var fwGoo = new GH_Number();
                if (!DA.GetData(3, ref fwGoo) || fwGoo == null || fwGoo.Value <= 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "FrameWidth must be a positive scene-unit value.");
                    return;
                }
                double frameWidth = fwGoo.Value;

                // ── FrameHeight ───────────────────────────────────────────────
                var fhGoo = new GH_Number();
                if (!DA.GetData(4, ref fhGoo) || fhGoo == null || fhGoo.Value <= 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "FrameHeight must be a positive scene-unit value.");
                    return;
                }
                double frameHeight = fhGoo.Value;

                // ── LightDir ──────────────────────────────────────────────────
                var lightDirGoo = new GH_Vector();
                bool hasLightDir = DA.GetData(5, ref lightDirGoo);
                Vector3d lightDir = (hasLightDir && lightDirGoo != null)
                    ? lightDirGoo.Value
                    : new Vector3d(-0.5, -0.5, -0.707);
                if (!lightDir.Unitize()) lightDir = new Vector3d(-0.5, -0.5, -0.707);

                // ── PixelWidth ────────────────────────────────────────────────
                int pixelWidth = 1024;
                var pwGoo = new GH_Integer();
                if (DA.GetData(6, ref pwGoo) && pwGoo != null && pwGoo.Value > 0)
                    pixelWidth = pwGoo.Value;

                // ── FilePath ──────────────────────────────────────────────────
                var pathGoo = new GH_String();
                if (!DA.GetData(7, ref pathGoo) || pathGoo == null ||
                    string.IsNullOrWhiteSpace(pathGoo.Value))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File path not provided.");
                    return;
                }
                string filePath = pathGoo.Value;

                // ── ShadowRes ─────────────────────────────────────────────────
                int shadowRes = 1024;
                var srGoo = new GH_Integer();
                if (DA.GetData(8, ref srGoo) && srGoo != null && srGoo.Value > 0)
                    shadowRes = srGoo.Value;

                // ── Ambient ───────────────────────────────────────────────────
                double ambient = 0.25;
                var alGoo = new GH_Number();
                if (DA.GetData(9, ref alGoo) && alGoo != null)
                    ambient = alGoo.Value;
                ambient = Math.Max(0.0, Math.Min(1.0, ambient));

                // ── Render ────────────────────────────────────────────────────
                var tris = ShadowMapCore.CollectTriangles(meshes);
                if (tris.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "Meshes produced no renderable triangles.");
                    return;
                }

                var camView   = ShadowMapCore.BuildBasis(camDir);
                var lightView = ShadowMapCore.BuildBasis(lightDir);

                Rectangle3d rect = ShadowMapCore.Render(
                    tris,
                    camView, camPos, frameWidth, frameHeight,
                    lightView,
                    pixelWidth, shadowRes,
                    (float)ambient, filePath);

                // ── Outputs ───────────────────────────────────────────────────
                DA.SetData(0, filePath);
                DA.SetData(1, rect);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }
    }
}
