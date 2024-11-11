using DetravRecipeCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DetravRecipeCalculator.Utils
{
    public class GraphBuilder
    {
        private IList<NodeVM> nodes;
        private GraphEditorVM graph;
        private readonly List<NodeVM> latestNodes = new List<NodeVM>();

        public GraphBuilder(GraphEditorVM graphEditorVM, IList<NodeVM> nodes)
        {
            this.nodes = nodes;
            this.graph = graphEditorVM;


        }

        public void Build()
        {
            graph.Inputs.Clear();
            graph.Outputs.Clear();

            latestNodes.Clear();
            foreach (var node in nodes)
            {
                if (node.Output.Sum(m => m.Connections.Count) == 0)
                {
                    latestNodes.Add(node);
                }

                node.Error = null;
                ResetTempVariables(node);
                node.UpdateExpressions();
            }

            foreach (var node in latestNodes)
            {
                try
                {
                    LoopProtectBackward(node, Array.Empty<NodeVM>());

                }
                catch (Exception ex)
                {
                    node.Error = ex;
                }
            }

            foreach (var node in latestNodes)
            {
                
                node.RequestResources();
            }

            foreach (var node in latestNodes)
            {
                node.Build();
            }

           

            foreach (var node in nodes)
            {
                foreach (var item in node.Input)
                {
                    if (item.Connection == null && !string.IsNullOrWhiteSpace(item.Name))
                    {
                        graph.Inputs.TryGetValue(item.Name, out var result);
                        result += item.TempCurrentValue;
                        graph.Inputs[item.Name] = result;
                    }
                }

                foreach (var item in node.Output)
                {
                    if ((item.Connections.Count == 0 || item.Connections.All(m => m.Parent is ResultTableNodeVM)) && !string.IsNullOrWhiteSpace(item.Name))
                    {
                        graph.Outputs.TryGetValue(item.Name, out var result);
                        result += item.TempCurrentValue;
                        graph.Outputs[item.Name] = result;
                    }
                }
            }
        }

        private void ResetTempVariables(NodeVM node)
        {
            node.Number = 1;


            foreach (var pin in node.Input)
            {
                pin.TempCurrentValue = 0;
                pin.TempRequest = 0;
            }

            foreach (var pin in node.Output)
            {
                pin.TempCurrentValue = 0;
                pin.TempRequest = 0;
            }
        }

        private void LoopProtectBackward(NodeVM node, IEnumerable<NodeVM> loopProtect)
        {
            if (loopProtect.Contains(node)) throw new NotSupportedException("Loops is not supported!");

            loopProtect = loopProtect.Append(node);

            foreach (var pin in node.Input)
            {
                if (pin.Connection != null)
                    LoopProtectBackward(pin.Connection.Parent, loopProtect);
            }
        }

    }
}
