using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataList
{
    public class Tree2ListComponent : RedbackGHBase
    {
        public Tree2ListComponent() : base("Tree2List", "Tree2List",
            "Groups tree branches by path depth into a list of JSON arrays", "2-Data - List") { }

        public override Guid ComponentGuid => new Guid("85852905-7564-4610-9f9c-76fc94a5ad7a");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-TreeToList.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("Tree", "T", "Data tree to flatten", GH_ParamAccess.tree);
            p[0].Optional = true;
            p.AddIntegerParameter("Branch Level", "B", "Number of path levels to group by (0 = all branches in one array)", GH_ParamAccess.item, 0);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("List", "L", "List of JSON arrays grouped by branch level", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var inTree = new GH_Structure<IGH_Goo>();
            int b = 0;
            DA.GetDataTree(0, out inTree);
            DA.GetData(1, ref b);
            b = Math.Max(0, b);

            // Group branches by first B path indices (maintain insertion order)
            var groupOrder = new List<string>();
            var groups = new Dictionary<string, List<IGH_Goo>>();

            foreach (var path in inTree.Paths)
            {
                string key = MakeGroupKey(path, b);
                if (!groups.TryGetValue(key, out var list))
                {
                    list = new List<IGH_Goo>();
                    groups[key] = list;
                    groupOrder.Add(key);
                }
                foreach (var item in inTree.get_Branch(path))
                    list.Add(item as IGH_Goo);
            }

            var results = new List<string>();
            foreach (var key in groupOrder)
            {
                var arr = new JsonArray();
                foreach (var goo in groups[key])
                {
                    string s = GooToString(goo);
                    try { arr.Add(JsonNode.Parse(s)); }
                    catch { arr.Add(s); }
                }
                results.Add(arr.ToJsonString());
            }

            DA.SetDataList(0, results);
        }

        private static string MakeGroupKey(GH_Path path, int depth)
        {
            if (depth <= 0 || path.Length == 0) return "";
            int take = Math.Min(depth, path.Length);
            return string.Join(",", path.Indices, 0, take);
        }

        private static string GooToString(IGH_Goo goo)
        {
            if (goo == null) return "null";
            if (goo is GH_String gs) return gs.Value;
            if (goo.CastTo<string>(out string s)) return s;
            return goo.ToString();
        }
    }
}
