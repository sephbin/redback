using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino.Geometry;

namespace Redback.Components.SVG
{
    public class SVGLayoutComponent : RedbackGHBase
    {
        public SVGLayoutComponent() : base("SVG Layout", "SVG Layout",
            "Creates a SVG layout dictionary", "3-SVG") { }

        public override Guid ComponentGuid => new Guid("c9354d5d-1daf-4b2b-a241-fdd8de4464bb");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        private static readonly Dictionary<string, (double w, double h)> PageLUT = new Dictionary<string, (double, double)>
        {
            ["A5"] = (210, 148), ["A5 (L)"] = (210, 148), ["A5 (P)"] = (148, 210),
            ["A4"] = (297, 210), ["A4 (L)"] = (297, 210), ["A4 (P)"] = (210, 297),
            ["A3"] = (420, 297), ["A3 (L)"] = (420, 297), ["A3 (P)"] = (297, 420),
            ["A2"] = (594, 420), ["A2 (L)"] = (594, 420), ["A2 (P)"] = (420, 594),
            ["A1"] = (841, 594), ["A1 (L)"] = (841, 594), ["A1 (P)"] = (594, 841),
            ["A0"] = (1189, 841), ["A0 (L)"] = (1189, 841), ["A0 (P)"] = (841, 1189),
        };

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddPlaneParameter("P", "P", "Location plane", GH_ParamAccess.item);
            p[0].Optional = true;
            p.AddGenericParameter("D", "D", "Paper dimensions (e.g. 'A4 (L)')", GH_ParamAccess.item);
            p[1].Optional = true;
            p.AddGenericParameter("S", "S", "Scale ratio (e.g. '1:100')", GH_ParamAccess.item);
            p[2].Optional = true;
            p.AddGenericParameter("L", "L", "If true, center page on plane", GH_ParamAccess.item);
            p[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("L", "L", "JSON layout string", GH_ParamAccess.item);
            p.AddCurveParameter("R", "R", "Page border rectangle", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var plane = Plane.WorldXY;
            if (!DA.GetData(0, ref plane)) return;

            IGH_Goo dimGoo = null, scaleGoo = null, locGoo = null;
            DA.GetData(1, ref dimGoo);
            DA.GetData(2, ref scaleGoo);
            DA.GetData(3, ref locGoo);

            string dim = dimGoo != null ? dimGoo.ToString() : null;
            if (string.IsNullOrEmpty(dim)) dim = "A3 (L)";

            string scale = scaleGoo != null ? scaleGoo.ToString() : null;
            if (string.IsNullOrEmpty(scale)) scale = "1:1";

            bool location = true;
            if (locGoo is GH_Boolean ghBool) location = ghBool.Value;
            else if (locGoo != null) locGoo.CastTo(out location);

            double width, height;
            if (PageLUT.TryGetValue(dim, out var dims))
            {
                width = dims.w; height = dims.h;
            }
            else
            {
                var parts = dim.Split(',');
                if (parts.Length < 2 ||
                    !double.TryParse(parts[0].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out width) ||
                    !double.TryParse(parts[1].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out height))
                    return;
            }

            var scaleParts = scale.Split(':');
            if (scaleParts.Length < 2 ||
                !double.TryParse(scaleParts[0].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out double scaleNum) ||
                !double.TryParse(scaleParts[1].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out double scaleDen))
                return;
            double pageScale = scaleDen / scaleNum;

            var doc = global::Rhino.RhinoDoc.ActiveDoc;
            double unitRatio = doc != null
                ? global::Rhino.RhinoMath.UnitScale(global::Rhino.UnitSystem.Millimeters, doc.ModelUnitSystem)
                : 1.0;
            double combScale = unitRatio * pageScale;
            double swidth = width * combScale;
            double sheight = height * combScale;

            Point3d positionedOrig, svgOrig;
            if (location)
            {
                positionedOrig = plane.PointAt(-swidth / 2.0, -sheight / 2.0, 0);
                svgOrig = plane.PointAt(-swidth / 2.0, sheight / 2.0, 0);
            }
            else
            {
                positionedOrig = plane.PointAt(0, -sheight, 0);
                svgOrig = plane.PointAt(0, 0, 0);
            }

            var rectPlane = plane;
            rectPlane.Origin = positionedOrig;
            var rect = new Rectangle3d(rectPlane, swidth, sheight);

            var svgPlane = plane;
            svgPlane.Origin = svgOrig;

            var planeArr = new JsonArray(
                new JsonArray(JsonValue.Create(svgPlane.Origin.X), JsonValue.Create(svgPlane.Origin.Y), JsonValue.Create(svgPlane.Origin.Z)),
                new JsonArray(JsonValue.Create(svgPlane.XAxis.X), JsonValue.Create(svgPlane.XAxis.Y), JsonValue.Create(svgPlane.XAxis.Z)),
                new JsonArray(JsonValue.Create(svgPlane.YAxis.X), JsonValue.Create(svgPlane.YAxis.Y), JsonValue.Create(svgPlane.YAxis.Z))
            );
            var layout = new JsonObject();
            layout["plane"] = planeArr;
            layout["page"] = new JsonArray(JsonValue.Create(width), JsonValue.Create(height));
            layout["scale"] = JsonValue.Create(combScale);

            DA.SetData(0, layout.ToJsonString());
            DA.SetData(1, rect.ToNurbsCurve());
        }
    }
}
