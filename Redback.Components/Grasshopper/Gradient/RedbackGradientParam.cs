using System;
using Grasshopper.Kernel;

namespace Redback.Components.Grasshopper.Gradient
{
    public class RedbackGradientParam : GH_Param<RedbackGradientGoo>
    {
        public RedbackGradientParam()
            : base("Gradient", "Gr",
                   "A Redback gradient object.",
                   "Redback", "Gradient",
                   GH_ParamAccess.item) { }

        public override Guid ComponentGuid => new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567891");
    }
}
