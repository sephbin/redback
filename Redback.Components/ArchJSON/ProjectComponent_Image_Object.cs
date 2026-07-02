using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino.Geometry;

namespace Redback.Components.ArchJSON
{
    public class ImageObjectComponent : RedbackGHBase
    {
        public ImageObjectComponent() : base("Image Object", "Image Object",
            "Creates an image object that can be parsed by archJSON", "1-ArchJSON") { }

        public override Guid ComponentGuid => new Guid("dfb7e3c1-c7db-4cd1-bdfe-6f61b73b66fe");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-ImageObject.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddCurveParameter("Rectangle", "RE", "Rectangle curve", GH_ParamAccess.item);
            p[0].Optional = true;
            p.AddTextParameter("Image Path", "IP", "Path to image file", GH_ParamAccess.item);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("Image Object", "IO", "ArchJSON image object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve rect = null;
            string ip = null;
            DA.GetData(0, ref rect);
            DA.GetData(1, ref ip);
            if (rect == null) { DA.SetData(0, null); return; }
            DA.SetData(0, new GH_ObjectWrapper(new ArchJsonImage(rect, ip ?? "")));
        }
    }
}
