using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino.Geometry;

namespace Redback.Components.ArchJSON
{
    public class Geo2ArchComponent : RedbackGHBase
    {
        public Geo2ArchComponent() : base("Geo2Arch", "Geo2Arch",
            "Converts a list of geometry to an ArchJSON FeatureCollection", "1-ArchJSON") { }

        public override Guid ComponentGuid => new Guid("f7528ac2-c3ad-4d18-b22e-4bd6f035829a");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-Geo2ArchiJSON.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("Geometry", "GE", "List of geometry to convert", GH_ParamAccess.list);
            p[0].Optional = true;
            p.AddTextParameter("Properties", "PR", "List of JSON properties per geometry", GH_ParamAccess.list);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("JSON", "JO", "ArchJSON FeatureCollection", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var geomList = new List<IGH_Goo>();
            var prList = new List<string>();
            DA.GetDataList(0, geomList);
            DA.GetDataList(1, prList);

            if (prList.Count == 0) prList.Add("{}");
            for (int i = 0; i < prList.Count; i++)
            {
                if (string.IsNullOrEmpty(prList[i])) { prList[i] = "{}"; continue; }
                try { JsonDocument.Parse(prList[i]); }
                catch { prList[i] = "{\"class\":" + JsonSerializer.Serialize(prList[i]) + "}"; }
            }

            var features = new JsonArray();
            for (int i = 0; i < geomList.Count; i++)
            {
                string propStr = i < prList.Count ? prList[i] : prList[prList.Count - 1];
                JsonNode props;
                try { props = JsonNode.Parse(propStr); }
                catch { props = new JsonObject(); }

                var feature = ConvertGoo(geomList[i], props);
                if (feature != null) features.Add(feature);
            }

            var collection = new JsonObject();
            collection["type"] = "FeatureCollection";
            collection["features"] = features;
            DA.SetData(0, collection.ToJsonString());
        }

        private static JsonObject ConvertGoo(IGH_Goo goo, JsonNode props)
        {
            if (goo == null) return null;
            try
            {
                if (goo is GH_ObjectWrapper wrapper)
                {
                    if (wrapper.Value is ArchJsonText txt) return MakeTextFeature(txt, props);
                    if (wrapper.Value is ArchJsonImage img) return MakeImageFeature(img, props);
                    if (wrapper.Value is Plane wPl) return MakePlaneFeature(wPl, props);
                    if (wrapper.Value is Point3d wPt) return MakePointFeature(wPt, props);
                }
                if (goo is GH_Point pt) return MakePointFeature(pt.Value, props);
                if (goo is GH_Plane pl) return MakePlaneFeature(pl.Value, props);
                if (goo is GH_Curve crv)
                {
                    var curve = crv.Value;
                    if (curve is PolyCurve poly) return MakePolyCurveFeature(poly, props);
                    return MakeCurveFeature(curve, props);
                }
                if (goo.CastTo<Point3d>(out Point3d fallback)) return MakePointFeature(fallback, props);
            }
            catch { }
            return null;
        }

        private static JsonArray XYZ(Point3d pt) =>
            new JsonArray(JsonValue.Create(pt.X), JsonValue.Create(pt.Y), JsonValue.Create(pt.Z));

        private static JsonArray PlaneMatrix(Plane pl) => new JsonArray(
            JsonValue.Create(pl.XAxis.X), JsonValue.Create(pl.YAxis.X), JsonValue.Create(pl.ZAxis.X), JsonValue.Create(pl.Origin.X),
            JsonValue.Create(pl.XAxis.Y), JsonValue.Create(pl.YAxis.Y), JsonValue.Create(pl.ZAxis.Y), JsonValue.Create(pl.Origin.Y),
            JsonValue.Create(pl.XAxis.Z), JsonValue.Create(pl.YAxis.Z), JsonValue.Create(pl.ZAxis.Z), JsonValue.Create(pl.Origin.Z),
            JsonValue.Create(0.0), JsonValue.Create(0.0), JsonValue.Create(0.0), JsonValue.Create(1.0));

        private static JsonObject Feature(JsonObject geom, JsonNode props)
        {
            var f = new JsonObject();
            f["type"] = "Feature";
            f["geometry"] = geom;
            f["properties"] = props != null ? JsonNode.Parse(props.ToJsonString()) : new JsonObject();
            return f;
        }

        private static JsonObject MakePointFeature(Point3d pt, JsonNode props)
        {
            var geom = new JsonObject();
            geom["coordinates"] = XYZ(pt);
            geom["type"] = "Point";
            return Feature(geom, props);
        }

        private static JsonObject MakePlaneFeature(Plane pl, JsonNode props)
        {
            var geom = new JsonObject();
            geom["coordinates"] = PlaneMatrix(pl);
            geom["type"] = "Xform";
            return Feature(geom, props);
        }

        private static JsonObject MakeTextFeature(ArchJsonText txt, JsonNode props)
        {
            var geom = new JsonObject();
            geom["coordinates"] = PlaneMatrix(txt.Location);
            geom["type"] = "Text";
            geom["content"] = txt.Content;
            return Feature(geom, props);
        }

        private static JsonObject MakeImageFeature(ArchJsonImage img, JsonNode props)
        {
            var geom = new JsonObject();
            geom["coordinates"] = PlaneMatrix(img.Location);
            geom["type"] = "Image";
            geom["image"] = img.ImagePath;
            geom["width"] = img.Width;
            geom["height"] = img.Height;
            return Feature(geom, props);
        }

        private static JsonObject MakeCurveFeature(Curve curve, JsonNode props) =>
            Feature(BuildCurveGeometry(curve), props);

        private static JsonObject BuildCurveGeometry(Curve curve)
        {
            if (curve.IsCircle(global::Rhino.RhinoMath.ZeroTolerance) && curve.TryGetCircle(out Circle circle))
            {
                var g = new JsonObject();
                g["coordinates"] = XYZ(circle.Center);
                g["type"] = "Circle";
                g["radius"] = circle.Radius;
                return g;
            }

            if (curve.IsArc(global::Rhino.RhinoMath.ZeroTolerance) && curve.TryGetArc(out Arc arc))
            {
                var nc2 = curve.ToNurbsCurve();
                var ptArr = new JsonArray();
                for (int i = 0; i < nc2.Points.Count; i++)
                    ptArr.Add(XYZ(nc2.Points[i].Location));

                var g = new JsonObject();
                g["coordinates"] = ptArr;
                g["type"] = "Arc";
                g["deg"] = nc2.Degree;
                g["closed"] = false;
                g["radius"] = arc.Radius;
                g["isBig"] = arc.Angle > Math.PI;
                g["center"] = XYZ(arc.Center);
                g["angle"] = arc.Angle;
                return g;
            }

            var nc = curve.ToNurbsCurve();
            bool closed = curve.IsClosed;
            int degree = nc.Degree;
            int ptCount = nc.Points.Count;
            int ptStop = (degree == 1 && closed) ? ptCount - 1 : ptCount;

            var pts = new JsonArray();
            for (int i = 0; i < ptStop; i++)
                pts.Add(XYZ(nc.Points[i].Location));

            var geom = new JsonObject();
            geom["coordinates"] = pts;
            geom["type"] = (degree == 1 && ptStop == 2) ? "Line" : "Spline";
            geom["closed"] = closed;
            geom["deg"] = degree;
            return geom;
        }

        private static JsonObject MakePolyCurveFeature(PolyCurve polyCurve, JsonNode props)
        {
            int orientation = (int)polyCurve.ClosedCurveOrientation();
            var segments = polyCurve.Explode();

            var segGeoms = new JsonArray();
            foreach (var seg in segments)
                segGeoms.Add(BuildCurveGeometry(seg));

            var partGeom = new JsonObject();
            partGeom["coordinates"] = segGeoms;
            partGeom["type"] = "PolyCurvePart";
            partGeom["closed"] = orientation;

            var part = new JsonObject();
            part["type"] = "Feature";
            part["geometry"] = partGeom;

            var partsArr = new JsonArray();
            partsArr.Add(part);

            var geom = new JsonObject();
            geom["coordinates"] = partsArr;
            geom["type"] = "PolyCurve";
            return Feature(geom, props);
        }
    }
}
