using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.SVG
{
    public class StyleSVGComponent : RedbackGHBase
    {
        public StyleSVGComponent() : base("StyleSVG", "StyleSVG",
            "Injects CSS styles into SVG lines at <preStyle/> markers", "3-SVG") { }

        public override Guid ComponentGuid => new Guid("ef5a6f3f-63b0-4c89-b503-d75075096ec3");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-StyleSVG.svg");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("SVG", "SVG", "List of SVG lines", GH_ParamAccess.list);
            p[0].Optional = true;
            p.AddTextParameter("CSS", "CSS", "List of CSS lines to inject", GH_ParamAccess.list);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("SVG", "SVG", "Modified SVG lines", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var svgList = new List<string>();
            var cssList = new List<string>();
            DA.GetDataList(0, svgList);
            DA.GetDataList(1, cssList);

            string cssJoined = string.Join("", cssList);
            for (int i = 0; i < svgList.Count; i++)
            {
                if (svgList[i] != null && svgList[i].Trim().Contains("<preStyle/>"))
                    svgList[i] = cssJoined;
            }
            DA.SetDataList(0, svgList);
        }
    }
}
