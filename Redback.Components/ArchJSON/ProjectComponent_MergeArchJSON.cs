using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.ArchJSON
{
    public class MergeArchJSONComponent : RedbackGHBase
    {
        public MergeArchJSONComponent() : base("MergeArchJSON", "MergeArchJSON",
            "Merges multiple ArchJSON objects by combining their features arrays", "1-ArchJSON") { }

        public override Guid ComponentGuid => new Guid("e8cf105b-5634-46b7-bd28-3813ade452a5");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("ArchJSON", "AJ", "ArchJSON objects to merge", GH_ParamAccess.list);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("ArchJSON", "AJ", "Merged ArchJSON object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var items = new List<string>();
            DA.GetDataList(0, items);
            if (items == null || items.Count == 0) { DA.SetData(0, null); return; }

            try
            {
                var first = JsonNode.Parse(items[0]).AsObject();
                var features = first["features"]?.AsArray() ?? new JsonArray();

                for (int i = 1; i < items.Count; i++)
                {
                    var next = JsonNode.Parse(items[i]).AsObject();
                    var nextFeatures = next["features"]?.AsArray();
                    if (nextFeatures != null)
                        foreach (var f in nextFeatures)
                            features.Add(f == null ? null : JsonNode.Parse(f.ToJsonString()));
                }

                first["features"] = features;
                DA.SetData(0, first.ToJsonString());
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
        }
    }
}
