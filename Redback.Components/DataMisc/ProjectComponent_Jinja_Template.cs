using System;
using Grasshopper.Kernel;
using Redback.Components.Core.Jinja;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataMisc
{
    public class JinjaTemplateComponent : RedbackGHBase
    {
        public JinjaTemplateComponent() : base("Render Template", "Template",
            "Renders a Jinja2 template string using a JSON object as the context.\n" +
            "Supports {{ variables }}, filters (e.g. {{ name|upper }}), {% if %}, {% for %}, {% set %}, {% macro %}, " +
            "comments {# #} and whitespace control {%- -%}.",
            "2-Data - Misc") { }

        public override Guid ComponentGuid => new Guid("72fc32cf-0615-45ae-9278-c2f2c3273bdb");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Template", "T", "Jinja2 template text", GH_ParamAccess.item);
            p.AddTextParameter("Context", "C", "JSON object whose keys become template variables", GH_ParamAccess.item);
            p[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Result", "R", "Rendered text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string template = null;
            string context = null;
            if (!DA.GetData(0, ref template)) return;
            DA.GetData(1, ref context);

            try
            {
                DA.SetData(0, JinjaTemplate.Render(template, context));
            }
            catch (TemplateError e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
        }
    }
}
