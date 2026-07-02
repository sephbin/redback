using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataJSON
{
    public class FilterJSONListComponent : RedbackGHBase
    {
        public FilterJSONListComponent() : base("Filter JSON List", "Filter JSON List",
            "Filters a list of JSON objects using expressions. Use & as placeholder for the object, e.g. &[\"key\"]==\"value\"", "2-Data - JSON") { }

        public override Guid ComponentGuid => new Guid("39b7bb81-5c4d-4b91-b14a-ba480cd10b69");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-FilterJSON.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Data", "D", "JSON objects to filter", GH_ParamAccess.list);
            p[0].Optional = true;
            p.AddTextParameter("Filter", "F", "Filter expressions (& = tested object). Any true expression includes the item.", GH_ParamAccess.list);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Data", "D", "Filtered JSON objects", GH_ParamAccess.list);
            p.AddIntegerParameter("Indices", "I", "Original indices of kept items", GH_ParamAccess.list);
            p.AddBooleanParameter("Mask", "P", "Boolean mask (true = kept)", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var data = new List<string>();
            var filters = new List<string>();
            DA.GetDataList(0, data);
            DA.GetDataList(1, filters);

            var kept = new List<string>();
            var indices = new List<int>();
            var mask = new List<bool>();

            for (int i = 0; i < data.Count; i++)
            {
                bool passes = false;
                try
                {
                    using var doc = JsonDocument.Parse(data[i] ?? "{}");
                    passes = EvalFilters(doc.RootElement, filters);
                }
                catch { }
                mask.Add(passes);
                if (passes) { kept.Add(data[i]); indices.Add(i); }
            }

            DA.SetDataList(0, kept);
            DA.SetDataList(1, indices);
            DA.SetDataList(2, mask);
        }

        private static bool EvalFilters(JsonElement obj, IList<string> filters)
        {
            foreach (var f in filters)
                try { if (EvalOne(obj, f)) return true; } catch { }
            return false;
        }

        private static bool EvalOne(JsonElement obj, string expr)
        {
            if (string.IsNullOrWhiteSpace(expr)) return false;
            expr = expr.Trim();

            // "value" in &["key"]
            var m = Regex.Match(expr, @"[""'](.+?)[""']\s+in\s+&\[[""'](.+?)[""']\]");
            if (m.Success)
            {
                string val = m.Groups[1].Value, key = m.Groups[2].Value;
                if (obj.TryGetProperty(key, out var p))
                    return (p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText())
                           ?.Contains(val) == true;
                return false;
            }

            // &["key"] OP "value" or number
            m = Regex.Match(expr, @"&\[[""'](.+?)[""']\]\s*(==|!=|>=|<=|>|<)\s*[""']?(.+?)[""']?\s*$");
            if (m.Success)
            {
                string key = m.Groups[1].Value, op = m.Groups[2].Value, rhs = m.Groups[3].Value.Trim('"', '\'');
                if (!obj.TryGetProperty(key, out var prop)) return false;
                string actual = prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.GetRawText();

                if (op == "==" ) return actual == rhs;
                if (op == "!=" ) return actual != rhs;

                if (double.TryParse(actual, out double a) && double.TryParse(rhs, out double b))
                {
                    if (op == ">" ) return a > b;
                    if (op == "<" ) return a < b;
                    if (op == ">=") return a >= b;
                    if (op == "<=") return a <= b;
                }
                return false;
            }

            // &["key"] â€” existence check
            m = Regex.Match(expr, @"&\[[""'](.+?)[""']\]");
            if (m.Success)
            {
                string key = m.Groups[1].Value;
                return obj.TryGetProperty(key, out var p) && p.ValueKind != JsonValueKind.Null;
            }

            return false;
        }
    }
}
