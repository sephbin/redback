using System;
using System.Collections.Generic;
using System.Text.Json;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataJSON
{
    public class JSONKeysComponent : RedbackGHBase
    {
        public JSONKeysComponent() : base("JSON Keys", "JSON Keys",
            "Returns all top-level keys from a JSON object", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("1863b4e0-dc8f-4542-b729-3b50878b3954");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-JSONKeys.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("JSON", "JO", "JSON object to inspect", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Keys", "K", "Top-level keys of the JSON object", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string json = null;
            DA.GetData(0, ref json);
            if (string.IsNullOrEmpty(json)) { DA.SetDataList(0, new List<string>()); return; }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var keys = new List<string>();
                foreach (var prop in doc.RootElement.EnumerateObject())
                    keys.Add(prop.Name);
                DA.SetDataList(0, keys);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
        }
    }
}
