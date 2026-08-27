using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Grasshopper.Kernel;
using Redback.Components.Grasshopper;

namespace Redback.Components.SVG
{
    public class SvgToRasterComponent : RedbackGHBase
    {
        private static string _browserPath;
        private static string _resvgPath;
        private static volatile bool _detected;
        private static readonly object _detectionLock = new();

        public SvgToRasterComponent()
            : base("SVG to Image", "SVGtoImg",
                   "Renders an SVG file to a raster image (PNG or JPEG). " +
                   "Uses headless Chrome or Edge if found, resvg if found, " +
                   "otherwise falls back to the Svg.NET library.",
                   "3-SVG") { }

        public override Guid ComponentGuid => new Guid("c2d3e4f5-a6b7-4890-bcde-f09876543210");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Set to True to trigger conversion.", GH_ParamAccess.item);
            pManager.AddTextParameter("SVG Path", "SVG", "File path to the source SVG.", GH_ParamAccess.item);
            pManager.AddTextParameter("Output Path", "P", "Output file path (.png or .jpg/.jpeg).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W",
                "Output width in pixels. 0 = derive from SVG or Height.", GH_ParamAccess.item, 0);
            pManager[3].Optional = true;
            pManager.AddIntegerParameter("Height", "H",
                "Output height in pixels. 0 = derive from SVG or Width.", GH_ParamAccess.item, 0);
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Saved Path", "SP", "Confirmed path of the saved image.", GH_ParamAccess.item);
            pManager.AddTextParameter("Renderer", "R", "Renderer used (chrome, msedge, resvg, or Svg.NET).", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            if (!DA.GetData(0, ref run) || !run) return;

            string svgPath = null;
            if (!DA.GetData(1, ref svgPath) || string.IsNullOrWhiteSpace(svgPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "SVG path not provided.");
                return;
            }
            if (!File.Exists(svgPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"SVG file not found: {svgPath}");
                return;
            }

            string outputPath = null;
            if (!DA.GetData(2, ref outputPath) || string.IsNullOrWhiteSpace(outputPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Output path not provided.");
                return;
            }

            int width = 0, height = 0;
            DA.GetData(3, ref width);
            DA.GetData(4, ref height);

            EnsureDir(outputPath);
            EnsureDetected();

            string rendererName;
            try
            {
                if (_browserPath != null)
                {
                    RenderWithBrowser(_browserPath, svgPath, outputPath, width, height);
                    rendererName = Path.GetFileNameWithoutExtension(_browserPath);
                }
                else if (_resvgPath != null)
                {
                    RenderWithResvg(_resvgPath, svgPath, outputPath, width, height);
                    rendererName = "resvg";
                }
                else
                {
                    RenderWithSvgNet(svgPath, outputPath, width, height);
                    rendererName = "Svg.NET";
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                return;
            }

            DA.SetData(0, outputPath);
            DA.SetData(1, rendererName);
        }

        // ── detection ─────────────────────────────────────────────────────────

        private static void EnsureDetected()
        {
            if (_detected) return;
            lock (_detectionLock)
            {
                if (_detected) return;
                foreach (var path in BrowserCandidates())
                    if (File.Exists(path)) { _browserPath = path; break; }
                _resvgPath = FindOnPath("resvg");
                _detected = true;
            }
        }

        private static string[] BrowserCandidates() => new[]
        {
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Google\Chrome\Application\chrome.exe"),
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        };

        private static string FindOnPath(string exeName)
        {
            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
            {
                var candidate = Path.Combine(dir.Trim(), exeName + ".exe");
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }

        // ── rendering ─────────────────────────────────────────────────────────

        private static void RenderWithBrowser(string browserPath, string svgPath, string outputPath, int width, int height)
        {
            ResolveSize(svgPath, ref width, ref height);

            // Use a temp dir so relative asset paths resolve from the SVG's own directory.
            string svgDir = Path.GetFullPath(Path.GetDirectoryName(svgPath) ?? ".");
            string baseHref = "file:///" + svgDir.Replace('\\', '/').TrimEnd('/') + "/";
            string svgContent = File.ReadAllText(svgPath, System.Text.Encoding.UTF8);

            string tempDir  = Path.Combine(Path.GetTempPath(), "redback_svg_" + Guid.NewGuid().ToString("N"));
            string htmlPath = Path.Combine(tempDir, "render.html");
            string pngPath  = Path.Combine(tempDir, "render.png");
            Directory.CreateDirectory(tempDir);
            try
            {
                // Inline the SVG so it fills the viewport; <base> restores relative paths.
                File.WriteAllText(htmlPath,
                    $"<!DOCTYPE html><html><head><meta charset=\"utf-8\">" +
                    $"<base href=\"{baseHref}\">" +
                    $"<style>*{{margin:0;padding:0}}" +
                    $"html,body{{width:{width}px;height:{height}px;overflow:hidden;background:white}}" +
                    $"svg{{display:block;width:100%;height:100%}}" +
                    $"</style></head><body>{svgContent}</body></html>",
                    System.Text.Encoding.UTF8);

                string fileUri = "file:///" + htmlPath.Replace('\\', '/');
                string args =
                    "--headless --no-sandbox --disable-gpu --no-first-run --hide-scrollbars " +
                    $"--screenshot=\"{pngPath}\" --window-size={width},{height} \"{fileUri}\"";

                RunProcess(browserPath, args, 30_000);

                if (!File.Exists(pngPath) || new FileInfo(pngPath).Length == 0)
                    throw new InvalidOperationException(
                        "Browser produced no output. Check that Chrome/Edge supports --headless --screenshot.");

                // Chrome always writes PNG; convert to JPEG if needed.
                ConvertOrCopy(pngPath, outputPath);
            }
            finally
            {
                TryDeleteDir(tempDir);
            }
        }

        private static void RenderWithResvg(string resvgPath, string svgPath, string outputPath, int width, int height)
        {
            // resvg only outputs PNG; write to temp first when JPEG is requested.
            bool isJpeg = IsJpegPath(outputPath);
            string pngPath = isJpeg
                ? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png")
                : outputPath;
            try
            {
                string args = $"\"{svgPath}\" \"{pngPath}\"";
                if (width  > 0) args += $" --width {width}";
                if (height > 0) args += $" --height {height}";
                RunProcess(resvgPath, args, 30_000);

                if (!File.Exists(pngPath) || new FileInfo(pngPath).Length == 0)
                    throw new InvalidOperationException("resvg produced no output.");

                if (isJpeg) ConvertOrCopy(pngPath, outputPath);
            }
            finally
            {
                if (isJpeg) TryDelete(pngPath);
            }
        }

        private static void RenderWithSvgNet(string svgPath, string outputPath, int width, int height)
        {
            var svgDoc = Svg.SvgDocument.Open(svgPath);
            System.Drawing.Bitmap bmp;
            if (width <= 0 && height <= 0)
            {
                bmp = svgDoc.Draw();
            }
            else
            {
                ResolveSize(svgPath, ref width, ref height);
                bmp = svgDoc.Draw(width, height);
            }
            using (bmp)
                bmp.Save(outputPath, ResolveFormat(outputPath));
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private static void ResolveSize(string svgPath, ref int width, ref int height)
        {
            if (width > 0 && height > 0) return;
            var (w, h) = GetSvgNaturalSize(svgPath);
            if (w <= 0 || h <= 0) { w = 1920; h = 1080; }
            float aspect = w / h;
            if      (width <= 0 && height <= 0) { width = (int)w; height = (int)h; }
            else if (width  <= 0) width  = Math.Max(1, (int)Math.Round(height * aspect));
            else                  height = Math.Max(1, (int)Math.Round(width  / aspect));
        }

        private static (float W, float H) GetSvgNaturalSize(string svgPath)
        {
            try
            {
                var root = XDocument.Load(svgPath).Root;
                float w = ParseSvgLength(root?.Attribute("width")?.Value);
                float h = ParseSvgLength(root?.Attribute("height")?.Value);
                if (w > 0 && h > 0) return (w, h);

                var vb = root?.Attribute("viewBox")?.Value
                    ?.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (vb?.Length >= 4 &&
                    float.TryParse(vb[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float vw) &&
                    float.TryParse(vb[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float vh))
                    return (vw, vh);
            }
            catch { }
            return (0, 0);
        }

        private static float ParseSvgLength(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            int i = value.Length - 1;
            while (i >= 0 && !char.IsDigit(value[i]) && value[i] != '.') i--;
            return float.TryParse(value[..(i + 1)], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float f) ? f : 0;
        }

        private static void ConvertOrCopy(string pngSrc, string dest)
        {
            if (IsJpegPath(dest))
            {
                using var bmp = new System.Drawing.Bitmap(pngSrc);
                bmp.Save(dest, ImageFormat.Jpeg);
            }
            else
            {
                File.Copy(pngSrc, dest, overwrite: true);
            }
        }

        private static void RunProcess(string exe, string args, int timeoutMs)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException($"Failed to start: {exe}");
            if (!proc.WaitForExit(timeoutMs))
            {
                proc.Kill();
                throw new TimeoutException($"Renderer timed out after {timeoutMs / 1000}s.");
            }
        }

        private static bool IsJpegPath(string path) =>
            Path.GetExtension(path).ToLowerInvariant() is ".jpg" or ".jpeg";

        private static ImageFormat ResolveFormat(string path) =>
            IsJpegPath(path) ? ImageFormat.Jpeg : ImageFormat.Png;

        private static void EnsureDir(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        private static void TryDeleteDir(string dir)
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); } catch { }
        }
    }
}
