using System;
using SD = System.Drawing;
using System.Reflection;

using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Platform;

namespace RhinoCodePlatform.Rhino3D.Projects.Plugin.GH
{
  public sealed class AssemblyInfo : GH_AssemblyInfo
  {
    public override Guid Id { get; } = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");

    public override string AssemblyName { get; } = "Redback-Beta.GH";
    public override string AssemblyVersion { get; } = "0.3.9.8959";
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
    static readonly PlatformSpec s_rhino = new PlatformSpec("mcneel.rhino3d.rhino");
    static readonly IProjectServer s_projectServer = default;
    static readonly IProject s_project = default;

    static readonly Guid s_projectId = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");
    static readonly string s_projectData = "ew0KICAiaWQiOiAiN2QxNDJkZTYtN2EzNy00YjczLTk2MWItYTVlZjNmN2JmYmM4IiwNCiAgImlkZW50aXR5Ijogew0KICAgICJuYW1lIjogIlJlZGJhY2stQmV0YSIsDQogICAgInZlcnNpb24iOiAiMC4zLjkiLA0KICAgICJwdWJsaXNoZXIiOiB7DQogICAgICAiZW1haWwiOiAiYW5kcmV3LmJ1dGxlckBzdHJhbmdlcmNvbGxlY3RpdmUuY29tIiwNCiAgICAgICJuYW1lIjogIkFuZHJldyBCdXRsZXIiLA0KICAgICAgImNvbXBhbnkiOiAiU3RyYW5nZXIgQ29sbGVjdGl2ZSIsDQogICAgICAiY291bnRyeSI6ICJBdXN0cmFsaWEiLA0KICAgICAgInVybCI6ICJodHRwczovL2dpdGh1Yi5jb20vc2VwaGJpbi8iDQogICAgfSwNCiAgICAiZGVzY3JpcHRpb24iOiAiQ29udGFpbnMgY29tcG9uZW50cyBmb3IgbWFuYWdpbmcgRGF0YSwgU1ZHIGFuZCBJQ01MLiIsDQogICAgImNvcHlyaWdodCI6ICJDb3B5cmlnaHQgXHUwMEE5IDIwMjQgIiwNCiAgICAiaW1hZ2UiOiB7DQogICAgICAibGlnaHQiOiB7DQogICAgICAgICJ0eXBlIjogInN2ZyIsDQogICAgICAgICJkYXRhIjogIlBITjJaeUJwWkQwaVRHRjVaWEpmTVNJZ1pHRjBZUzF1WVcxbFBTSk1ZWGxsY2lBeElpQjRiV3h1Y3owaWFIUjBjRG92TDNkM2R5NTNNeTV2Y21jdk1qQXdNQzl6ZG1jaUlIaHRiRzV6T25oc2FXNXJQU0pvZEhSd09pOHZkM2QzTG5jekxtOXlaeTh4T1RrNUwzaHNhVzVySWlCMmFXVjNRbTk0UFNJd0lEQWdPVFlnT1RZaVBnb2dJRHhrWldaelBnb2dJQ0FnUEhOMGVXeGxQZ29nSUNBZ0lDQXVZMnh6TFRFc0lDNWpiSE10TWlCN0NpQWdJQ0FnSUNBZ1ptbHNiRG9nYm05dVpUc0tJQ0FnSUNBZ2ZRb0tJQ0FnSUNBZ0xtTnNjeTB5SUhzS0lDQWdJQ0FnSUNCamJHbHdMWEJoZEdnNklIVnliQ2dqWTJ4cGNIQmhkR2dwT3dvZ0lDQWdJQ0I5Q2dvZ0lDQWdJQ0F1WTJ4ekxUTWdld29nSUNBZ0lDQWdJR1pwYkd3NklDTmxZakJoT0dNN0NpQWdJQ0FnSUgwS0NpQWdJQ0FnSUM1amJITXROQ0I3Q2lBZ0lDQWdJQ0FnWm1sc2JEb2dJekl6TVdZeU1Ec0tJQ0FnSUNBZ2ZRb2dJQ0FnUEM5emRIbHNaVDRLSUNBZ0lEeGpiR2x3VUdGMGFDQnBaRDBpWTJ4cGNIQmhkR2dpUGdvZ0lDQWdJQ0E4Y0dGMGFDQmpiR0Z6Y3owaVkyeHpMVEVpSUdROUltMDROeTQwTVN3MU9HTXdMREkxTGpZNUxURTNMalkxTERNMkxqVXlMVE01TGpReExETTJMalV5VXprdU5Ua3NPRE11Tmprc09TNDFPU3cxT0N3eU5pNHlNeXd4TGpRNExEUTRMREV1TkRoek16a3VOREVzTXpBdU9ETXNNemt1TkRFc05UWXVOVEphSWk4XHUwMDJCQ2lBZ0lDQThMMk5zYVhCUVlYUm9QZ29nSUR3dlpHVm1jejRLSUNBOFp5QmpiR0Z6Y3owaVkyeHpMVElpUGdvZ0lDQWdQSEpsWTNRZ1kyeGhjM005SW1Oc2N5MDBJaUIzYVdSMGFEMGlPVFlpSUdobGFXZG9kRDBpT1RZaUx6NEtJQ0FnSUR4eVpXTjBJR05zWVhOelBTSmpiSE10TXlJZ2VEMGlNemN1TkRRaUlIazlJaTAxTGpjNElpQjNhV1IwYUQwaU1qRXVNVEVpSUdobGFXZG9kRDBpTmpNdU16TWlJSEo0UFNJNUlpQnllVDBpT1NJdlBnb2dJRHd2Wno0S1BDOXpkbWNcdTAwMkIiDQogICAgICB9DQogICAgfQ0KICB9LA0KICAic2V0dGluZ3MiOiB7DQogICAgImJ1aWxkUGF0aCI6ICJmaWxlOi8vL0s6L0NvbXB1dGF0aW9uYWwgRGVzaWduIEdyb3VwLzkzX0RldmVsb3BtZW50L1BhY2thZ2VNYW5hZ2VyL3JlZGJhY2svcmg4UHJvamVjdC9idWlsZC9yaDgiLA0KICAgICJidWlsZFRhcmdldCI6IHsNCiAgICAgICJhcHBOYW1lIjogIlJoaW5vM0QiLA0KICAgICAgImFwcFZlcnNpb24iOiB7DQogICAgICAgICJtYWpvciI6IDgNCiAgICAgIH0sDQogICAgICAidGl0bGUiOiAiUmhpbm8zRCAoOC4qKSIsDQogICAgICAic2x1ZyI6ICJyaDgiDQogICAgfSwNCiAgICAicHVibGlzaFRhcmdldCI6IHsNCiAgICAgICJ0aXRsZSI6ICJNY05lZWwgWWFrIFNlcnZlciINCiAgICB9DQogIH0sDQogICJjb2RlcyI6IFtdDQp9";
    static readonly string _iconData = "ew0KICAibGlnaHQiOiB7DQogICAgInR5cGUiOiAic3ZnIiwNCiAgICAiZGF0YSI6ICJQSE4yWnlCcFpEMGlUR0Y1WlhKZk1TSWdaR0YwWVMxdVlXMWxQU0pNWVhsbGNpQXhJaUI0Yld4dWN6MGlhSFIwY0RvdkwzZDNkeTUzTXk1dmNtY3ZNakF3TUM5emRtY2lJSGh0Ykc1ek9uaHNhVzVyUFNKb2RIUndPaTh2ZDNkM0xuY3pMbTl5Wnk4eE9UazVMM2hzYVc1cklpQjJhV1YzUW05NFBTSXdJREFnT1RZZ09UWWlQZ29nSUR4a1pXWnpQZ29nSUNBZ1BITjBlV3hsUGdvZ0lDQWdJQ0F1WTJ4ekxURXNJQzVqYkhNdE1pQjdDaUFnSUNBZ0lDQWdabWxzYkRvZ2JtOXVaVHNLSUNBZ0lDQWdmUW9LSUNBZ0lDQWdMbU5zY3kweUlIc0tJQ0FnSUNBZ0lDQmpiR2x3TFhCaGRHZzZJSFZ5YkNnalkyeHBjSEJoZEdncE93b2dJQ0FnSUNCOUNnb2dJQ0FnSUNBdVkyeHpMVE1nZXdvZ0lDQWdJQ0FnSUdacGJHdzZJQ05sWWpCaE9HTTdDaUFnSUNBZ0lIMEtDaUFnSUNBZ0lDNWpiSE10TkNCN0NpQWdJQ0FnSUNBZ1ptbHNiRG9nSXpJek1XWXlNRHNLSUNBZ0lDQWdmUW9nSUNBZ1BDOXpkSGxzWlQ0S0lDQWdJRHhqYkdsd1VHRjBhQ0JwWkQwaVkyeHBjSEJoZEdnaVBnb2dJQ0FnSUNBOGNHRjBhQ0JqYkdGemN6MGlZMnh6TFRFaUlHUTlJbTA0Tnk0ME1TdzFPR013TERJMUxqWTVMVEUzTGpZMUxETTJMalV5TFRNNUxqUXhMRE0yTGpVeVV6a3VOVGtzT0RNdU5qa3NPUzQxT1N3MU9Dd3lOaTR5TXl3eExqUTRMRFE0TERFdU5EaHpNemt1TkRFc016QXVPRE1zTXprdU5ERXNOVFl1TlRKYUlpOFx1MDAyQkNpQWdJQ0E4TDJOc2FYQlFZWFJvUGdvZ0lEd3ZaR1ZtY3o0S0lDQThaeUJqYkdGemN6MGlZMnh6TFRJaVBnb2dJQ0FnUEhKbFkzUWdZMnhoYzNNOUltTnNjeTAwSWlCM2FXUjBhRDBpT1RZaUlHaGxhV2RvZEQwaU9UWWlMejRLSUNBZ0lEeHlaV04wSUdOc1lYTnpQU0pqYkhNdE15SWdlRDBpTXpjdU5EUWlJSGs5SWkwMUxqYzRJaUIzYVdSMGFEMGlNakV1TVRFaUlHaGxhV2RvZEQwaU5qTXVNek1pSUhKNFBTSTVJaUJ5ZVQwaU9TSXZQZ29nSUR3dlp6NEtQQzl6ZG1jXHUwMDJCIg0KICB9DQp9";

    static ProjectComponentPlugin()
    {
      Rhino.PlugIns.PlugIn.LoadPlugIn(s_rhinocode);

      // get platforms registry into a dynamic type to avoid using
      // the actual registry type. Otherwise when underlying api changes
      // it will throw an exception.
      dynamic projectRegistry = RhinoCode.Platforms;
      // get project server
      s_projectServer = projectRegistry.QueryLatest(s_rhino)?.ProjectServer;
      if (s_projectServer is null)
      {
        RhinoApp.WriteLine($"Error loading Grasshopper plugin. Missing \"{s_rhino}\" platform");
        return;
      }

      // get project
      var dctx = new InvokeContext
      {
        Inputs =
        {
          ["projectAssembly"] = typeof(ProjectComponentPlugin).Assembly,
          ["projectId"] = s_projectId,
          ["projectData"] = s_projectData,
        }
      };

      if (s_projectServer.TryInvoke("plugins/v1/deserialize", dctx)
            && dctx.Outputs.TryGet("project", out IProject project))
      {
        // server reports errors
        s_project = project;
      }

      // get icons
      if (!_iconData.Contains("ASSEMBLY-ICON"))
      {
        var ictx = new InvokeContext { Inputs = { ["iconData"] = _iconData } };
        if (s_projectServer.TryInvoke("plugins/v1/icon/gh/assembly", ictx)
              && ictx.Outputs.TryGet("icon", out SD.Bitmap icon))
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

      var dctx = new InvokeContext
      {
        Inputs =
        {
          ["component"] = ghcomponent,
          ["project"] = s_project,
          ["scriptData"] = serialized,
        }
      };

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

      var ictx = new InvokeContext
      {
        Inputs =
        {
          ["script"] = script,
        }
      };

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

      var dctx = new InvokeContext
      {
        Inputs =
        {
          ["component"] = ghcomponent,
          ["project"] = s_project,
          ["script"] = script,
        }
      };

      if (!s_projectServer.TryInvoke("plugins/v1/gh/dispose", dctx))
        throw new Exception("Error disposing Grasshopper script component");
    }
  }
}
