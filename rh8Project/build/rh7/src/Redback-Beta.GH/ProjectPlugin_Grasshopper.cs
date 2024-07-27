using System;
using System.IO;
using System.Text;
using System.Drawing;

using Grasshopper.Kernel;

namespace RhinoCodePlatform.Rhino3D.Projects.Plugin.GH
{
  public sealed class AssemblyInfo : GH_AssemblyInfo
  {
    internal static readonly string _assemblyIconData = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAAW5JREFUSEu9VDtOw0AUdG0h4YICCbFvZYsW0dBR0KbjAiicAJEThIomkaCgDzeAEiq4Adwg1IDEEZY3zjP2Ji/2rpEYaaTV23kz3o83CcFsc5h9pZPhRzodgxhfc02m/4bc2PFjNvr+TKeuSdQwJ7J4EFFWGDsryLqnbOSZg6hhjkOeoZW2cHDjPQy6AiTkVdrCkBNdVM0hAQvSjbS3g5dr/cbQAJCOxWY9CkN3y42hATgPsdGhfT0YvgKwZRXLe18xKoB3QOxWwddyrjXFrcA69dqW914Rg7EB6jZx8UQX9wq4FNsaKOpi6263TlcCUNO0YG7oRWxrtAUMdg7dfOPq1xxj1DQtGB0AHu3uu/PtQUmMNU3FXgEx/I+AB7GtsUd0oIn7ED+s2Prgv/Bda4glnhyx9MHJZ1pDHDuebeyf3hhAQ2/qM9EEBH1CcHM6zZsot4u/SDPzyOcGrbTFA7drcTa4xjVRw5zI1iBJfgBeiEn6eGfuwQAAAABJRU5ErkJggg==";
    internal static readonly string _categoryIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAATtJREFUOE+FkiFPA0EQhU8SQkjTIEhoZ/Zur+ZSgeIX9AdUIAgK0bSmBodAIJAEXUlCUoMlITgUf6CihgSBrEAgK8qb7UC7y7J9yUv2dt/3brLZLCZm3nrdvRjOtm8exLKWPT1OqzwoG5bN5Ll2vgDsLGtLZmobttRYXPhLDcF3FDjIK8BeweYDmX2N/5UlvpPgfwWuhMyTxn21mA9/QqkCcU55R7GV8PfReihVYJkfFVupYJ6th5IFZOZyX4oCLoq2F4DTE4i5qzjGx0cYGNcHvwX39b53JsZlXiruxj8LA0fNanG7d+os6/AcBdeKu4KTMLDZfKV4/A42uSQ6VnwpTPEWC0ZN5rOqqh1Fl2oRdfAW5lEgsNyZYr5kLGmPQc7EX5bynsbjktEsUQ+XhJfJL+qR7HmPxynLvgHeQAfl+kqnzAAAAABJRU5ErkJggg==";
    internal static readonly Bitmap _assemblyIcon;
    internal static readonly Bitmap _categoryIcon;

    static AssemblyInfo()
    {
      using (var aicon = new MemoryStream(Convert.FromBase64String(_assemblyIconData)))
        _assemblyIcon = new Bitmap(aicon);

      using (var cicon = new MemoryStream(Convert.FromBase64String(_categoryIconData)))
        _categoryIcon = new Bitmap(cicon);
    }

    public override Guid Id { get; } = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");

    public override string AssemblyName { get; } = "Redback-Beta.GH";
    public override string AssemblyVersion { get; } = "0.3.8.8959";
    public override string AssemblyDescription { get; } = "Contains components for managing Data, SVG and ICML.";
    public override string AuthorName { get; } = "Andrew Butler";
    public override string AuthorContact { get; } = "andrew.butler@strangercollective.com";
    public override GH_LibraryLicense AssemblyLicense { get; } = GH_LibraryLicense.unset;
    public override Bitmap AssemblyIcon { get; } = _assemblyIcon;
  }

  public class ProjectComponentPlugin : GH_AssemblyPriority
  {
    public override GH_LoadingInstruction PriorityLoad()
    {
      Grasshopper.Instances.ComponentServer.AddCategoryIcon("Redback-Beta", AssemblyInfo._categoryIcon);
      Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Redback-Beta", "Redback-Beta"[0]);
      return GH_LoadingInstruction.Proceed;
    }

    public static string DecryptString(string text)
    {
      if (text is null)
        throw new System.ArgumentNullException(nameof(text));

      if (string.IsNullOrWhiteSpace(text))
        return string.Empty;

      return Encoding.UTF8.GetString(Convert.FromBase64String(text));
    }
  }
}
