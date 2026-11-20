using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataList
{
    /// <summary>
    /// Inverse of Tree2List: parses each item as JSON, turns JSON arrays into
    /// branches (nested arrays become deeper paths), re-serialises JSON objects
    /// as text, and collapses single-element wrappers.
    /// </summary>
    public class List2TreeComponent : RedbackGHBase
    {
        public List2TreeComponent() : base("List2Tree", "List2Tree",
            "Converts JSON arrays into Grasshopper tree branches (nested arrays become deeper paths)", "2-Data - List") { }

        public override Guid ComponentGuid => new Guid("84ebc8e8-e298-4725-93a8-f5eb048d232b");
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-List2Tree.png");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("List", "L", "JSON array strings (or a tree of them) to expand into branches", GH_ParamAccess.tree);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Tree", "T", "Tree with one branch per JSON array", GH_ParamAccess.tree);
        }

        private static readonly JsonSerializerOptions s_jsonOut = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // A node is either a List<object> (branch) or a string leaf; null marks a hole.
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataTree(0, out GH_Structure<IGH_Goo> inTree);
            var outTree = new GH_Structure<GH_String>();
            if (inTree == null || inTree.DataCount == 0) { DA.SetDataTree(0, outTree); return; }

            // 1. Rebuild the input tree as nested lists, parsing each item as JSON.
            var root = new List<object>();
            foreach (var path in inTree.Paths)
            {
                var branch = new List<object>();
                foreach (var item in inTree.get_Branch(path))
                    branch.Add(ParseItem(item as IGH_Goo));
                Place(root, path.Indices, branch);
            }

            // 2. Collapse single-element wrappers, then 3. emit with paths starting at {0}.
            var simplified = Simplify(root);
            if (simplified is List<object> list)
                Emit(list, new List<int> { 0 }, outTree);
            else if (simplified is string s)
                outTree.Append(new GH_String(s), new GH_Path(0));

            DA.SetDataTree(0, outTree);
        }

        private static void Place(List<object> root, int[] indices, List<object> branch)
        {
            var current = root;
            for (int level = 0; level < indices.Length; level++)
            {
                int target = indices[level];
                while (current.Count <= target) current.Add(null);

                if (level == indices.Length - 1)
                {
                    if (current[target] != null) current.Insert(target, branch);
                    else current[target] = branch;
                    return;
                }

                if (!(current[target] is List<object> next))
                {
                    next = new List<object>();
                    current[target] = next;
                }
                current = next;
            }
        }

        private static object ParseItem(IGH_Goo goo)
        {
            if (goo == null) return null;
            string s = GooToString(goo);
            if (s == null) return null;

            try
            {
                using (var doc = JsonDocument.Parse(s))
                    return FromJson(doc.RootElement);
            }
            catch (JsonException)
            {
                return s; // not JSON: keep raw text
            }
        }

        private static object FromJson(JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var child in e.EnumerateArray()) list.Add(FromJson(child));
                    return list;
                case JsonValueKind.Object:
                    return JsonSerializer.Serialize(e, s_jsonOut);
                case JsonValueKind.String:
                    return e.GetString();
                case JsonValueKind.True:
                    return "True";
                case JsonValueKind.False:
                    return "False";
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    return e.GetRawText(); // numbers keep their JSON spelling
            }
        }

        private static object Simplify(object node)
        {
            if (!(node is List<object> list)) return node;
            object simplified = list.ConvertAll(Simplify);
            while (simplified is List<object> l && l.Count == 1 && l[0] is List<object> only)
                simplified = only;
            return simplified;
        }

        private static void Emit(List<object> items, List<int> track, GH_Structure<GH_String> tree)
        {
            var path = new GH_Path(track.ToArray());
            for (int i = 0; i < items.Count; i++)
            {
                switch (items[i])
                {
                    case List<object> sub:
                        track.Add(i);
                        Emit(sub, track, tree);
                        track.RemoveAt(track.Count - 1);
                        break;
                    case string s:
                        tree.Append(new GH_String(s), path);
                        break;
                    // null: hole, skipped (index still counts for sub-branches)
                }
            }
        }

        private static string GooToString(IGH_Goo goo)
        {
            if (goo is GH_String gs) return gs.Value;
            if (goo.CastTo<string>(out string s)) return s;
            return goo.ToString();
        }
    }
}
