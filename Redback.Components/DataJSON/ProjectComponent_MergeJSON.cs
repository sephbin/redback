using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataJSON
{
    public class MergeJSONComponent : RedbackGHBase
    {
        public MergeJSONComponent() : base("MergeJSON", "MergeJSON",
            "Merges a list of JSON objects into one (later entries override earlier)", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("aa6a1a4d-feda-471d-b9bb-9b1f49b007e5");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-MergeJSON.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("JSON", "J", "JSON objects to merge", GH_ParamAccess.list);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("JSON", "J", "Merged JSON object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var items = new List<string>();
            DA.GetDataList(0, items);
            if (items == null || items.Count == 0) { DA.SetData(0, null); return; }

            try
            {
                var merged = JsonNode.Parse(items[0]).AsObject();
                for (int i = 1; i < items.Count; i++)
                {
                    var next = JsonNode.Parse(items[i]).AsObject();
                    foreach (var kvp in next)
                        merged[kvp.Key] = kvp.Value == null ? null : JsonNode.Parse(kvp.Value.ToJsonString());
                }
                DA.SetData(0, merged.ToJsonString());
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
        }
    }
}
