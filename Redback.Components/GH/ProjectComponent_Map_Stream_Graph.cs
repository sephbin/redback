using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Redback.Components.Grasshopper;

namespace Redback.Components.GH
{
    public class MapStreamGraphComponent : RedbackGHBase
    {
        public MapStreamGraphComponent() : base("Map Stream Graph", "Map Stream Graph",
            "Maps the upstream GH component graph as JSON", "7-GH") { }

        public override Guid ComponentGuid => new Guid("17b83380-9bf2-466d-98ec-956107a0806e");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("D", "D", "Data input to trace upstream graph", GH_ParamAccess.tree);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("D", "D", "Pass-through data", GH_ParamAccess.item);
            p.AddTextParameter("S", "S", "Stream graph as JSON", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var nodes = new Dictionary<string, JsonObject>();
            var edges = new List<JsonObject>();
            var visited = new HashSet<string>();

            var result = SerializeComponent(this, nodes, edges, visited, null, 0);
            DA.SetData(1, result.ToJsonString());
        }

        private static JsonObject SerializeComponent(
            IGH_DocumentObject component,
            Dictionary<string, JsonObject> nodes,
            List<JsonObject> edges,
            HashSet<string> visited,
            IGH_Param parent,
            int layer)
        {
            if (component == null) return new JsonObject();
            string guid = component.InstanceGuid.ToString();
            if (!visited.Add(guid)) return new JsonObject();

            string nickName = component.NickName ?? "";
            var node = new JsonObject();
            node["guid"] = guid;
            node["name"] = component.Name;
            node["nickname"] = nickName.Split('|')[0];
            node["description"] = component.Description;
            node["inputs"] = new JsonObject();

            bool willRecurse = true;
            var nickParts = nickName.Split('|');
            var variables = nickParts.Length > 1 ? nickParts[1].Split(' ').ToList() : new List<string>();

            if (variables.Contains("end"))
            {
                willRecurse = false;
                var values = new List<string>();
                if (component is IGH_Param endParam)
                    foreach (var goo in endParam.VolatileData.AllData(true))
                        values.Add(goo?.ScriptVariable()?.ToString());

                if (values.Count == 1)
                    node["value"] = JsonValue.Create(values[0]);
                else
                    node["value"] = new JsonArray(values.Select(v => JsonValue.Create(v)).Cast<JsonNode>().ToArray());
            }

            if (component is GH_NumberSlider slider)
            {
                var nickVal = node["nickname"]?.GetValue<string>();
                if (string.IsNullOrEmpty(nickVal) && parent != null)
                    node["nickname"] = parent.Name;
                try { node["value"] = JsonValue.Create((double)slider.CurrentValue); }
                catch { }
            }
            else if (component is GH_Panel panel)
            {
                try { node["value"] = JsonValue.Create(panel.UserText); }
                catch { }
            }

            if (component is IGH_Param paramComp && !(component is GH_Component))
            {
                foreach (var source in paramComp.Sources)
                {
                    var attr = source?.Attributes;
                    if (attr?.GetTopLevel?.DocObject == null) continue;
                    var upstream = attr.GetTopLevel.DocObject;
                    string fromGuid = upstream.InstanceGuid.ToString();
                    var edge = new JsonObject();
                    edge["from_guid"] = fromGuid;
                    edge["to_guid"] = guid;
                    edge["to_param_name"] = paramComp.Name;
                    edges.Add(edge);
                    if (willRecurse)
                        SerializeComponent(upstream, nodes, edges, visited, paramComp, layer + 1);
                }
            }
            else if (component is GH_Component ghComp)
            {
                try
                {
                    var inputsJson = node["inputs"].AsObject();
                    foreach (var inputParam in ghComp.Params.Input)
                    {
                        foreach (var source in inputParam.Sources)
                        {
                            inputsJson[inputParam.Name] = new JsonObject
                            {
                                ["name"] = inputParam.Name,
                                ["nickname"] = inputParam.NickName
                            };
                            var attr = source?.Attributes;
                            if (attr?.GetTopLevel?.DocObject == null) continue;
                            var upstream = attr.GetTopLevel.DocObject;
                            string fromGuid = upstream.InstanceGuid.ToString();
                            var edge = new JsonObject();
                            edge["from_guid"] = fromGuid;
                            edge["to_guid"] = guid;
                            edge["to_param_name"] = inputParam.Name;
                            edges.Add(edge);
                            if (willRecurse)
                                SerializeComponent(upstream, nodes, edges, visited, inputParam, layer + 1);
                        }
                    }
                }
                catch { }
            }

            nodes[guid] = node;

            if (layer == 0)
            {
                nodes.Remove(guid);
                var nodeList = nodes.Values.ToList();
                var guidIndex = nodeList.Select(n => n["guid"]?.GetValue<string>()).ToList();

                var filteredEdges = edges.Where(e => e["to_guid"]?.GetValue<string>() != guid).ToList();
                var validEdges = new List<JsonObject>();
                foreach (var edge in filteredEdges)
                {
                    int fromIdx = guidIndex.IndexOf(edge["from_guid"]?.GetValue<string>());
                    int toIdx = guidIndex.IndexOf(edge["to_guid"]?.GetValue<string>());
                    if (fromIdx < 0 || toIdx < 0) continue;
                    validEdges.Add(new JsonObject
                    {
                        ["from_index"] = fromIdx,
                        ["to_index"] = toIdx,
                        ["to_param_name"] = edge["to_param_name"]?.GetValue<string>()
                    });
                }

                foreach (var n in nodeList) n.Remove("guid");

                return new JsonObject
                {
                    ["nodes"] = new JsonArray(nodeList.Select(n => JsonNode.Parse(n.ToJsonString())).Cast<JsonNode>().ToArray()),
                    ["edges"] = new JsonArray(validEdges.Select(e => (JsonNode)e).ToArray())
                };
            }

            return new JsonObject();
        }
    }
}
