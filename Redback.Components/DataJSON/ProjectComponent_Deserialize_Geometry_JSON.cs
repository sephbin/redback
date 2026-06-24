using System;
using System.Text.Json;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;
using Rhino.Geometry;

namespace Redback.Components.DataJSON
{
    public class DeserializeGeometryJSONComponent : RedbackGHBase
    {
        public DeserializeGeometryJSONComponent() : base("Deserialize Geometry JSON", "Deserialize Geometry JSON",
            "Deserializes Rhino geometry from a JSON string", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("0d7e9383-771a-4e12-9bb5-f044b388dbd1");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("JSON", "JO", "Geometry JSON string (from Serialize Geometry)", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGeometryParameter("Geometry", "GE", "Deserialized geometry", GH_ParamAccess.item);
            p.AddTextParameter("Properties", "PR", "Properties JSON string", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string jo = null;
            DA.GetData(0, ref jo);
            if (string.IsNullOrEmpty(jo)) { DA.SetData(0, null); DA.SetData(1, null); return; }

            try
            {
                using var doc = JsonDocument.Parse(jo);
                string prJson = "{}";
                if (doc.RootElement.TryGetProperty("properties", out var prEl))
                    prJson = prEl.GetRawText();

                var geom = GeometryBase.FromJSON(jo);
                DA.SetData(0, geom);
                DA.SetData(1, prJson);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
        }
    }
}
