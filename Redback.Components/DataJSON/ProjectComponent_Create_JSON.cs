using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataJSON
{
    public class CreateJSONComponent : RedbackGHBase
    {
        public CreateJSONComponent() : base("Create JSON", "JSON",
            "Creates a JSON string from matching lists of keys and values", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("2cff7366-09b5-4bde-8fae-bc8396aabbfd");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-CreateJson.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Keys", "K", "List of keys", GH_ParamAccess.list);
            p[0].Optional = true;
            p.AddTextParameter("Values", "V", "List of values (JSON strings are kept as objects)", GH_ParamAccess.list);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("JSON", "JO", "Output JSON object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var keys = new List<string>();
            var values = new List<string>();
            DA.GetDataList(0, keys);
            DA.GetDataList(1, values);

            var obj = new JsonObject();
            for (int i = 0; i < keys.Count; i++)
            {
                string k = keys[i];
                if (k == null) continue;
                string v = i < values.Count ? values[i] : null;

                JsonNode node = null;
                if (v != null)
                {
                    try { node = JsonNode.Parse(v); }
                    catch { node = JsonValue.Create(v); }
                }
                obj[k] = node;
            }
            DA.SetData(0, obj.ToJsonString());
        }
    }
}
