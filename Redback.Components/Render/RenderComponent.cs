using System;
using System.IO;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;

namespace Redback.Components.Render
{
    public class RenderComponent : RedbackGHBase
    {
        public RenderComponent()
            : base("Render", "Render",
                   "Captures the active Rhino view to an image file. " +
                   "Viewport capture mode is fast and batch-friendly with no popup. " +
                   "Full render mode invokes the active render engine (a render window may appear; " +
                   "the output path must also be set in Document Properties → Render → Output). " +
                   "Supply a Line (From = eye, To = target) or Plane (Origin = eye, ZAxis = look direction) " +
                   "to override the camera — the viewport is always restored after capture.",
                   "Render") { }

        public override Guid ComponentGuid => new Guid("e3f4a5b6-c7d8-4901-abcd-ef2345678902");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // 0
            pManager.AddBooleanParameter("Run", "R", "Set to True to trigger the render.", GH_ParamAccess.item);
            // 1
            pManager.AddTextParameter("Output Path", "P", "File path for the output image (.png or .jpg).", GH_ParamAccess.item);
            // 2
            pManager.AddIntegerParameter("Width", "W", "Output image width in pixels.", GH_ParamAccess.item, 1920);
            // 3
            pManager.AddIntegerParameter("Height", "H", "Output image height in pixels.", GH_ParamAccess.item, 1080);
            // 4
            pManager.AddBooleanParameter("Full Render", "FR",
                "False = fast viewport capture (recommended for batch use). " +
                "True = full Rhino render engine.",
                GH_ParamAccess.item, false);
            // 5
            pManager.AddGenericParameter("Camera", "C",
                "Optional Line (From = eye, To = target) or Plane (Origin = eye position, ZAxis = look direction). " +
                "If omitted, the active view camera is used unchanged.",
                GH_ParamAccess.item);
            pManager[5].Optional = true;
            // 6
            pManager.AddNumberParameter("FOV", "FOV",
                "Horizontal field of view in degrees. Applies to viewport capture only.",
                GH_ParamAccess.item);
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Saved Path", "SP", "Confirmed path of the saved image.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            if (!DA.GetData(0, ref run) || !run) return;

            string outputPath = null;
            if (!DA.GetData(1, ref outputPath) || string.IsNullOrWhiteSpace(outputPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Output path not provided.");
                return;
            }

            int width = 1920, height = 1080;
            DA.GetData(2, ref width);
            DA.GetData(3, ref height);
            if (width <= 0 || height <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Width and Height must be positive.");
                return;
            }

            bool fullRender = false;
            DA.GetData(4, ref fullRender);

            IGH_Goo cameraGoo = null;
            DA.GetData(5, ref cameraGoo);

            double fovDeg = 0;
            bool hasFov = DA.GetData(6, ref fovDeg) && fovDeg > 0;

            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active Rhino document.");
                return;
            }

            var view = doc.Views.ActiveView;
            if (view == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active Rhino view.");
                return;
            }

            var vp = view.ActiveViewport;
            var savedLoc    = vp.CameraLocation;
            var savedTarget = vp.CameraTarget;
            var savedLens   = vp.Camera35mmLensLength;
            bool cameraOverridden = false;

            try
            {
                if (cameraGoo != null)
                {
                    cameraOverridden = ApplyCamera(vp, cameraGoo, hasFov ? (double?)fovDeg : null);
                    if (!cameraOverridden)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            "Camera input must be a Line or Plane — camera input ignored.");
                }

                // Always redraw and flush before capture so the framebuffer is current.
                view.Redraw();
                RhinoApp.Wait();

                string savedPath = fullRender
                    ? DoFullRender(doc, width, height, outputPath)
                    : DoViewCapture(view, width, height, outputPath);

                DA.SetData(0, savedPath);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
            finally
            {
                if (cameraOverridden)
                {
                    vp.SetCameraLocation(savedLoc, false);
                    vp.SetCameraTarget(savedTarget, false);
                    vp.Camera35mmLensLength = savedLens;
                    view.Redraw();
                }
            }
        }

        private static bool ApplyCamera(RhinoViewport vp, IGH_Goo cameraGoo, double? fovDeg)
        {
            if (cameraGoo.CastTo(out Line line))
            {
                vp.SetCameraLocation(line.From, false);
                vp.SetCameraTarget(line.To, false);
            }
            else if (cameraGoo.CastTo(out Plane plane))
            {
                vp.SetCameraLocation(plane.Origin, false);
                // Use existing eye→target distance to place a target along the look direction
                double dist = vp.CameraLocation.DistanceTo(vp.CameraTarget);
                if (dist <= RhinoMath.ZeroTolerance) dist = 1.0;
                vp.SetCameraTarget(plane.Origin + plane.ZAxis * dist, false);
            }
            else
            {
                return false;
            }

            if (fovDeg.HasValue)
            {
                // Convert horizontal FOV to 35mm lens length (36mm sensor width)
                double halfRad = fovDeg.Value * 0.5 * Math.PI / 180.0;
                vp.Camera35mmLensLength = 18.0 / Math.Tan(halfRad);
            }
            return true;
        }

        private static string DoViewCapture(RhinoView view, int w, int h, string path)
        {
            var bmp = view.CaptureToBitmap(new System.Drawing.Size(w, h));
            if (bmp == null) throw new InvalidOperationException("CaptureToBitmap returned null bitmap.");

            EnsureDir(path);
            bmp.Save(path, ResolveFormat(path));
            return path;
        }

        private static string DoFullRender(RhinoDoc doc, int w, int h, string path)
        {
            var rs       = doc.RenderSettings;
            var prevSize = rs.ImageSize;
            rs.ImageSize = new System.Drawing.Size(w, h);

            try
            {
                EnsureDir(path);
                // Triggers the active render engine. The output file path is controlled by
                // Document Properties → Render → Output — set it there to match this component's path.
                RhinoApp.RunScript("-_Render", false);
            }
            finally
            {
                rs.ImageSize = prevSize;
            }

            return path;
        }

        private static void EnsureDir(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        private static System.Drawing.Imaging.ImageFormat ResolveFormat(string path) =>
            Path.GetExtension(path).ToLowerInvariant() == ".jpg"
                ? System.Drawing.Imaging.ImageFormat.Jpeg
                : System.Drawing.Imaging.ImageFormat.Png;
    }
}
