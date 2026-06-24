using Grasshopper.Kernel;

namespace Redback.Components.Grasshopper
{
    public abstract class RedbackGHBase : GH_Component
    {
        protected RedbackGHBase(string name, string nick, string desc, string subcategory)
            : base(name, nick, desc, "Redback", subcategory) { }
    }
}
