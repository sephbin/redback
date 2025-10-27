using System;
using System.IO;
using System.Text;
using SD = System.Drawing;

using Rhino;
using Grasshopper.Kernel;

namespace RhinoCodePlatform.Rhino3D.Projects.Plugin.GH
{
  public sealed class AssemblyInfo : GH_AssemblyInfo
  {
    static readonly string s_assemblyIconData = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAAWRJREFUSEu9VDFOwzAUzRwhNQMDEsLfssWKWNgYWLtxARROgOgJ2omllWBgb28AI0xwA3qDdgYkjhD0ElqSZytxjMSTnmR9v/9e4vw4SQIwH+TZZzrN39PZGMT6dpBnrIuCUXr8lI2+PtJZUSdq2GN9MEQks0rPrejiORs1zEHUsGeUfoGW+zthlH6AQVfAT8gb97fCiFxvmkMCKsod+3ghIrrZGBpQhpyxnwOrZMGNoQH4HuzXgO/p+wRUbHkLPvuoACUL9t3CKr1yGvoGiC68Y1vOvUccE+A9Jity7gqjAybsj4CJK6x4v3vhBKDGug2Nklf2bw0Y7p8Uq52brTnWqLEuOgA8PTgqrvaGJbHm/T8H9OF/BDyyf3IocszCWOKHZf8SVsmaxTHElcPeJYzIJYv7s+Paxvm5TYFUsvReE3VAEBOCyek0r6M8LiVLNnKoZA0t9wcD01V9G4zxL1HDHusZ316ISfpIyju/AAAAAElFTkSuQmCC";
    static readonly string s_categoryIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAAS1JREFUOE+NkiFPA0EQhU+ShpCmQZBQ5s3tXM0FgeIX9AcgEASFaMDU4BAIBLKprmxCgsGSEByKP4DAkCCQJxDIiiOzXO+60yvXl7xkb++9byebjaIaAdh43boaZq3Rg1rXumdztUp2k66A357bl3nWGnnrWojfpSuJzQcC0BbiTwH7UgAA5w78BWDH9koJYarBVQAPIX6yPa8ecDAP/QdQxxT3bV9Pn6wLEODR9iMHZGsDiGd6X1XZuf3whAbA3xRHJUA/bOC+c1EC7jrnSwBHfF1NAJzZwOFemo+3T711bf874ttFwIkNNBs3FaDmDpqcEB2XgGKKDxtaaeLvNE03A0CPqC+E2VK4xnpnQXkuHUvptlCdjB+heGB7gXQ0IRoI9GXipfBE94LHU+gX3kAH5XIWIckAAAAASUVORK5CYII=";

    public static readonly SD.Bitmap PluginIcon = default;
    public static readonly SD.Bitmap PluginCategoryIcon = default;

    static AssemblyInfo()
    {
      if (!s_assemblyIconData.Contains("ASSEMBLY-ICON"))
      {
        using (var aicon = new MemoryStream(Convert.FromBase64String(s_assemblyIconData)))
          PluginIcon = new SD.Bitmap(aicon);
      }

      if (!s_categoryIconData.Contains("ASSEMBLY-CATEGORY-ICON"))
      {
        using (var cicon = new MemoryStream(Convert.FromBase64String(s_categoryIconData)))
          PluginCategoryIcon = new SD.Bitmap(cicon);
      }
    }

    public override Guid Id { get; } = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");

    public override string AssemblyName { get; } = "Redback.Components";
    public override string AssemblyVersion { get; } = "0.5.43.9432";
    public override string AssemblyDescription { get; } = "";
    public override string AuthorName { get; } = "Andrew Butler";
    public override string AuthorContact { get; } = "andrew.butler@strangercollective.com";
    public override GH_LibraryLicense AssemblyLicense { get; } = GH_LibraryLicense.unset;
    public override SD.Bitmap AssemblyIcon { get; } = PluginIcon;
  }

  public class ProjectComponentPlugin : GH_AssemblyPriority
  {
    static readonly Guid s_projectId = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");
    static readonly string s_projectData = "ew0KICAiaG9zdCI6IHsNCiAgICAibmFtZSI6ICJSaGlubzNEIiwNCiAgICAidmVyc2lvbiI6ICI4LjI0LjI1MjgxXHUwMDJCMTUwMDEiLA0KICAgICJvcyI6ICJ3aW5kb3dzIiwNCiAgICAiYXJjaCI6ICJ4NjQiDQogIH0sDQogICJpZCI6ICI3ZDE0MmRlNi03YTM3LTRiNzMtOTYxYi1hNWVmM2Y3YmZiYzgiLA0KICAiaWRlbnRpdHkiOiB7DQogICAgIm5hbWUiOiAiUmVkYmFjayIsDQogICAgInZlcnNpb24iOiAiMC41LjQzIiwNCiAgICAicHVibGlzaGVyIjogew0KICAgICAgImVtYWlsIjogImFuZHJldy5idXRsZXJAc3RyYW5nZXJjb2xsZWN0aXZlLmNvbSIsDQogICAgICAibmFtZSI6ICJBbmRyZXcgQnV0bGVyIiwNCiAgICAgICJjb21wYW55IjogIlN0cmFuZ2VyIENvbGxlY3RpdmUiLA0KICAgICAgImNvdW50cnkiOiAiQXVzdHJhbGlhIiwNCiAgICAgICJ1cmwiOiAiaHR0cHM6Ly9naXRodWIuY29tL3NlcGhiaW4vIg0KICAgIH0sDQogICAgImNvcHlyaWdodCI6ICJDb3B5cmlnaHQgXHUwMEE5IDIwMjUgQW5kcmV3IEJ1dGxlciIsDQogICAgImltYWdlIjogew0KICAgICAgImxpZ2h0Ijogew0KICAgICAgICAidHlwZSI6ICJzdmciLA0KICAgICAgICAiZGF0YSI6ICJQSE4yWnlCM2FXUjBhRDBpTkRnaUlHaGxhV2RvZEQwaU5EZ2lJSFpsY25OcGIyNDlJakV1TVNJZ2VHMXNibk05SW1oMGRIQTZMeTkzZDNjdWR6TXViM0puTHpJd01EQXZjM1puSWlCNGJXeHVjenA0YkdsdWF6MGlhSFIwY0RvdkwzZDNkeTUzTXk1dmNtY3ZNVGs1T1M5NGJHbHVheUlnZG1sbGQwSnZlRDBpTUhCMElEQndkQ0EwT0hCMElEUTRjSFFpSUdacGJHd3RaR0Z5YXowaUkwWkdSaUlnYzNSeWIydGxMV1JoY21zOUltNXZibVVpUGcwS0lDQThjM1puSUdsa1BTSk1ZWGxsY2w4eElpQmtZWFJoTFc1aGJXVTlJa3hoZVdWeUlERWlJSGh0Ykc1elBTSm9kSFJ3T2k4dmQzZDNMbmN6TG05eVp5OHlNREF3TDNOMlp5SWdlRzFzYm5NNmVHeHBibXM5SW1oMGRIQTZMeTkzZDNjdWR6TXViM0puTHpFNU9Ua3ZlR3hwYm1zaUlIWnBaWGRDYjNnOUlqQWdNQ0E1TmlBNU5pSVx1MDAyQkRRb2dJQ0FnUEdSbFpuTWdlRzFzYm5NOUltaDBkSEE2THk5M2QzY3Vkek11YjNKbkx6SXdNREF2YzNabklqNE5DaUFnSUNBZ0lEeHpkSGxzWlQ0S0lDQWdJQ0FnTG1Oc2N5MHhMQ0F1WTJ4ekxUSWdld29nSUNBZ0lDQWdJR1pwYkd3NklHNXZibVU3Q2lBZ0lDQWdJSDBLQ2lBZ0lDQWdJQzVqYkhNdE1pQjdDaUFnSUNBZ0lDQWdZMnhwY0Mxd1lYUm9PaUIxY213b0kyTnNhWEJ3WVhSb0tUc0tJQ0FnSUNBZ2ZRb0tJQ0FnSUNBZ0xtTnNjeTB6SUhzS0lDQWdJQ0FnSUNCbWFXeHNPaUFqWldJd1lUaGpPd29nSUNBZ0lDQjlDZ29nSUNBZ0lDQXVZMnh6TFRRZ2V3b2dJQ0FnSUNBZ0lHWnBiR3c2SUNNeU16Rm1NakE3Q2lBZ0lDQWdJSDBLSUNBZ0lEd3ZjM1I1YkdVXHUwMDJCRFFvZ0lDQWdJQ0E4WTJ4cGNGQmhkR2dnYVdROUltTnNhWEJ3WVhSb0lqNE5DaUFnSUNBZ0lDQWdQSEJoZEdnZ1kyeGhjM005SW1Oc2N5MHhJaUJrUFNKdE9EY3VOREVzTlRoak1Dd3lOUzQyT1MweE55NDJOU3d6Tmk0MU1pMHpPUzQwTVN3ek5pNDFNbE01TGpVNUxEZ3pMalk1TERrdU5Ua3NOVGdzTWpZdU1qTXNNUzQwT0N3ME9Dd3hMalE0Y3pNNUxqUXhMRE13TGpnekxETTVMalF4TERVMkxqVXlXaUlnTHo0TkNpQWdJQ0FnSUR3dlkyeHBjRkJoZEdnXHUwMDJCRFFvZ0lDQWdQQzlrWldaelBnMEtJQ0FnSUR4bklHTnNZWE56UFNKamJITXRNaUlnZUcxc2JuTTlJbWgwZEhBNkx5OTNkM2N1ZHpNdWIzSm5Mekl3TURBdmMzWm5JajROQ2lBZ0lDQWdJRHh5WldOMElHTnNZWE56UFNKamJITXROQ0lnZDJsa2RHZzlJamsySWlCb1pXbG5hSFE5SWprMklpQXZQZzBLSUNBZ0lDQWdQSEpsWTNRZ1kyeGhjM005SW1Oc2N5MHpJaUI0UFNJek55NDBOQ0lnZVQwaUxUVXVOemdpSUhkcFpIUm9QU0l5TVM0eE1TSWdhR1ZwWjJoMFBTSTJNeTR6TXlJZ2NuZzlJamtpSUhKNVBTSTVJaUF2UGcwS0lDQWdJRHd2Wno0TkNpQWdQQzl6ZG1jXHUwMDJCRFFvOEwzTjJaejQ9Ig0KICAgICAgfSwNCiAgICAgICJwcm9qZWN0SWNvbiI6IHsNCiAgICAgICAgImxpZ2h0Ijogew0KICAgICAgICAgICJieXRlcyI6ICJpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBQmdBQUFBWUNBWUFBQURnZHozNEFBQUFCR2RCVFVFQUFMR1BDL3hoQlFBQUFBbHdTRmx6QUFBT3dnQUFEc0lCRlNoS2dBQUFBV1JKUkVGVVNFdTlWREZPd3pBVXpSd2hOUU1ERXNMZnNzV0tXTmdZV0x0eEFSUk9nT2dKMm9tbGxXQmdiMjhBSTB4d0EzcURkZ1lramhEMEVscVNaeXR4ak1TVG5tUjl2LzllNHZ3NFNRSXdIXHUwMDJCVFpaenJOMzlQWkdNVDZkcEJuckl1Q1VYcjhsSTJcdTAwMkJQdEpaVVNkcTJHTjlNRVFrczByUHJlamlPUnMxekVIVXNHZVVmb0dXXHUwMDJCenRobEg2QVFWZkFUOGdiOTdmQ2lGeHZta01DS3NvZFx1MDAyQjNnaElyclpHQnBRaHB5eG53T3JaTUdOb1FINEh1elhnTy9wXHUwMDJCd1JVYkhrTFB2dW9BQ1VMOXQzQ0tyMXlHdm9HaUM2OFkxdk92VWNjRVx1MDAyQkE5Sml0eTdncWpBeWJzajRDSks2eDR2M3ZoQktER3VnMk5rbGYyYncwWTdwOFVxNTJiclRuV3FMRXVPZ0E4UFRncXJ2YUdKYkhtL1Q4SDlPRi9CRHl5ZjNJb2NzekNXT0tIWmY4U1ZzbWF4VEhFbGNQZUpZeklKWXY3c1x1MDAyQlBheHZtNVRZRlVzdlJlRTNWQUVCT0N5ZWswcjZNOExpVkxObktvWkEwdDl3Y0QwMVY5RzR6eEwxSERIdXNaMzE2SVNmcEl5anUvQUFBQUFFbEZUa1N1UW1DQyIsDQogICAgICAgICAgIndpZHRoIjogMjQsDQogICAgICAgICAgImhlaWdodCI6IDI0DQogICAgICAgIH0sDQogICAgICAgICJkYXJrIjogew0KICAgICAgICAgICJieXRlcyI6ICJpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBQmdBQUFBWUNBWUFBQURnZHozNEFBQUFCR2RCVFVFQUFMR1BDL3hoQlFBQUFBbHdTRmx6QUFBT3dnQUFEc0lCRlNoS2dBQUFBQmhKUkVGVVNFdnR3UUVOQUFBQXdxRDNUXHUwMDJCM3NBUlFBQURjSkdBQUJDUHUxNGdBQUFBQkpSVTVFcmtKZ2dnPT0iLA0KICAgICAgICAgICJ3aWR0aCI6IDI0LA0KICAgICAgICAgICJoZWlnaHQiOiAyNA0KICAgICAgICB9LA0KICAgICAgICAiaWNvRGF0YSI6ICJBQUFCQUFFQUdCZ0FBQUVBSUFCT0FnQUFGZ0FBQUlsUVRrY05DaG9LQUFBQURVbElSRklBQUFBWUFBQUFHQWdHQUFBQTRIYzlcdTAwMkJBQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzSUFBQTdDQVJVb1NvQUFBQUh3U1VSQlZFaExwWlpOYnROUUZJWFBjVkpsUmt4bkZWS3U1UWdKSkNRQ0svQUdFT3dnMlVITEN1b2RBQ3RJRUJ1Z2c0eExONENDaE5SS2tkbzhKTlFKRXJqcUFMVk5MeU0vN091NGRadHY5TjY1Vi83czVcdTAwMkJjZm9nSGo3akI4ZGZuczlUVVlBVUFBWFh6YVx1MDAyQkw3M052djR4L1phQWh0WTRsNjB1OFhORXdVbkJGSUNxWUtUcDl3OGlYdlJydTIzMUFwRUpPejNvakdKbEVCbzZ3UkNFbW5jaS9aRnBGTFBxUlcwbEdNUUk1dGJTQ1F0NWI3TmMxWUtZcEVkRW05c1hnZUpRVi9rdmMyeFNpQWlFY0YzTnI4ZGJ2ZEZFcHRXQkcxRmFyT21xTEp5MDBzQ0VZbEFEb3ZaWFNDUjJLc29DVnBvdnU2MWFIbGpsQVJVYmhmbjk0SWNGcmV0RjRoSUNDTHlqV3ZRQmdiNTJBdmFRR1VIcklFL1ZuR0p2TlV5Yi9cdTAwMkJ5MGNvc1IzVzFvSlpwNXdqbndZV2Zud2NYbUhhT1NqMTFNQi8wUlZLZ3VvOXp0cTRmNE9YbEl3REExNDJmT0EzT2JJdEhWUVx1MDAyQk9mN2dFVGE4QUFFNkRNMHc3aDVoMkRtODh1S1d4NEk3NDc0UVhCTUJuWDE0WDRrc1x1MDAyQjlJSzVjek9vT3RcdTAwMkIwQnN2Q3laYVdTSG4vRjkxLzlJTnpicEhQV3NYUzd5eWJQZXgyWDVCOFVzd2JvL3J0aWhobFdmWTNqeW8zZVVtTVZIWFA1cmVocWdkWFJPS2NLLzBJXHUwMDJCT2ZBRW91TXFOZ0JcdTAwMkJkeldTcWc2SmRKajV5YTJoSnNFT1k5RkJrdGdRSlJmaEFvc1dzQnM3dHlzbUZ2XHUwMDJCQVlzYmlXcW1kbHZBQUFBQUFFbEZUa1N1UW1DQyINCiAgICAgIH0sDQogICAgICAiY2F0ZWdvcnlJY29uIjogew0KICAgICAgICAibGlnaHQiOiB7DQogICAgICAgICAgImJ5dGVzIjogImlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFCQUFBQUFRQ0FZQUFBQWY4LzloQUFBQUJHZEJUVUVBQUxHUEMveGhCUUFBQUFsd1NGbHpBQUFPd2dBQURzSUJGU2hLZ0FBQUFTMUpSRUZVT0VcdTAwMkJOa2lGUEEwRVFoVVx1MDAyQlNocENtUVpCUTVzM3RYTTBGZ2VJWDlBY2dFQVNGYU1EVTRCQUlCTEtwcm14Q2dzR1NFQnlLUDREQWtDQ1FKeERJaWlPelhPXHUwMDJCNjB5dlhsN3hrYlx1MDAyQlx1MDAyQjlieWViamFJYUFkaDQzYm9hWnEzUmcxclh1bWR6dFVwMms2NkEzNTdibDNuV0dubnJXb2pmcFN1SnpRY0MwQmJpVHdIN1VnQUE1dzc4QldESDlrb0pZYXJCVlFBUElYNnlQYThlY0RBUC9RZFF4eFQzYlY5UG42d0xFT0RSOWlNSFpHc0RpR2Q2WDFYWnVmM3doQWJBM3hSSEpVQS9iT0NcdTAwMkJjMUVDN2pyblN3QkhmRjFOQUp6WndPRmVtb1x1MDAyQjNUNzExYmY4NzR0dEZ3SWtOTkJzM0ZhRG1EcHFjRUIyWGdHS0tEeHRhYWVMdk5FMDNBMENQcUNcdTAwMkJFMlZLNHhucG5RWGt1SFV2cHRsQ2RqQlx1MDAyQmhlR0I3Z1hRMElSb0k5R1hpcGZCRTk0TEhVXHUwMDJCZ1gza0FINVhJV0lja0FBQUFBU1VWT1JLNUNZSUk9IiwNCiAgICAgICAgICAid2lkdGgiOiAxNiwNCiAgICAgICAgICAiaGVpZ2h0IjogMTYNCiAgICAgICAgfSwNCiAgICAgICAgImRhcmsiOiB7DQogICAgICAgICAgImJ5dGVzIjogImlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFCQUFBQUFRQ0FZQUFBQWY4LzloQUFBQUJHZEJUVUVBQUxHUEMveGhCUUFBQUFsd1NGbHpBQUFPd2dBQURzSUJGU2hLZ0FBQUFCSkpSRUZVT0U5allCZ0ZvMkFVakFJSUFBQUVFQUFCVEx0R1ZRQUFBQUJKUlU1RXJrSmdnZz09IiwNCiAgICAgICAgICAid2lkdGgiOiAxNiwNCiAgICAgICAgICAiaGVpZ2h0IjogMTYNCiAgICAgICAgfQ0KICAgICAgfQ0KICAgIH0NCiAgfSwNCiAgInNldHRpbmdzIjogew0KICAgICJidWlsZFBhdGgiOiAiZmlsZTovLy9FOi9teWRldi9yZWRiYWNrL3JoOFByb2plY3QvYnVpbGQvcmg4IiwNCiAgICAiYnVpbGRUYXJnZXQiOiB7DQogICAgICAiaG9zdCI6IHsNCiAgICAgICAgIm5hbWUiOiAiUmhpbm8zRCIsDQogICAgICAgICJ2ZXJzaW9uIjogIjgiDQogICAgICB9LA0KICAgICAgInRpdGxlIjogIlJoaW5vM0QgKDguKikiLA0KICAgICAgInNsdWciOiAicmg4Ig0KICAgIH0sDQogICAgInB1Ymxpc2hUYXJnZXQiOiB7DQogICAgICAidGl0bGUiOiAiTWNOZWVsIFlhayBTZXJ2ZXIiDQogICAgfQ0KICB9LA0KICAiY29kZXMiOiBbXQ0KfQ==";
    static readonly dynamic s_projectServer = default;
    static readonly object s_project = default;

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
    }

    public override GH_LoadingInstruction PriorityLoad()
    {
      if (AssemblyInfo.PluginCategoryIcon is SD.Bitmap icon)
      {
        Grasshopper.Instances.ComponentServer.AddCategoryIcon("Redback", icon);
      }
      Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Redback", "Redback"[0]);

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
