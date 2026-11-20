using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;
using Redback.Render;
using Rhino.Geometry;

namespace Redback.Components.Render
{
    /// <summary>
    /// Renders a 360° flat-ID view (cube map) from each camera point and reports which
    /// meshes are visible and how much of the view each one covers.
    /// </summary>
    public class IdBufferRendererComponent : RedbackGHBase
    {
        public IdBufferRendererComponent()
            : base("ID Buffer Renderer", "IDBuffer",
                   "CPU 360° visibility render. Every mesh gets a unique ID, six 90° cube faces are " +
                   "rasterised from each camera point, and the share of the view covered by each ID " +
                   "is reported. Use it to find the elements bounding a room, or for view analysis.",
                   "Render") { }

        public override Guid ComponentGuid => new Guid("3d7c52a8-91e4-4b0f-a6d3-58f1c2e97b40");
        protected override System.Drawing.Bitmap Icon => LoadIcon("Icon-IdBuffer.png");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // 0
            pManager.AddMeshParameter("Meshes", "M",
                "Geometry to render. One ID is assigned per mesh.", GH_ParamAccess.list);

            // 1
            pManager.AddTextParameter("IDs", "ID",
                "ID for each mesh (e.g. Revit ElementId), matched to Meshes by index. " +
                "Defaults to the mesh index.", GH_ParamAccess.list);
            pManager[1].Optional = true;

            // 2
            pManager.AddTextParameter("SubIDs", "SID",
                "Optional sub-ID for each mesh (e.g. material or layer), so one element can be " +
                "split into several meshes and reported separately.", GH_ParamAccess.list);
            pManager[2].Optional = true;

            // 3
            pManager.AddPointParameter("Cameras", "C",
                "Camera points. Each is rendered independently.", GH_ParamAccess.list);

            // 4
            pManager.AddIntegerParameter("Resolution", "R",
                "Pixels per cube-face edge (six faces are rendered). Raise it to catch small elements.",
                GH_ParamAccess.item, 512);

            // 5
            pManager.AddNumberParameter("MaxDistance", "D",
                "Hits further than this count as background. 0 = unlimited. " +
                "Useful to ignore what is seen through doors and windows.",
                GH_ParamAccess.item, 0.0);
            pManager[5].Optional = true;

            // 6
            pManager.AddTextParameter("Folder", "F",
                "Optional folder. When set, per-camera ID, depth and normal panoramas are saved as PNGs.",
                GH_ParamAccess.item);
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("IDs", "ID",
                "Visible IDs per camera, largest share first.", GH_ParamAccess.tree);
            pManager.AddTextParameter("SubIDs", "SID",
                "Sub-ID matching each ID.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Percent", "%",
                "Percentage of the full 360° sphere covered by each ID.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("HitPercent", "H%",
                "Percentage of the geometry-covered view only, ignoring background.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Background", "BG",
                "Percentage of the sphere with no hit, per camera.", GH_ParamAccess.list);
            pManager.AddTextParameter("Images", "IMG",
                "Saved PNG paths per camera (if Folder is set).", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                // ── Meshes ────────────────────────────────────────────────────
                var meshGoos = new List<GH_Mesh>();
                if (!DA.GetDataList(0, meshGoos) || meshGoos.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No meshes provided.");
                    return;
                }
                var meshes = meshGoos.Select(g => g?.Value).ToList();

                // ── IDs / SubIDs ──────────────────────────────────────────────
                var ids = new List<string>();
                if (DA.GetDataList(1, ids) && ids.Count != meshes.Count)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        $"{ids.Count} IDs for {meshes.Count} meshes; missing IDs default to the mesh index.");
                var subIds = new List<string>();
                DA.GetDataList(2, subIds);

                string IdOf(int i)  => i < ids.Count    && !string.IsNullOrEmpty(ids[i]) ? ids[i] : i.ToString();
                string SubOf(int i) => i < subIds.Count && subIds[i] != null ? subIds[i] : "";

                // ── Cameras ───────────────────────────────────────────────────
                var cams = new List<Point3d>();
                if (!DA.GetDataList(3, cams) || cams.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No camera points provided.");
                    return;
                }

                // ── Settings ──────────────────────────────────────────────────
                int res = 512;
                DA.GetData(4, ref res);
                res = Math.Max(16, Math.Min(4096, res));

                double maxDist = 0.0;
                DA.GetData(5, ref maxDist);

                string folder = null;
                DA.GetData(6, ref folder);
                bool saveImages = !string.IsNullOrWhiteSpace(folder);
                if (saveImages) Directory.CreateDirectory(folder);

                // ── Render ────────────────────────────────────────────────────
                var tris = IdBufferCore.CollectTriangles(meshes);
                if (tris.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "Meshes produced no renderable triangles.");
                    return;
                }

                var idTree   = new DataTree<string>();
                var subTree  = new DataTree<string>();
                var pctTree  = new DataTree<double>();
                var hitTree  = new DataTree<double>();
                var imgTree  = new DataTree<string>();
                var bgList   = new List<double>();

                for (int c = 0; c < cams.Count; c++)
                {
                    var path = new GH_Path(c);
                    var r = IdBufferCore.Render(tris, meshes.Count, cams[c], res, maxDist);

                    // Merge meshes that share an (ID, SubID) pair.
                    var groups = new Dictionary<(string, string), (double sphere, double hit)>();
                    for (int m = 0; m < meshes.Count; m++)
                    {
                        if (r.SphereShare[m] <= 0) continue;
                        var key = (IdOf(m), SubOf(m));
                        groups.TryGetValue(key, out var acc);
                        groups[key] = (acc.sphere + r.SphereShare[m], acc.hit + r.HitShare[m]);
                    }

                    foreach (var kv in groups.OrderByDescending(k => k.Value.sphere))
                    {
                        idTree.Add(kv.Key.Item1, path);
                        subTree.Add(kv.Key.Item2, path);
                        pctTree.Add(kv.Value.sphere * 100.0, path);
                        hitTree.Add(kv.Value.hit * 100.0, path);
                    }
                    bgList.Add(r.BackgroundShare * 100.0);

                    if (saveImages)
                    {
                        foreach (IdPass pass in Enum.GetValues(typeof(IdPass)))
                        {
                            var px = IdBufferCore.PassImage(r, tris, pass, out int w, out int h);
                            string file = Path.Combine(folder, $"cam{c}_{pass.ToString().ToLowerInvariant()}.png");
                            ShadowMapCore.WriteBitmap(px, w, h, file);
                            imgTree.Add(file, path);
                        }
                    }
                }

                DA.SetDataTree(0, idTree);
                DA.SetDataTree(1, subTree);
                DA.SetDataTree(2, pctTree);
                DA.SetDataTree(3, hitTree);
                DA.SetDataList(4, bgList);
                DA.SetDataTree(5, imgTree);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }
    }
}
