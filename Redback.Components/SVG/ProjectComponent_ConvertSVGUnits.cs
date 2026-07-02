using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.SVG
{
    public class ConvertSVGUnitsComponent : RedbackGHBase
    {
        public ConvertSVGUnitsComponent() : base("ConvertSVGUnits", "ConvertSVGUnits",
            "Converts mm units in CSS to points for InDesign and Photoshop", "3-SVG") { }

        public override Guid ComponentGuid => new Guid("990df47d-b5b3-4c5d-831b-75ea31992127");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-ConvertSVGUnits.svg");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("CSS", "CSS", "CSS string with mm units", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("CSS", "CSS", "CSS with converted units", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string css = "";
            DA.GetData(0, ref css);
            if (string.IsNullOrEmpty(css)) { DA.SetData(0, css); return; }

            css = Regex.Replace(css, @"\d+(\.\d*)?mm", m =>
            {
                string numStr = m.Value.TrimEnd('m');
                if (double.TryParse(numStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
                    return (val * 0.352778).ToString(CultureInfo.InvariantCulture) + "mm";
                return m.Value;
            });
            DA.SetData(0, css);
        }
    }
}
