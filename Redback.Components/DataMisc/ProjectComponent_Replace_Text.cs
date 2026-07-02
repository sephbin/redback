using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataMisc
{
    public class ReplaceTextComponent : RedbackGHBase
    {
        public ReplaceTextComponent() : base("Replace Text", "Replace Text",
            "Replaces text values in a list", "2-Data - Misc") { }

        public override Guid ComponentGuid => new Guid("b477f946-0652-476e-aebd-efbb5be28254");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-ReplaceText.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Value", "V", "Values to replace in", GH_ParamAccess.list);
            p[0].Optional = true;
            p.AddTextParameter("From", "F", "Text to replace from", GH_ParamAccess.list);
            p[1].Optional = true;
            p.AddTextParameter("To", "T", "Text to replace with", GH_ParamAccess.list);
            p[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Value", "V", "Modified values", GH_ParamAccess.list);
            p.AddTextParameter("Order", "O", "HTML entity codes for replaced characters", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var values = new List<string>();
            var from = new List<string>();
            var to = new List<string>();
            DA.GetDataList(0, values);
            DA.GetDataList(1, from);
            DA.GetDataList(2, to);

            var chars = new SortedSet<char>();
            foreach (var f in from)
                if (f != null)
                    foreach (char c in f)
                        chars.Add(c);

            var order = new List<string>();
            foreach (char c in chars)
                order.Add("&#" + (int)c + ";");

            var result = new List<string>(values);
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] == null) continue;
                string v = result[i];
                int len = Math.Min(from.Count, to.Count);
                for (int j = 0; j < len; j++)
                    if (from[j] != null && to[j] != null)
                        v = v.Replace(from[j], to[j]);
                result[i] = v;
            }

            DA.SetDataList(0, result);
            DA.SetDataList(1, order);
        }
    }
}
