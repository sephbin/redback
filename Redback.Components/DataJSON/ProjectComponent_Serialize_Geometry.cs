using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino.Geometry;

namespace Redback.Components.DataJSON
{
    public class SerializeGeometryComponent : RedbackGHBase
    {
        public SerializeGeometryComponent() : base("Serialize Geometry", "Serialize Geometry",
            "Serializes Rhino geometry to a JSON string with an optional properties object", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("dba33b97-43c6-4628-a4d8-0bae9cd64851");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-SerialiseGeometry-01.svg");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGeometryParameter("Geometry", "GE", "Rhino geometry to serialize", GH_ParamAccess.item);
            p[0].Optional = true;
            p.AddTextParameter("Properties", "PR", "JSON properties object to attach", GH_ParamAccess.item);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("JSON Object", "JO", "Serialized geometry JSON", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase geom = null;
            string prStr = null;
            DA.GetData(0, ref geom);
            DA.GetData(1, ref prStr);

            if (geom == null) { DA.SetData(0, null); return; }

            try
            {
                JsonNode prNode;
                if (string.IsNullOrEmpty(prStr))
                    prNode = new JsonObject();
                else
                    try { prNode = JsonNode.Parse(prStr); }
                    catch { prNode = new JsonObject(); }

                var dataNode = JsonNode.Parse(geom.ToJSON(null)).AsObject();
                dataNode["properties"] = prNode;
                DA.SetData(0, dataNode.ToJsonString());
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
        }
    }
}
