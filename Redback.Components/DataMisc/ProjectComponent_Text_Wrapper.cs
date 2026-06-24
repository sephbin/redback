using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataMisc
{
    public class TextWrapperComponent : RedbackGHBase
    {
        public TextWrapperComponent() : base("Text Wrapper", "Wrapper",
            "Wraps and joins text with selected data characters", "2-Data - Misc") { }

        public override Guid ComponentGuid => new Guid("cdec7349-8002-4ab6-8fbb-2c7d7ef8c72a");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Content", "CO", "Text content to wrap", GH_ParamAccess.list);
            p[0].Optional = true;
            p.AddTextParameter("Wrap Char", "WR", "Character that will wrap the text", GH_ParamAccess.item);
            p[1].Optional = true;
            p.AddTextParameter("Delimiter", "DE", "Join character; leave disconnected to wrap each item individually", GH_ParamAccess.item);
            p[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Result", "RE", "Wrapped text", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var content = new List<string>();
            string wrap = "";
            string delim = null;
            DA.GetDataList(0, content);
            DA.GetData(1, ref wrap);
            DA.GetData(2, ref delim);

            if (content == null) content = new List<string>();
            if (wrap == null) wrap = "";

            List<string> items = content;
            if (delim != null)
                items = new List<string> { string.Join(delim, content) };

            DA.SetDataList(0, items.Select(x => WrapText(x ?? "", wrap)).ToList());
        }

        private static string WrapText(string text, string wrap)
        {
            switch (wrap)
            {
                case "":   return "[" + text + "]";
                case "\"": return "\"" + text + "\"";
                case "'":  return "'" + text + "'";
                case "{":
                case "}":
                case "{}": return "{" + text + "}";
                case "[":
                case "]":
                case "[]": return "[" + text + "]";
                case "(":
                case ")":
                case "()": return "(" + text + ")";
                default:   return wrap + text + wrap;
            }
        }
    }
}
