using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino.Geometry;

namespace Redback.Components.ArchJSON
{
    public class TextObjectComponent : RedbackGHBase
    {
        public TextObjectComponent() : base("Text Object", "Text Object",
            "Creates a text object that can be parsed by archJSON", "1-ArchJSON") { }

        public override Guid ComponentGuid => new Guid("fabdeb8d-b794-48e9-84ab-3f9923edfbf1");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("Plane", "PL", "Location plane", GH_ParamAccess.item);
            p[0].Optional = true;
            p.AddGenericParameter("Text", "TE", "Text content", GH_ParamAccess.item);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("Text Object", "TO", "ArchJSON text object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_Goo plGoo = null, teGoo = null;
            DA.GetData(0, ref plGoo);
            DA.GetData(1, ref teGoo);

            var pl = Plane.WorldXY;
            if (plGoo is GH_Plane ghPl) pl = ghPl.Value;
            else if (plGoo != null) plGoo.CastTo<Plane>(out pl);

            string te = "";
            if (teGoo is GH_String ghStr) te = ghStr.Value;
            else if (teGoo != null) te = teGoo.ToString();

            DA.SetData(0, new GH_ObjectWrapper(new ArchJsonText(pl, te)));
        }
    }
}
