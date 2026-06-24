using System;
using System.Drawing;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using GHGradient = Grasshopper.GUI.Gradient.GH_Gradient;

namespace Redback.Components.Grasshopper.Gradient
{
    public class CreateGradientComponent : GH_GradientControl
    {
        private static readonly FieldInfo GradientField =
            typeof(GH_GradientControl).GetField("m_gradient",
                BindingFlags.Instance | BindingFlags.NonPublic);

        public CreateGradientComponent() { }

        public override Guid ComponentGuid => new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678902");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public override string Category    => "Redback";
        public override string SubCategory => "Gradient";
        public override string Name        => "Create Gradient";
        public override string NickName    => "Grad";
        public override string Description =>
            "Defines a colour gradient. Click the bar to add grips; drag grips to move; " +
            "drag off the bar to delete; double-click a grip to change its colour.";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Domain", "D",
                "The numeric domain the gradient spans. Values outside this range are clamped.",
                GH_ParamAccess.item, new Interval(0, 1));
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new RedbackGradientParam(), "Gradient", "Gr",
                "The gradient object. Connect to an Evaluate Gradient component.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var domainGoo = new GH_Interval();
            DA.GetData(0, ref domainGoo);
            var domain = domainGoo?.Value ?? new Interval(0, 1);

            var ghGradient = GradientField?.GetValue(this) as GHGradient
                ?? new GHGradient(new[] { 0.0, 1.0 }, new[] { Color.Black, Color.White });

            DA.SetData(0, new RedbackGradientGoo(
                new RedbackGradient(new GHGradient(ghGradient), domain.T0, domain.T1)));
        }
    }
}
