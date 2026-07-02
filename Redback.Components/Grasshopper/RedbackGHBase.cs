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
                return new System.Drawing.Bitmap(stream);
            });
        }
    }
}
