using System;
using SD = System.Drawing;

using Rhino;
using Grasshopper.Kernel;

namespace RhinoCodePlatform.Rhino3D.Projects.Plugin.GH
{
  public sealed class AssemblyInfo : GH_AssemblyInfo
  {
    public override Guid Id { get; } = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");

    public override string AssemblyName { get; } = "Redback-Beta.GH";
    public override string AssemblyVersion { get; } = "0.3.17.9028";
    public override string AssemblyDescription { get; } = "Contains components for managing Data, SVG and ICML.";
    public override string AuthorName { get; } = "Andrew Butler";
    public override string AuthorContact { get; } = "andrew.butler@strangercollective.com";
    public override GH_LibraryLicense AssemblyLicense { get; } = GH_LibraryLicense.unset;
    public override SD.Bitmap AssemblyIcon { get; } = ProjectComponentPlugin.PluginIcon;
  }

  public class ProjectComponentPlugin : GH_AssemblyPriority
  {
    public static SD.Bitmap PluginIcon { get; }
    public static SD.Bitmap PluginCategoryIcon { get; }

    static readonly Guid s_rhinocode = new Guid("c9cba87a-23ce-4f15-a918-97645c05cde7");
    static readonly Type s_invokeContextType = default;
    static readonly dynamic s_projectServer = default;
    static readonly object s_project = default;

    static readonly Guid s_projectId = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");
    static readonly string s_projectData = "ew0KICAiaWQiOiAiN2QxNDJkZTYtN2EzNy00YjczLTk2MWItYTVlZjNmN2JmYmM4IiwNCiAgImlkZW50aXR5Ijogew0KICAgICJuYW1lIjogIlJlZGJhY2stQmV0YSIsDQogICAgInZlcnNpb24iOiAiMC4zLjE3IiwNCiAgICAicHVibGlzaGVyIjogew0KICAgICAgImVtYWlsIjogImFuZHJldy5idXRsZXJAc3RyYW5nZXJjb2xsZWN0aXZlLmNvbSIsDQogICAgICAibmFtZSI6ICJBbmRyZXcgQnV0bGVyIiwNCiAgICAgICJjb21wYW55IjogIlN0cmFuZ2VyIENvbGxlY3RpdmUiLA0KICAgICAgImNvdW50cnkiOiAiQXVzdHJhbGlhIiwNCiAgICAgICJ1cmwiOiAiaHR0cHM6Ly9naXRodWIuY29tL3NlcGhiaW4vIg0KICAgIH0sDQogICAgImRlc2NyaXB0aW9uIjogIkNvbnRhaW5zIGNvbXBvbmVudHMgZm9yIG1hbmFnaW5nIERhdGEsIFNWRyBhbmQgSUNNTC4iLA0KICAgICJjb3B5cmlnaHQiOiAiQ29weXJpZ2h0IFx1MDBBOSAyMDI0ICIsDQogICAgImltYWdlIjogew0KICAgICAgImxpZ2h0Ijogew0KICAgICAgICAidHlwZSI6ICJzdmciLA0KICAgICAgICAiZGF0YSI6ICJQSE4yWnlCcFpEMGlUR0Y1WlhKZk1TSWdaR0YwWVMxdVlXMWxQU0pNWVhsbGNpQXhJaUI0Yld4dWN6MGlhSFIwY0RvdkwzZDNkeTUzTXk1dmNtY3ZNakF3TUM5emRtY2lJSGh0Ykc1ek9uaHNhVzVyUFNKb2RIUndPaTh2ZDNkM0xuY3pMbTl5Wnk4eE9UazVMM2hzYVc1cklpQjJhV1YzUW05NFBTSXdJREFnT1RZZ09UWWlQZ29nSUR4a1pXWnpQZ29nSUNBZ1BITjBlV3hsUGdvZ0lDQWdJQ0F1WTJ4ekxURXNJQzVqYkhNdE1pQjdDaUFnSUNBZ0lDQWdabWxzYkRvZ2JtOXVaVHNLSUNBZ0lDQWdmUW9LSUNBZ0lDQWdMbU5zY3kweUlIc0tJQ0FnSUNBZ0lDQmpiR2x3TFhCaGRHZzZJSFZ5YkNnalkyeHBjSEJoZEdncE93b2dJQ0FnSUNCOUNnb2dJQ0FnSUNBdVkyeHpMVE1nZXdvZ0lDQWdJQ0FnSUdacGJHdzZJQ05sWWpCaE9HTTdDaUFnSUNBZ0lIMEtDaUFnSUNBZ0lDNWpiSE10TkNCN0NpQWdJQ0FnSUNBZ1ptbHNiRG9nSXpJek1XWXlNRHNLSUNBZ0lDQWdmUW9nSUNBZ1BDOXpkSGxzWlQ0S0lDQWdJRHhqYkdsd1VHRjBhQ0JwWkQwaVkyeHBjSEJoZEdnaVBnb2dJQ0FnSUNBOGNHRjBhQ0JqYkdGemN6MGlZMnh6TFRFaUlHUTlJbTA0Tnk0ME1TdzFPR013TERJMUxqWTVMVEUzTGpZMUxETTJMalV5TFRNNUxqUXhMRE0yTGpVeVV6a3VOVGtzT0RNdU5qa3NPUzQxT1N3MU9Dd3lOaTR5TXl3eExqUTRMRFE0TERFdU5EaHpNemt1TkRFc016QXVPRE1zTXprdU5ERXNOVFl1TlRKYUlpOFx1MDAyQkNpQWdJQ0E4TDJOc2FYQlFZWFJvUGdvZ0lEd3ZaR1ZtY3o0S0lDQThaeUJqYkdGemN6MGlZMnh6TFRJaVBnb2dJQ0FnUEhKbFkzUWdZMnhoYzNNOUltTnNjeTAwSWlCM2FXUjBhRDBpT1RZaUlHaGxhV2RvZEQwaU9UWWlMejRLSUNBZ0lEeHlaV04wSUdOc1lYTnpQU0pqYkhNdE15SWdlRDBpTXpjdU5EUWlJSGs5SWkwMUxqYzRJaUIzYVdSMGFEMGlNakV1TVRFaUlHaGxhV2RvZEQwaU5qTXVNek1pSUhKNFBTSTVJaUJ5ZVQwaU9TSXZQZ29nSUR3dlp6NEtQQzl6ZG1jXHUwMDJCIg0KICAgICAgfQ0KICAgIH0NCiAgfSwNCiAgInNldHRpbmdzIjogew0KICAgICJidWlsZFBhdGgiOiAiZmlsZTovLy9LOi9Db21wdXRhdGlvbmFsIERlc2lnbiBHcm91cC85M19EZXZlbG9wbWVudC9QYWNrYWdlTWFuYWdlci9yZWRiYWNrL3JoOFByb2plY3QvYnVpbGQvcmg4IiwNCiAgICAiYnVpbGRUYXJnZXQiOiB7DQogICAgICAiYXBwTmFtZSI6ICJSaGlubzNEIiwNCiAgICAgICJhcHBWZXJzaW9uIjogew0KICAgICAgICAibWFqb3IiOiA4DQogICAgICB9LA0KICAgICAgInRpdGxlIjogIlJoaW5vM0QgKDguKikiLA0KICAgICAgInNsdWciOiAicmg4Ig0KICAgIH0sDQogICAgInB1Ymxpc2hUYXJnZXQiOiB7DQogICAgICAidGl0bGUiOiAiTWNOZWVsIFlhayBTZXJ2ZXIiDQogICAgfQ0KICB9LA0KICAiY29kZXMiOiBbXQ0KfQ==";
    static readonly string _iconData = "ew0KICAibGlnaHQiOiB7DQogICAgInR5cGUiOiAic3ZnIiwNCiAgICAiZGF0YSI6ICJQSE4yWnlCcFpEMGlUR0Y1WlhKZk1TSWdaR0YwWVMxdVlXMWxQU0pNWVhsbGNpQXhJaUI0Yld4dWN6MGlhSFIwY0RvdkwzZDNkeTUzTXk1dmNtY3ZNakF3TUM5emRtY2lJSGh0Ykc1ek9uaHNhVzVyUFNKb2RIUndPaTh2ZDNkM0xuY3pMbTl5Wnk4eE9UazVMM2hzYVc1cklpQjJhV1YzUW05NFBTSXdJREFnT1RZZ09UWWlQZ29nSUR4a1pXWnpQZ29nSUNBZ1BITjBlV3hsUGdvZ0lDQWdJQ0F1WTJ4ekxURXNJQzVqYkhNdE1pQjdDaUFnSUNBZ0lDQWdabWxzYkRvZ2JtOXVaVHNLSUNBZ0lDQWdmUW9LSUNBZ0lDQWdMbU5zY3kweUlIc0tJQ0FnSUNBZ0lDQmpiR2x3TFhCaGRHZzZJSFZ5YkNnalkyeHBjSEJoZEdncE93b2dJQ0FnSUNCOUNnb2dJQ0FnSUNBdVkyeHpMVE1nZXdvZ0lDQWdJQ0FnSUdacGJHdzZJQ05sWWpCaE9HTTdDaUFnSUNBZ0lIMEtDaUFnSUNBZ0lDNWpiSE10TkNCN0NpQWdJQ0FnSUNBZ1ptbHNiRG9nSXpJek1XWXlNRHNLSUNBZ0lDQWdmUW9nSUNBZ1BDOXpkSGxzWlQ0S0lDQWdJRHhqYkdsd1VHRjBhQ0JwWkQwaVkyeHBjSEJoZEdnaVBnb2dJQ0FnSUNBOGNHRjBhQ0JqYkdGemN6MGlZMnh6TFRFaUlHUTlJbTA0Tnk0ME1TdzFPR013TERJMUxqWTVMVEUzTGpZMUxETTJMalV5TFRNNUxqUXhMRE0yTGpVeVV6a3VOVGtzT0RNdU5qa3NPUzQxT1N3MU9Dd3lOaTR5TXl3eExqUTRMRFE0TERFdU5EaHpNemt1TkRFc016QXVPRE1zTXprdU5ERXNOVFl1TlRKYUlpOFx1MDAyQkNpQWdJQ0E4TDJOc2FYQlFZWFJvUGdvZ0lEd3ZaR1ZtY3o0S0lDQThaeUJqYkdGemN6MGlZMnh6TFRJaVBnb2dJQ0FnUEhKbFkzUWdZMnhoYzNNOUltTnNjeTAwSWlCM2FXUjBhRDBpT1RZaUlHaGxhV2RvZEQwaU9UWWlMejRLSUNBZ0lEeHlaV04wSUdOc1lYTnpQU0pqYkhNdE15SWdlRDBpTXpjdU5EUWlJSGs5SWkwMUxqYzRJaUIzYVdSMGFEMGlNakV1TVRFaUlHaGxhV2RvZEQwaU5qTXVNek1pSUhKNFBTSTVJaUJ5ZVQwaU9TSXZQZ29nSUR3dlp6NEtQQzl6ZG1jXHUwMDJCIg0KICB9DQp9";

    static ProjectComponentPlugin()
    {
      s_projectServer = ProjectInterop.GetProjectServer();
      if (s_projectServer is null)
      {
        RhinoApp.WriteLine($"Error loading Grasshopper plugin. Missing Rhino3D platform");
        return;
      }

      // get project
      dynamic dctx = ProjectInterop.CreateInvokeContext();
      dctx.Inputs["projectAssembly"] = typeof(ProjectComponentPlugin).Assembly;
      dctx.Inputs["projectId"] = s_projectId;
      dctx.Inputs["projectData"] = s_projectData;

      object project = default;
      if (s_projectServer.TryInvoke("plugins/v1/deserialize", dctx)
            && dctx.Outputs.TryGet("project", out project))
      {
        // server reports errors
        s_project = project;
      }

      // get icons
      if (!_iconData.Contains("ASSEMBLY-ICON"))
      {
        dynamic ictx = ProjectInterop.CreateInvokeContext();
        ictx.Inputs["iconData"] = _iconData;
        SD.Bitmap icon = default;
        if (s_projectServer.TryInvoke("plugins/v1/icon/gh/assembly", ictx)
              && ictx.Outputs.TryGet("icon", out icon))
        {
          // server reports errors
          PluginIcon = icon;
        }

        if (s_projectServer.TryInvoke("plugins/v1/icon/gh/category", ictx)
              && ictx.Outputs.TryGet("icon", out icon))
        {
          // server reports errors
          PluginCategoryIcon = icon;
        }
      }
    }

    public override GH_LoadingInstruction PriorityLoad()
    {
      Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Redback-Beta", "Redback-Beta"[0]);

      if (PluginCategoryIcon != null)
        Grasshopper.Instances.ComponentServer.AddCategoryIcon("Redback-Beta", PluginCategoryIcon);

      return GH_LoadingInstruction.Proceed;
    }

    public static bool TryCreateScript(GH_Component ghcomponent, string serialized, out object script)
    {
      script = default;

      if (s_projectServer is null) return false;

      dynamic dctx = ProjectInterop.CreateInvokeContext();
      dctx.Inputs["component"] = ghcomponent;
      dctx.Inputs["project"] = s_project;
      dctx.Inputs["scriptData"] = serialized;

      if (s_projectServer.TryInvoke("plugins/v1/gh/deserialize", dctx))
      {
        return dctx.Outputs.TryGet("script", out script);
      }

      return false;
    }

    public static bool TryCreateScriptIcon(object script, out SD.Bitmap icon)
    {
      icon = default;

      if (s_projectServer is null) return false;

      dynamic ictx = ProjectInterop.CreateInvokeContext();
      ictx.Inputs["script"] = script;

      if (s_projectServer.TryInvoke("plugins/v1/icon/gh/script", ictx))
      {
        // server reports errors
        return ictx.Outputs.TryGet("icon", out icon);
      }

      return false;
    }

    public static void DisposeScript(GH_Component ghcomponent, object script)
    {
      if (script is null)
        return;

      dynamic dctx = ProjectInterop.CreateInvokeContext();
      dctx.Inputs["component"] = ghcomponent;
      dctx.Inputs["project"] = s_project;
      dctx.Inputs["script"] = script;

      if (!s_projectServer.TryInvoke("plugins/v1/gh/dispose", dctx))
        throw new Exception("Error disposing Grasshopper script component");
    }
  }
}
