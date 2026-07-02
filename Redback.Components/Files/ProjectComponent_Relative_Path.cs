using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;

namespace Redback.Components.Files
{
    public class RelativePathComponent : RedbackGHBase
    {
        public RelativePathComponent() : base("Relative Path", "Relative Path",
            "Resolves a relative path against the current Rhino or Grasshopper document", "4-Files") { }

        public override Guid ComponentGuid => new Guid("aef758a6-b93c-442c-9f88-b8daffc854db");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-RelativePath.png");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("Path", "PA",
                "Base path string, or boolean (true = Rhino doc, false = GH doc)", GH_ParamAccess.item);
            p[0].Optional = true;
            p.AddGenericParameter("Relative Path", "RP",
                "Relative path template; supports {rh} and {gh} tokens, and . to step up a directory", GH_ParamAccess.item);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Full Path", "FP", "Resolved full path", GH_ParamAccess.item);
            p.AddTextParameter("Directory", "DI", "Directory portion", GH_ParamAccess.item);
            p.AddTextParameter("File Name", "FN", "File name portion", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_Goo paGoo = null, rpGoo = null;
            DA.GetData(0, ref paGoo);
            DA.GetData(1, ref rpGoo);

            string rp = rpGoo?.ToString();

            try
            {
                string rhName = "", ghName = "";
                try { rhName = Path.GetFileNameWithoutExtension(global::Rhino.RhinoDoc.ActiveDoc?.Path ?? ""); } catch { }
                try { ghName = Path.GetFileNameWithoutExtension(global::Grasshopper.Instances.ActiveCanvas?.Document?.FilePath ?? ""); } catch { }

                if (rp != null)
                    rp = rp.Replace("{rh}", rhName).Replace("{gh}", ghName);

                bool isServer = false;
                string docPath = ResolveDocPath(paGoo);
                if (docPath == null) { EmitOutputs(DA, rp); return; }
                if (docPath.StartsWith("\\\\")) isServer = true;

                string docDir = Directory.Exists(docPath)
                    ? docPath
                    : Path.GetDirectoryName(docPath) ?? docPath;

                if (rp == null)
                    rp = Path.GetFileName(docPath);

                char sep = Path.DirectorySeparatorChar;
                var docDirs = docDir.Split(sep).ToList();
                var relDirs = (rp ?? "").Split(sep).ToList();

                foreach (string step in relDirs)
                    if (step == "." && docDirs.Count > 0)
                        docDirs.RemoveAt(docDirs.Count - 1);

                relDirs.RemoveAll(s => s == ".");

                var combined = docDirs.Concat(relDirs)
                    .Select(x => x.EndsWith(":") ? x + sep.ToString() : x)
                    .ToArray();

                string newPath = combined.Length == 1
                    ? combined[0]
                    : Path.Combine(combined);

                if (isServer) newPath = "\\\\" + newPath;
                EmitOutputs(DA, newPath);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.Message);
                EmitOutputs(DA, rp);
            }
        }

        private static string ResolveDocPath(IGH_Goo goo)
        {
            if (goo is GH_Boolean b)
                return b.Value
                    ? global::Rhino.RhinoDoc.ActiveDoc?.Path
                    : global::Grasshopper.Instances.ActiveCanvas?.Document?.FilePath;
            if (goo != null)
                return goo.ToString();
            return global::Grasshopper.Instances.ActiveCanvas?.Document?.FilePath;
        }

        private void EmitOutputs(IGH_DataAccess DA, string fullPath)
        {
            DA.SetData(0, fullPath);
            if (fullPath == null) { DA.SetData(1, null); DA.SetData(2, null); return; }
            if (Directory.Exists(fullPath))
            {
                DA.SetData(1, fullPath);
                DA.SetData(2, null);
            }
            else
            {
                DA.SetData(1, Path.GetDirectoryName(fullPath));
                DA.SetData(2, Path.GetFileName(fullPath));
            }
        }
    }
}
