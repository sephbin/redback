using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Redback.Components.Core.Jinja
{
    /// <summary>
    /// A self-contained Jinja2 template engine (no external dependencies).
    ///
    /// Supported: {{ expressions }}, {% if/elif/else %}, {% for ... in ... if ... %} with loop.* and {% else %},
    /// {% set %} (incl. tuple unpacking, block set and namespace attributes), {% macro %} / {% call %},
    /// {% filter %}, {% with %}, {% raw %}, {% break %}/{% continue %}, comments, whitespace control (-),
    /// Python-style operators/literals/slicing, string &amp; dict methods, and the standard Jinja filters and tests.
    ///
    /// Not supported (no template loader): include, extends, import, from.
    /// Autoescaping is off, matching Jinja's default Environment.
    /// </summary>
    public sealed class JinjaTemplate
    {
        private readonly List<Node> _body;

        public JinjaTemplate(string source)
        {
            _body = new Parser(Lexer.Split(source)).ParseTemplate();
        }

        /// <summary>Render with a JSON object string as the context (null/empty = no variables).</summary>
        public string RenderJson(string contextJson)
        {
            var vars = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(contextJson))
            {
                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(contextJson, new JsonDocumentOptions
                    {
                        AllowTrailingCommas = true,
                        CommentHandling = JsonCommentHandling.Skip,
                    });
                }
                catch (JsonException ex)
                {
                    throw new TemplateError("Context is not valid JSON: " + ex.Message);
                }
                using (doc)
                {
                    if (doc.RootElement.ValueKind != JsonValueKind.Object)
                        throw new TemplateError("Context must be a JSON object, e.g. {\"name\": \"value\"}");
                    vars = (Dictionary<string, object>)Py.FromJson(doc.RootElement);
                }
            }
            return Render(vars);
        }

        internal string Render(Dictionary<string, object> vars)
        {
            var ctx = new Context(new List<Dictionary<string, object>> { vars });
            var sb = new StringBuilder();
            try
            {
                Node.RenderAll(_body, ctx, sb);
            }
            catch (BreakSignal) { throw new TemplateError("'break' used outside of a for-loop"); }
            catch (ContinueSignal) { throw new TemplateError("'continue' used outside of a for-loop"); }
            return sb.ToString();
        }

        public static string Render(string template, string contextJson) => new JinjaTemplate(template).RenderJson(contextJson);
    }
}
