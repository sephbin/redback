using System;
using SD = System.Drawing;

using Rhino.Geometry;

using Grasshopper.Kernel;

namespace RhinoCodePlatform.Rhino3D.Projects.Plugin.GH
{
  public sealed class ProjectComponent_20dc8abf : GH_Component
  {
    readonly SD.Bitmap _icon = default;
    readonly string _scriptData = "ew0KICAidHlwZSI6ICJzY3JpcHQiLA0KICAic2NyaXB0Ijogew0KICAgICJsYW5ndWFnZSI6IHsNCiAgICAgICJpZCI6ICIqLioucHl0aG9uIiwNCiAgICAgICJ2ZXJzaW9uIjogIjMuKi4qIg0KICAgIH0sDQogICAgInRpdGxlIjogIlNlcmlhbGl6ZSBNb2RlbCBPYmplY3QiLA0KICAgICJ0ZXh0IjogImFXMXdiM0owSUdwemIyNE5DbWx0Y0c5eWRDQnlhR2x1YjNOamNtbHdkSE41Ym5SaGVDQmhjeUJ5Y3cwS2FXMXdiM0owSUVkeVlYTnphRzl3Y0dWeUlHRnpJR2RvRFFwc1lYbGxjaUE5SUU5Q0xreGhlV1Z5RFFwdmRYUlBRaUE5SUhzTkNpQWdJQ0FpWjJWdmJXVjBjbmtpT250OUxBMEtJQ0FnSUNKd2NtOXdaWEowYVdWeklqcDdEUW9nSUNBZ0lDQWdJQ0pmY21ocGJtOHpSRjlzWVhsbGNrNWhiV1VpT2lCemRISW9iR0Y1WlhJdVRtRnRaU2tzRFFvZ0lDQWdJQ0FnSUNKZmNtaHBibTh6UkY5c1lYbGxjbEJoZEdnaU9pQnpkSElvYkdGNVpYSXVVR0YwYUNrc0RRb2dJQ0FnSUNBZ0lDSmZjbWhwYm04elJGOXZZbXBsWTNST1lXMWxJam9nYzNSeUtFOUNMazVoYldVcExBMEtJQ0FnSUgwTkNuME5DZzBLRFFwblpXOXRJRDBnWjJndVMyVnlibVZzTGtkSVgwTnZiblpsY25RdVZHOUhaVzl0WlhSeWVVSmhjMlVvVDBJdVNXUXBEUXAwY25rNklDQWdJRzkxZEU5Q1d5Sm5aVzl0WlhSeWVTSmRJRDBnYW5OdmJpNXNiMkZrY3loblpXOXRMbFJ2U2xOUFRpaE9iMjVsS1NrTkNtVjRZMlZ3ZERvZ2NHRnpjdzBLYjNWMFQwSmJJbkJ5YjNCbGNuUnBaWE1pWFM1MWNHUmhkR1VvWkdsamRDaFBRaTVWYzJWeVZHVjRkQ2twRFFvTkNnMEtEUW9OQ2cwS0RRcEtUeUE5SUdwemIyNHVaSFZ0Y0hNb2IzVjBUMElwRFFvPSIsDQogICAgInVyaSI6ICJyaGlub2NvZGU6Ly8vZ3Jhc3Nob3BwZXIvMS8yMGRjOGFiZi1lYTA4LTQ2YWQtODJmYS1jZDFlZmQ4MDIzZTEvIiwNCiAgICAiaWQiOiAiMjBkYzhhYmYtZWEwOC00NmFkLTgyZmEtY2QxZWZkODAyM2UxIiwNCiAgICAibmlja25hbWUiOiAiU2VyaWFsaXplIE1vZGVsIE9iamVjdCIsDQogICAgImltYWdlIjogew0KICAgICAgImxpZ2h0Ijogew0KICAgICAgICAidHlwZSI6ICJzdmciLA0KICAgICAgICAiZGF0YSI6ICJQSE4yWnlCcFpEMGlUR0Y1WlhKZk1TSWdaR0YwWVMxdVlXMWxQU0pNWVhsbGNpQXhJaUI0Yld4dWN6MGlhSFIwY0RvdkwzZDNkeTUzTXk1dmNtY3ZNakF3TUM5emRtY2lJSFpwWlhkQ2IzZzlJakFnTUNBeU5DQXlOQ0lcdTAwMkJDaUFnUEdSbFpuTVx1MDAyQkNpQWdJQ0E4YzNSNWJHVVx1MDAyQkNpQWdJQ0FnSUM1amJITXRNU0I3Q2lBZ0lDQWdJQ0FnWm1sc2JEb2dJekF3TURzS0lDQWdJQ0FnZlFvS0lDQWdJQ0FnTG1Oc2N5MHhMQ0F1WTJ4ekxUSXNJQzVqYkhNdE15d2dMbU5zY3kwMExDQXVZMnh6TFRVZ2V3b2dJQ0FnSUNBZ0lITjBjbTlyWlMxM2FXUjBhRG9nTUhCNE93b2dJQ0FnSUNCOUNnb2dJQ0FnSUNBdVkyeHpMVFlnZXdvZ0lDQWdJQ0FnSUhOMGNtOXJaUzF0YVhSbGNteHBiV2wwT2lBeE1Ec0tJQ0FnSUNBZ0lDQnpkSEp2YTJVdGQybGtkR2c2SURKd2VEc0tJQ0FnSUNBZ2ZRb0tJQ0FnSUNBZ0xtTnNjeTAyTENBdVkyeHpMVGNnZXdvZ0lDQWdJQ0FnSUdacGJHdzZJRzV2Ym1VN0NpQWdJQ0FnSUNBZ2MzUnliMnRsT2lBak5HWXhPREJpT3dvZ0lDQWdJQ0I5Q2dvZ0lDQWdJQ0F1WTJ4ekxUY2dld29nSUNBZ0lDQWdJSE4wY205clpTMXNhVzVsWTJGd09pQnliM1Z1WkRzS0lDQWdJQ0FnSUNCemRISnZhMlV0YkdsdVpXcHZhVzQ2SUhKdmRXNWtPd29nSUNBZ0lDQWdJSE4wY205clpTMTNhV1IwYURvZ0xqYzFjSGc3Q2lBZ0lDQWdJSDBLQ2lBZ0lDQWdJQzVqYkhNdE1pQjdDaUFnSUNBZ0lDQWdabWxzYkRvZ0kyVmtObUl3TURzS0lDQWdJQ0FnZlFvS0lDQWdJQ0FnTG1Oc2N5MHpJSHNLSUNBZ0lDQWdJQ0JtYVd4c09pQWpabVprWVRZek93b2dJQ0FnSUNCOUNnb2dJQ0FnSUNBdVkyeHpMVFFnZXdvZ0lDQWdJQ0FnSUdacGJHdzZJQ05tWm1ZN0NpQWdJQ0FnSUgwS0NpQWdJQ0FnSUM1amJITXROU0I3Q2lBZ0lDQWdJQ0FnWm1sc2JEb2dJMlptWXpJd01Ec0tJQ0FnSUNBZ2ZRb2dJQ0FnUEM5emRIbHNaVDRLSUNBOEwyUmxabk1cdTAwMkJDaUFnUEdjXHUwMDJCQ2lBZ0lDQThjRzlzZVdkdmJpQmpiR0Z6Y3owaVkyeHpMVFVpSUhCdmFXNTBjejBpTnk0MU9DQXhOUzR4TVNBM0xqVTRJRGN1TkRJZ01TQTBMakl4SURFZ01URXVPVEVnTnk0MU9DQXhOUzR4TVNJdlBnb2dJQ0FnUEhCdmJIbG5iMjRnWTJ4aGMzTTlJbU5zY3kweUlpQndiMmx1ZEhNOUlqY3VOVGdnTVRVdU1URWdOeTQxT0NBM0xqUXlJREUwTGpFMUlEUXVNakVnTVRRdU1UVWdNVEV1T1RFZ055NDFPQ0F4TlM0eE1TSXZQZ29nSUNBZ1BIQnZiSGxuYjI0Z1kyeGhjM005SW1Oc2N5MHpJaUJ3YjJsdWRITTlJakVnTkM0eU1TQTNMalU0SURFZ01UUXVNVFVnTkM0eU1TQTNMalU0SURjdU5ESWdNU0EwTGpJeElpOFx1MDAyQkNpQWdJQ0E4Y0c5c2VXZHZiaUJqYkdGemN6MGlZMnh6TFRjaUlIQnZhVzUwY3owaU55NDFPQ0F4SURFMExqRTFJRFF1TWpFZ01UUXVNVFVnTVRFdU9URWdOeTQxT0NBeE5TNHhNU0F4SURFeExqa3hJREVnTkM0eU1TQTNMalU0SURFaUx6NEtJQ0E4TDJjXHUwMDJCQ2lBZ1BHY1x1MDAyQkNpQWdJQ0E4Y21WamRDQmpiR0Z6Y3owaVkyeHpMVEVpSUhnOUlqRXdJaUI1UFNJeE1TSWdkMmxrZEdnOUlqRXpJaUJvWldsbmFIUTlJakV5SWk4XHUwMDJCQ2lBZ0lDQThjbVZqZENCamJHRnpjejBpWTJ4ekxUUWlJSGc5SWpFeElpQjVQU0l4TVNJZ2QybGtkR2c5SWpFaUlHaGxhV2RvZEQwaU1USWlMejRLSUNBZ0lEeHlaV04wSUdOc1lYTnpQU0pqYkhNdE5DSWdlRDBpTVRRaUlIazlJakV4SWlCM2FXUjBhRDBpTVNJZ2FHVnBaMmgwUFNJeE1pSXZQZ29nSUNBZ1BISmxZM1FnWTJ4aGMzTTlJbU5zY3kwMElpQjRQU0l4TmlJZ2VUMGlNVEVpSUhkcFpIUm9QU0l5SWlCb1pXbG5hSFE5SWpFeUlpOFx1MDAyQkNpQWdJQ0E4Y21WamRDQmpiR0Z6Y3owaVkyeHpMVFFpSUhnOUlqSXdJaUI1UFNJeE1TSWdkMmxrZEdnOUlqSWlJR2hsYVdkb2REMGlNVElpTHo0S0lDQThMMmNcdTAwMkJDaUFnUEhCaGRHZ2dZMnhoYzNNOUltTnNjeTAySWlCa1BTSk5PU3d4T1hNdE5Dd3dMVFl0TWl3eUxUWXNNaTAySWk4XHUwMDJCQ2lBZ1BIQnZiSGxzYVc1bElHTnNZWE56UFNKamJITXROaUlnY0c5cGJuUnpQU0kwTGpVZ01UWWdPU0F4T1NBMExqVWdNaklpTHo0S1BDOXpkbWNcdTAwMkIiDQogICAgICB9DQogICAgfSwNCiAgICAiZ3JvdXAiOiAiRGF0YSIsDQogICAgImV4cG9zdXJlIjogImxldmVsMiIsDQogICAgImlucHV0cyI6IFsNCiAgICAgIHsNCiAgICAgICAgIm5hbWUiOiAiT0IiLA0KICAgICAgICAidHlwZSI6IHsNCiAgICAgICAgICAibmFtZSI6ICJTeXN0ZW0uT2JqZWN0Ig0KICAgICAgICB9LA0KICAgICAgICAicHJldHR5IjogIkdlb21ldHJ5IiwNCiAgICAgICAgImRlc2MiOiAicmhpbm9zY3JpcHRzeW50YXggZ2VvbWV0cnkiDQogICAgICB9DQogICAgXSwNCiAgICAib3V0cHV0cyI6IFsNCiAgICAgIHsNCiAgICAgICAgIm5hbWUiOiAiSk8iLA0KICAgICAgICAidHlwZSI6IHsNCiAgICAgICAgICAibmFtZSI6ICJTeXN0ZW0uT2JqZWN0Ig0KICAgICAgICB9LA0KICAgICAgICAic3RyaWN0IjogZmFsc2UsDQogICAgICAgICJwcmV0dHkiOiAiSlNPTiBPYmplY3QiLA0KICAgICAgICAiZGVzYyI6ICJyaGlub3NjcmlwdHN5bnRheCBnZW9tZXRyeSINCiAgICAgIH0NCiAgICBdDQogIH0NCn0=";
    readonly dynamic _script;

    public override Guid ComponentGuid { get; } = new Guid("20dc8abf-ea08-46ad-82fa-cd1efd8023e1");

    public override GH_Exposure Exposure { get; } = GH_Exposure.secondary;

    public override bool Obsolete { get; } = false;

    protected override SD.Bitmap Icon => _icon;

    public ProjectComponent_20dc8abf() : base(
        name: "Serialize Model Object",
        nickname: "Serialize Model Object",
        description: "",
        category: "Redback-Beta",
        subCategory: "Data"
        )
    {
      bool success = ProjectComponentPlugin.TryCreateScript(this, _scriptData, out _script);
      if (success)
      {
        ProjectComponentPlugin.TryCreateScriptIcon(_script, out _icon);
      }
      else
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Scripting platform is not ready.");
      }
    }

    protected override void RegisterInputParams(GH_InputParamManager _) { }

    protected override void RegisterOutputParams(GH_OutputParamManager _) { }

    protected override void BeforeSolveInstance()
    {
      if (_script is null) return;
      _script.BeforeSolve(this);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      if (_script is null) return;
      _script.Solve(this, DA);
    }

    protected override void AfterSolveInstance()
    {
      if (_script is null) return;
      _script.AfterSolve(this);
    }

    public override void RemovedFromDocument(GH_Document document)
    {
      ProjectComponentPlugin.DisposeScript(this, _script);
      base.RemovedFromDocument(document);
    }

    public override BoundingBox ClippingBox
    {
      get
      {
        if (_script is null) return BoundingBox.Empty;
        return _script.GetClipBox(this);
      }
    }

    public override void DrawViewportWires(IGH_PreviewArgs args)
    {
      if (_script is null) return;
      _script.DrawWires(this, args);
    }

    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
      if (_script is null) return;
      _script.DrawMeshes(this, args);
    }
  }
}
