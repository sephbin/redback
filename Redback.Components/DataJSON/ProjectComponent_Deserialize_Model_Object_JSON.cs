using System;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Redback.Components.DataJSON
{
    public class DeserializeModelObjectJSONComponent : RedbackGHBase
    {
        public DeserializeModelObjectJSONComponent() : base("Deserialize Model Object JSON", "Deserialize Model Object JSON",
            "Deserializes a Rhino model object from JSON", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("b465cd09-8473-452a-8f75-a1c9092adbc4");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-DeserialiseModelObject-01.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("JO", "JO", "JSON object string", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("OB", "OB", "Deserialized Rhino model object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string jo = null;
            DA.GetData(0, ref jo);
            if (string.IsNullOrEmpty(jo)) return;

            JsonObject jObj;
            try { jObj = JsonNode.Parse(jo).AsObject(); }
            catch { return; }

            string geomJson = jObj["geometry"]?.ToJsonString() ?? "{}";
            GeometryBase geom = null;
            try { geom = GeometryBase.FromJSON(geomJson) as GeometryBase; }
            catch { }

            var attrs = new ObjectAttributes();
            var propsNode = jObj["properties"]?.AsObject();
            if (propsNode != null)
            {
                foreach (var kvp in propsNode)
                {
                    if (!kvp.Key.StartsWith("_"))
                        attrs.SetUserString(kvp.Key, kvp.Value?.GetValue<string>() ?? "");
                }
                if (propsNode["_rhino3D_objectName"] is JsonNode nameNode)
                    attrs.Name = nameNode.GetValue<string>() ?? "";
            }

            var doc = global::Rhino.RhinoDoc.ActiveDoc;
            if (doc == null || geom == null) return;

            var newObj = new global::Grasshopper.Rhinoceros.Model.ModelObject(doc, attrs, geom);
            DA.SetData(0, new GH_ObjectWrapper(newObj));
        }
    }
}
