using System;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataJSON
{
    public class SerializeModelObjectComponent : RedbackGHBase
    {
        public SerializeModelObjectComponent() : base("Serialize Model Object", "Serialize Model Object",
            "Serializes a Rhino model object to JSON", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("20dc8abf-ea08-46ad-82fa-cd1efd8023e1");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-SerialiseModelObject-01.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("OB", "OB", "Rhino model object", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("JO", "JO", "Serialized JSON object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_Goo goo = null;
            DA.GetData(0, ref goo);
            if (goo == null) return;

            global::Grasshopper.Rhinoceros.Model.ModelObject ob = null;
            if (goo is GH_ObjectWrapper wrapper)
                ob = wrapper.Value as global::Grasshopper.Rhinoceros.Model.ModelObject;
            if (ob == null) return;

            var doc = global::Rhino.RhinoDoc.ActiveDoc;
            var rhinoObj = doc?.Objects.FindId(ob.Id.GetValueOrDefault());
            var rhinoAttrs = rhinoObj?.Attributes;
            var layer = rhinoAttrs != null ? doc?.Layers.FindIndex(rhinoAttrs.LayerIndex) : null;

            var props = new JsonObject();
            props["_rhino3D_layerName"] = layer?.Name ?? "";
            props["_rhino3D_layerPath"] = layer?.FullPath ?? "";
            props["_rhino3D_objectName"] = rhinoAttrs?.Name ?? "";

            var userStrings = rhinoAttrs?.GetUserStrings();
            if (userStrings != null)
                foreach (string key in userStrings.AllKeys)
                    if (key != null && !props.ContainsKey(key))
                        props[key] = JsonValue.Create(userStrings[key]);

            string geomJson = "{}";
            try { geomJson = rhinoObj?.Geometry?.ToJSON(null) ?? "{}"; }
            catch { }

            var result = new JsonObject();
            try { result["geometry"] = JsonNode.Parse(geomJson); }
            catch { result["geometry"] = new JsonObject(); }
            result["properties"] = props;

            DA.SetData(0, result.ToJsonString());
        }
    }
}
