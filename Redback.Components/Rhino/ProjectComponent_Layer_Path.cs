using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;

namespace Redback.Components.Rhino
{
    public class LayerPathComponent : RedbackGHBase
    {
        public LayerPathComponent() : base("Layer Path", "Layer Path",
            "Returns the full path of a Rhino layer", "6-Rhino") { }

        public override Guid ComponentGuid => new Guid("4337018e-7eaf-4fc5-89ca-9bfadc1e14c4");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-LayerPath.svg");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("Layer", "L", "Rhino layer object", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Path", "P", "Full layer path", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_Goo goo = null;
            if (!DA.GetData(0, ref goo) || goo == null) { DA.SetData(0, null); return; }

            if (goo is GH_ObjectWrapper wrapper && wrapper.Value is global::Rhino.DocObjects.Layer layer)
            {
                DA.SetData(0, layer.FullPath);
                return;
            }

            DA.SetData(0, goo.ToString());
        }
    }
}
