using System.Collections.Concurrent;
using System.Reflection;
using Grasshopper.Kernel;

namespace Redback.Components.Grasshopper
{
    public abstract class RedbackGHBase : GH_Component
    {
        protected RedbackGHBase(string name, string nick, string desc, string subcategory)
            : base(name, nick, desc, "Redback", subcategory) { }

        private static readonly ConcurrentDictionary<string, System.Drawing.Bitmap> _iconCache = new();

        protected static System.Drawing.Bitmap LoadIcon(string resourceName)
        {
            return _iconCache.GetOrAdd(resourceName, static name =>
            {
                var asm = Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream($"Redback.Components.Resources.{name}");
                if (stream == null) return null;
                try
                {
                    var svgDoc = Svg.SvgDocument.Open<Svg.SvgDocument>(stream);
                    return svgDoc.Draw(24, 24);
                }
                catch { return null; }
            });
        }
    }
}
