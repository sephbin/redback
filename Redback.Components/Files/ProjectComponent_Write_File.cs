using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.Files
{
    public class WriteFileComponent : RedbackGHBase
    {
        public WriteFileComponent() : base("Write File", "Write File",
            "Writes content to a file", "4-Files") { }

        public override Guid ComponentGuid => new Guid("982b8e72-1322-42de-bca6-e65ee014c03c");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-WriteFile.svg");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddBooleanParameter("Run", "R", "Set true to write the file", GH_ParamAccess.item, false);
            p[0].Optional = true;
            p.AddTextParameter("Content", "CO", "Lines of content to write", GH_ParamAccess.list);
            p[1].Optional = true;
            p.AddTextParameter("File Directory", "FD", "Directory path (%user% expands to home folder)", GH_ParamAccess.item);
            p[2].Optional = true;
            p.AddTextParameter("File Name", "FN", "File name including extension", GH_ParamAccess.item);
            p[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Full Path", "FP", "Path of the written file", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            var content = new List<string>();
            string dir = null, filename = null;
            DA.GetData(0, ref run);
            DA.GetDataList(1, content);
            DA.GetData(2, ref dir);
            DA.GetData(3, ref filename);

            if (!run) { DA.SetData(0, null); return; }
            if (dir == null || filename == null) { DA.SetData(0, null); return; }

            dir = dir.Replace("%user%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            dir = dir.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            string fullPath = Path.Combine(dir, filename);
            string fileDir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(fileDir) && !Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            File.WriteAllBytes(fullPath, Encoding.UTF8.GetBytes(string.Join("\n", content)));

            DA.SetData(0, fullPath);
        }
    }
}
