using System;
using System.Collections.Generic;
using System.Text.Json;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataJSON
{
    public class ReadJSONComponent : RedbackGHBase
    {
        public ReadJSONComponent() : base("Read JSON", "Read JSON",
            "Reads a key from a JSON string using dot-notation (a.b.0.c)", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("8550d131-c087-4402-a849-c005997a51e4");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-ReadJson.svg");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("JSON Object", "JO", "JSON objects to read from", GH_ParamAccess.list);
            p[0].Optional = true;
            p.AddTextParameter("Keys", "K", "Dot-notation key path (e.g. a.b.0)", GH_ParamAccess.item);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Values", "V", "Values read from the JSON", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var jsonList = new List<string>();
            string key = null;
            DA.GetDataList(0, jsonList);
            DA.GetData(1, ref key);

            var results = new List<string>();
            foreach (var jo in jsonList)
            {
                if (jo == null) { results.Add(null); continue; }
                try
                {
                    using var doc = JsonDocument.Parse(jo);
                    var val = DeepGet(doc.RootElement, key);
                    if (val == null) { results.Add(null); continue; }

                    string s = val.Value.ValueKind switch
                    {
                        JsonValueKind.String => val.Value.GetString(),
                        JsonValueKind.Array  => val.Value.GetRawText(),
                        JsonValueKind.Object => val.Value.GetRawText(),
                        JsonValueKind.Null   => null,
                        _                   => val.Value.GetRawText(),
                    };
                    results.Add(s);
                }
                catch { results.Add(null); }
            }
            DA.SetDataList(0, results);
        }

        private static JsonElement? DeepGet(JsonElement element, string key)
        {
            if (key == null) return element;
            foreach (var part in key.Split('.'))
            {
                if (int.TryParse(part, out int idx))
                {
                    if (element.ValueKind == JsonValueKind.Array && idx < element.GetArrayLength())
                        element = element[idx];
                    else return null;
                }
                else
                {
                    if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(part, out var prop))
                        element = prop;
                    else return null;
                }
            }
            return element;
        }
    }
}
