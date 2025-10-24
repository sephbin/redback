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
    static readonly string s_assemblyIconData = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAAW5JREFUSEu9VDtOw0AUdG0h4YICCbFvZYsW0dBR0KbjAiicAJEThIomkaCgDzeAEiq4Adwg1IDEEZY3zjP2Ji/2rpEYaaTV23kz3o83CcFsc5h9pZPhRzodgxhfc02m/4bc2PFjNvr+TKeuSdQwJ7J4EFFWGDsryLqnbOSZg6hhjkOeoZW2cHDjPQy6AiTkVdrCkBNdVM0hAQvSjbS3g5dr/cbQAJCOxWY9CkN3y42hATgPsdGhfT0YvgKwZRXLe18xKoB3QOxWwddyrjXFrcA69dqW914Rg7EB6jZx8UQX9wq4FNsaKOpi6263TlcCUNO0YG7oRWxrtAUMdg7dfOPq1xxj1DQtGB0AHu3uu/PtQUmMNU3FXgEx/I+AB7GtsUd0oIn7ED+s2Prgv/Bda4glnhyx9MHJZ1pDHDuebeyf3hhAQ2/qM9EEBH1CcHM6zZsot4u/SDPzyOcGrbTFA7drcTa4xjVRw5zI1iBJfgBeiEn6eGfuwQAAAABJRU5ErkJggg==";
    static readonly string s_categoryIconData = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAATtJREFUOE+FkiFPA0EQhU8SQkjTIEhoZ/Zur+ZSgeIX9AdUIAgK0bSmBodAIJAEXUlCUoMlITgUf6CihgSBrEAgK8qb7UC7y7J9yUv2dt/3brLZLCZm3nrdvRjOtm8exLKWPT1OqzwoG5bN5Ll2vgDsLGtLZmobttRYXPhLDcF3FDjIK8BeweYDmX2N/5UlvpPgfwWuhMyTxn21mA9/QqkCcU55R7GV8PfReihVYJkfFVupYJ6th5IFZOZyX4oCLoq2F4DTE4i5qzjGx0cYGNcHvwX39b53JsZlXiruxj8LA0fNanG7d+os6/AcBdeKu4KTMLDZfKV4/A42uSQ6VnwpTPEWC0ZN5rOqqh1Fl2oRdfAW5lEgsNyZYr5kLGmPQc7EX5bynsbjktEsUQ+XhJfJL+qR7HmPxynLvgHeQAfl+kqnzAAAAABJRU5ErkJggg==";

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
    public override string AssemblyVersion { get; } = "0.5.41.9421";
    public override string AssemblyDescription { get; } = "";
    public override string AuthorName { get; } = "Andrew Butler";
    public override string AuthorContact { get; } = "andrew.butler@strangercollective.com";
    public override GH_LibraryLicense AssemblyLicense { get; } = GH_LibraryLicense.unset;
    public override SD.Bitmap AssemblyIcon { get; } = PluginIcon;
  }

  public class ProjectComponentPlugin : GH_AssemblyPriority
  {
    static readonly Guid s_projectId = new Guid("7d142de6-7a37-4b73-961b-a5ef3f7bfbc8");
    static readonly string s_projectData = "ew0KICAiaG9zdCI6IHsNCiAgICAibmFtZSI6ICJSaGlubzNEIiwNCiAgICAidmVyc2lvbiI6ICI4LjIzLjI1MjUxXHUwMDJCMTMwMDEiLA0KICAgICJvcyI6ICJ3aW5kb3dzIiwNCiAgICAiYXJjaCI6ICJ4NjQiDQogIH0sDQogICJpZCI6ICI3ZDE0MmRlNi03YTM3LTRiNzMtOTYxYi1hNWVmM2Y3YmZiYzgiLA0KICAiaWRlbnRpdHkiOiB7DQogICAgIm5hbWUiOiAiUmVkYmFjayIsDQogICAgInZlcnNpb24iOiAiMC41LjQxIiwNCiAgICAicHVibGlzaGVyIjogew0KICAgICAgImVtYWlsIjogImFuZHJldy5idXRsZXJAc3RyYW5nZXJjb2xsZWN0aXZlLmNvbSIsDQogICAgICAibmFtZSI6ICJBbmRyZXcgQnV0bGVyIiwNCiAgICAgICJjb21wYW55IjogIlN0cmFuZ2VyIENvbGxlY3RpdmUiLA0KICAgICAgImNvdW50cnkiOiAiQXVzdHJhbGlhIiwNCiAgICAgICJ1cmwiOiAiaHR0cHM6Ly9naXRodWIuY29tL3NlcGhiaW4vIg0KICAgIH0sDQogICAgImltYWdlIjogew0KICAgICAgImxpZ2h0Ijogew0KICAgICAgICAidHlwZSI6ICJzdmciLA0KICAgICAgICAiZGF0YSI6ICJQSE4yWnlCM2FXUjBhRDBpTkRnaUlHaGxhV2RvZEQwaU5EZ2lJSFpsY25OcGIyNDlJakV1TVNJZ2VHMXNibk05SW1oMGRIQTZMeTkzZDNjdWR6TXViM0puTHpJd01EQXZjM1puSWlCNGJXeHVjenA0YkdsdWF6MGlhSFIwY0RvdkwzZDNkeTUzTXk1dmNtY3ZNVGs1T1M5NGJHbHVheUlnZG1sbGQwSnZlRDBpTUhCMElEQndkQ0EwT0hCMElEUTRjSFFpSUdacGJHd3RaR0Z5YXowaUkwWkdSaUlnYzNSeWIydGxMV1JoY21zOUltNXZibVVpUGcwS0lDQThjM1puSUdsa1BTSk1ZWGxsY2w4eElpQmtZWFJoTFc1aGJXVTlJa3hoZVdWeUlERWlJSGh0Ykc1elBTSm9kSFJ3T2k4dmQzZDNMbmN6TG05eVp5OHlNREF3TDNOMlp5SWdlRzFzYm5NNmVHeHBibXM5SW1oMGRIQTZMeTkzZDNjdWR6TXViM0puTHpFNU9Ua3ZlR3hwYm1zaUlIWnBaWGRDYjNnOUlqQWdNQ0E1TmlBNU5pSVx1MDAyQkRRb2dJQ0FnUEdSbFpuTWdlRzFzYm5NOUltaDBkSEE2THk5M2QzY3Vkek11YjNKbkx6SXdNREF2YzNabklqNE5DaUFnSUNBZ0lEeHpkSGxzWlQ0S0lDQWdJQ0FnTG1Oc2N5MHhMQ0F1WTJ4ekxUSWdld29nSUNBZ0lDQWdJR1pwYkd3NklHNXZibVU3Q2lBZ0lDQWdJSDBLQ2lBZ0lDQWdJQzVqYkhNdE1pQjdDaUFnSUNBZ0lDQWdZMnhwY0Mxd1lYUm9PaUIxY213b0kyTnNhWEJ3WVhSb0tUc0tJQ0FnSUNBZ2ZRb0tJQ0FnSUNBZ0xtTnNjeTB6SUhzS0lDQWdJQ0FnSUNCbWFXeHNPaUFqWldJd1lUaGpPd29nSUNBZ0lDQjlDZ29nSUNBZ0lDQXVZMnh6TFRRZ2V3b2dJQ0FnSUNBZ0lHWnBiR3c2SUNNeU16Rm1NakE3Q2lBZ0lDQWdJSDBLSUNBZ0lEd3ZjM1I1YkdVXHUwMDJCRFFvZ0lDQWdJQ0E4WTJ4cGNGQmhkR2dnYVdROUltTnNhWEJ3WVhSb0lqNE5DaUFnSUNBZ0lDQWdQSEJoZEdnZ1kyeGhjM005SW1Oc2N5MHhJaUJrUFNKdE9EY3VOREVzTlRoak1Dd3lOUzQyT1MweE55NDJOU3d6Tmk0MU1pMHpPUzQwTVN3ek5pNDFNbE01TGpVNUxEZ3pMalk1TERrdU5Ua3NOVGdzTWpZdU1qTXNNUzQwT0N3ME9Dd3hMalE0Y3pNNUxqUXhMRE13TGpnekxETTVMalF4TERVMkxqVXlXaUlnTHo0TkNpQWdJQ0FnSUR3dlkyeHBjRkJoZEdnXHUwMDJCRFFvZ0lDQWdQQzlrWldaelBnMEtJQ0FnSUR4bklHTnNZWE56UFNKamJITXRNaUlnZUcxc2JuTTlJbWgwZEhBNkx5OTNkM2N1ZHpNdWIzSm5Mekl3TURBdmMzWm5JajROQ2lBZ0lDQWdJRHh5WldOMElHTnNZWE56UFNKamJITXROQ0lnZDJsa2RHZzlJamsySWlCb1pXbG5hSFE5SWprMklpQXZQZzBLSUNBZ0lDQWdQSEpsWTNRZ1kyeGhjM005SW1Oc2N5MHpJaUI0UFNJek55NDBOQ0lnZVQwaUxUVXVOemdpSUhkcFpIUm9QU0l5TVM0eE1TSWdhR1ZwWjJoMFBTSTJNeTR6TXlJZ2NuZzlJamtpSUhKNVBTSTVJaUF2UGcwS0lDQWdJRHd2Wno0TkNpQWdQQzl6ZG1jXHUwMDJCRFFvOEwzTjJaejQ9Ig0KICAgICAgfSwNCiAgICAgICJwcm9qZWN0SWNvbiI6IHsNCiAgICAgICAgImxpZ2h0Ijogew0KICAgICAgICAgICJieXRlcyI6ICJpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBQmdBQUFBWUNBWUFBQURnZHozNEFBQUFCR2RCVFVFQUFMR1BDL3hoQlFBQUFBbHdTRmx6QUFBT3dnQUFEc0lCRlNoS2dBQUFBVzVKUkVGVVNFdTlWRHRPdzBBVWRHMGg0WUlDQ2JGdlpZc1cwZEJSMEtiakFpaWNBSkVUaElvbWthQ2dEemVBRWlxNEFkd2cxSURFRVpZM3pqUDJKaS8ycnBFWWFhVFYyM2t6M284M0NjRnNjNWg5cFpQaFJ6b2RneGhmYzAybS80YmMyUEZqTnZyXHUwMDJCVEtldVNkUXdKN0o0RUZGV0dEc3J5THFuYk9TWmc2aGhqa09lb1pXMmNIRGpQUXk2QWlUa1ZkckNrQk5kVk0waEFRdlNqYlMzZzVkci9jYlFBSkNPeFdZOUNrTjN5NDJoQVRnUHNkR2hmVDBZdmdLd1pSWExlMTh4S29CM1FPeFd3ZGR5cmpYRnJjQTY5ZHFXOTE0Umc3RUI2alp4OFVRWDl3cTRGTnNhS09waTYyNjNUbGNDVU5PMFlHN29SV3hydEFVTWRnN2RmT1BxMXh4ajFEUXRHQjBBSHUzdXUvUHRRVW1NTlUzRlhnRXgvSVx1MDAyQkFCN0d0c1VkMG9JbjdFRFx1MDAyQnMyUHJndi9CZGE0Z2xuaHl4OU1ISloxcERIRHVlYmV5ZjNoaEFRMi9xTTlFRUJIMUNjSE02elpzb3Q0dS9TRFB6eU9jR3JiVEZBN2RyY1RhNHhqVlJ3NXpJMWlCSmZnQmVpRW42ZUdmdXdRQUFBQUJKUlU1RXJrSmdnZz09IiwNCiAgICAgICAgICAid2lkdGgiOiAyNCwNCiAgICAgICAgICAiaGVpZ2h0IjogMjQNCiAgICAgICAgfSwNCiAgICAgICAgImRhcmsiOiB7DQogICAgICAgICAgImJ5dGVzIjogImlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFCZ0FBQUFZQ0FZQUFBRGdkejM0QUFBQUJHZEJUVUVBQUxHUEMveGhCUUFBQUFsd1NGbHpBQUFPd2dBQURzSUJGU2hLZ0FBQUFCaEpSRUZVU0V2dHdRRUJBQUFBZ2lEL3I2NGhRQUFBWEEwSkdBQUIwQUpiWGdBQUFBQkpSVTVFcmtKZ2dnPT0iLA0KICAgICAgICAgICJ3aWR0aCI6IDI0LA0KICAgICAgICAgICJoZWlnaHQiOiAyNA0KICAgICAgICB9LA0KICAgICAgICAiaWNvRGF0YSI6ICJBQUFCQUFFQUdCZ0FBQUVBSUFCSUFnQUFGZ0FBQUlsUVRrY05DaG9LQUFBQURVbElSRklBQUFBWUFBQUFHQWdHQUFBQTRIYzlcdTAwMkJBQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzSUFBQTdDQVJVb1NvQUFBQUhxU1VSQlZFaExwWlpoTGtOQkVNZG5xdUtibG04aXNVMUZRaUpSVHRBTENEZmdCamdCTjhBSlNseUFEejdqQWxLSmhFUkNWeUtcdTAwMkJTSGppZ3lqRzdHYTY3ZmE5NVQzdmw3eDBaanJkL1x1MDAyQjdzdk4waXBLQlJXaWt2dG1lWHZnRXJ4aThBdFE0R0w0ODJvdjBYbS9BTEJma01VcDJvYkk3aDZCMEI3dkZzdHN4ajdCbU9tZThrTFVoUVFDbFZucHlvTkJEdG9HVUpPMHpNZk1jaUp5Wlh3akdDQWdPRURSNWxWZHdnTEZMbjNCTnhZeVFLVkpWYTV4OHVpL3NubkZ1YlZHcEhYSVx1MDAyQllBQ1x1MDAyQjNnb0RiNG1ZQTExaWtMbzRqSmxBazJCSXpNMFFZMjNSUHdNeWUxN3NpYm1iTWZ2U3Z3aE1ZZ1BSMUQwSlx1MDAyQlkzZ0NTTGdtNXYvaEN2UzJyUk93UVFUN3B1YWxDRkFUc3l2QXdWZ0g1TUNOMVZzaXA5clBUZkZKckM1SnNRNUV5UUpCam9ldTRhM3dJUjVZMjhUUzRFNVRiaS91LzNnZmR4ajdIb2FGOXJpMXp3Y2Y0TEh3YXUwa2lPanM5bDdiVmFSYWdjRU1lRHgwWlovZkJ1OG50VUJHM0QzaEJOZzRGRE0vQ0tkaWRRVnV0RzV5OGJTNHVmanFtYXhYSXVJTFJNd2MwSzdXdWlXT09YNjZQRWRSYzZSVW1rZkVhUWxsZ1x1MDAyQmppa3lcdTAwMkJwS0lyZUpSTGY1QzlPNERZN0VqYzFwalY1OERyUDN2c2o0TjZEZnZoV1cwV0NkVDY4NWlTVURPXHUwMDJCYktlMnQxbnNTOFFnS2RKaFNxc2FiVnVORTd5QWtnQmJYdDJtYkl3akFENHNiaVdxUzlYS1x1MDAyQkFBQUFBRWxGVGtTdVFtQ0MiDQogICAgICB9LA0KICAgICAgImNhdGVnb3J5SWNvbiI6IHsNCiAgICAgICAgImxpZ2h0Ijogew0KICAgICAgICAgICJieXRlcyI6ICJpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBQkFBQUFBUUNBWUFBQUFmOC85aEFBQUFCR2RCVFVFQUFMR1BDL3hoQlFBQUFBbHdTRmx6QUFBT3dnQUFEc0lCRlNoS2dBQUFBVHRKUkVGVU9FXHUwMDJCRmtpRlBBMEVRaFU4U1FralRJRWhvWi9adXJcdTAwMkJaU2dlSVg5QWRVSUFnSzBiU21Cb2RBSUpBRVhVbENVb01sSVRnVWY2Q2loZ1NCckVBZ0s4cWI3VUM3eTdKOXlVdjJkdC8zYnJMWkxDWm0zbnJkdlJqT3RtOGV4TEtXUFQxT3F6d29HNWJONUxsMnZnRHNMR3RMWm1vYnR0UllYUGhMRGNGM0ZEaklLOEJld2VZRG1YMk4vNVVsdnBQZ2Z3V3VoTXlUeG4yMW1BOS9RcWtDY1U1NVI3R1Y4UGZSZWloVllKa2ZGVnVwWUo2dGg1SUZaT1p5WDRvQ0xvcTJGNERURTRpNXF6akd4MGNZR05jSHZ3WDM5YjUzSnNabFhpcnV4ajhMQTBmTmFuRzdkXHUwMDJCb3M2L0FjQmRlS3U0S1RNTERaZktWNC9BNDJ1U1E2Vm53cFRQRVdDMFpONXJPcXFoMUZsMm9SZGZBVzVsRWdzTnlaWXI1a0xHbVBRYzdFWDVieW5zYmprdEVzVVFcdTAwMkJYaEpmSkxcdTAwMkJxUjdIbVB4eW5MdmdIZVFBZmxcdTAwMkJrcW56QUFBQUFCSlJVNUVya0pnZ2c9PSIsDQogICAgICAgICAgIndpZHRoIjogMTYsDQogICAgICAgICAgImhlaWdodCI6IDE2DQogICAgICAgIH0sDQogICAgICAgICJkYXJrIjogew0KICAgICAgICAgICJieXRlcyI6ICJpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBQkFBQUFBUUNBWUFBQUFmOC85aEFBQUFCR2RCVFVFQUFMR1BDL3hoQlFBQUFBbHdTRmx6QUFBT3dnQUFEc0lCRlNoS2dBQUFBQk5KUkVGVU9FOWpHQVdqWUJTTUFqQmdZQUFBQkJBQUFhZEVmR01BQUFBQVNVVk9SSzVDWUlJPSIsDQogICAgICAgICAgIndpZHRoIjogMTYsDQogICAgICAgICAgImhlaWdodCI6IDE2DQogICAgICAgIH0NCiAgICAgIH0NCiAgICB9DQogIH0sDQogICJzZXR0aW5ncyI6IHsNCiAgICAiYnVpbGRQYXRoIjogImZpbGU6Ly8vWDovOTNfRGV2ZWxvcG1lbnQvUGFja2FnZU1hbmFnZXIvcmVkYmFjay9yaDhQcm9qZWN0L2J1aWxkL3JoOCIsDQogICAgImJ1aWxkVGFyZ2V0Ijogew0KICAgICAgImhvc3QiOiB7DQogICAgICAgICJuYW1lIjogIlJoaW5vM0QiLA0KICAgICAgICAidmVyc2lvbiI6ICI4Ig0KICAgICAgfSwNCiAgICAgICJ0aXRsZSI6ICJSaGlubzNEICg4LiopIiwNCiAgICAgICJzbHVnIjogInJoOCINCiAgICB9LA0KICAgICJwdWJsaXNoVGFyZ2V0Ijogew0KICAgICAgInRpdGxlIjogIk1jTmVlbCBZYWsgU2VydmVyIg0KICAgIH0NCiAgfSwNCiAgImNvZGVzIjogW10NCn0=";
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
