using System;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Redback.Components.Grasshopper.Gradient
{
    public class EvaluateGradientComponent : RedbackGHBase
    {
        public EvaluateGradientComponent()
            : base("Evaluate Gradient", "EvalGrad",
                   "Evaluates a gradient at one or more values and returns the corresponding colour(s).",
                   "Gradient") { }

        public override Guid ComponentGuid => new Guid("c3d4e5f6-a7b8-9012-cdef-123456789013");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RedbackGradientParam(), "Gradient", "Gr",
                "The gradient to evaluate.", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Domain", "D",
                "The numeric domain to map the input value across. Overrides the gradient's own domain.",
                GH_ParamAccess.item, new Interval(0, 1));
            pManager.AddNumberParameter("Value", "t",
                "The value to evaluate within the domain.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("Colour", "C",
                "The colour at the given value.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var goo = new RedbackGradientGoo();
            if (!DA.GetData(0, ref goo) || goo == null || !goo.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No gradient connected.");
                return;
            }

            var domainGoo = new GH_Interval();
            DA.GetData(1, ref domainGoo);
            var domain = domainGoo?.Value ?? new Interval(0, 1);

            double value = 0.0;
            if (!DA.GetData(2, ref value)) return;

            double range = domain.T1 - domain.T0;
            double t = range == 0.0 ? 0.5 : (value - domain.T0) / range;
            t = Math.Max(0.0, Math.Min(1.0, t));

            Color colour = goo.Value.Gradient.ColourAt(t);
            DA.SetData(0, colour);
        }
    }
}
