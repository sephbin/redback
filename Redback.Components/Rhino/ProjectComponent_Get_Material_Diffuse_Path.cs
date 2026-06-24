using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Rhino.DocObjects;

namespace Redback.Components.Rhino
{
    public class GetMaterialDiffusePathComponent : RedbackGHBase
    {
        public GetMaterialDiffusePathComponent() : base("Get Material Diffuse Path", "Get Material Diffuse Path",
            "Returns the file path of a material's diffuse texture", "6-Rhino") { }

        public override Guid ComponentGuid => new Guid("a76050d7-756a-40ef-8443-51b184fbdeb5");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("M", "M", "Material with diffuse texture", GH_ParamAccess.item);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("P", "P", "Folder path to diffuse image", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_Goo goo = null;
            DA.GetData(0, ref goo);
            if (goo == null) return;

            Material mat = null;
            if (goo is GH_ObjectWrapper wrapper)
                mat = wrapper.Value as Material;
            if (mat == null) goo.CastTo(out mat);
            if (mat == null) return;

            var doc = global::Rhino.RhinoDoc.ActiveDoc;
            if (doc == null) return;

            Material found = null;
            foreach (Material m in doc.Materials)
            {
                if (m.Name == mat.Name) { found = m; break; }
            }
            if (found == null) return;

            var tex = found.GetBitmapTexture();
            if (tex != null) DA.SetData(0, tex.FileName);
        }
    }
}
