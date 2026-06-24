using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataList
{
    public class List2TreeComponent : RedbackGHBase
    {
        public List2TreeComponent() : base("List2Tree", "List2Tree",
            "Converts a Grasshopper data tree to a tree of text strings", "2-Data - List") { }

        public override Guid ComponentGuid => new Guid("84ebc8e8-e298-4725-93a8-f5eb048d232b");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("List", "L", "Data tree to convert to strings", GH_ParamAccess.tree);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Tree", "T", "Tree of string values (same structure as input)", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var inTree = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out inTree);

            var outTree = new GH_Structure<GH_String>();
            foreach (var path in inTree.Paths)
            {
                var branch = inTree.get_Branch(path);
                var outBranch = new List<GH_String>();
                foreach (var item in branch)
                {
                    var goo = item as IGH_Goo;
                    outBranch.Add(goo != null ? new GH_String(GooToString(goo)) : null);
                }
                outTree.AppendRange(outBranch, path);
            }
            DA.SetDataTree(0, outTree);
        }

        private static string GooToString(IGH_Goo goo)
        {
            if (goo is GH_String gs) return gs.Value;
            if (goo.CastTo<string>(out string s)) return s;
            return goo.ToString();
        }
    }
}
