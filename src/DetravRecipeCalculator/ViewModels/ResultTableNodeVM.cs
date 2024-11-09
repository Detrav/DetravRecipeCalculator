using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Utils;
using Nodify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ResultTableNodeVM : NodeVM
    {

        [ObservableProperty]
        private ConnectorVM? inputPin;
        public ResultTableNodeVM(GraphEditorVM parent) : base(parent)
        {
            Title = Xloc.Get("__ResultTable_Title");
        }

        [ObservableProperty]
        private ResultDataTable? totalInput;
        [ObservableProperty]
        private ResultDataTable? totalOutput;
        [ObservableProperty]
        private ResultDataTable? totalResources;
        [ObservableProperty]
        private ResultDataTable? totalRecipes;

        [ObservableProperty]
        private Exception? error;

        private const int MAX_GENERATION = 100;

        public override void RefreshValues(RecipeVM? recipe = null)
        {
            base.RefreshValues(recipe);

            if (Input.Count < 1)
                Input.Add(new ConnectorVM());

            InputPin = Input[0];

            InputPin.IsInput = true;
            InputPin.ConnectorCollor = Avalonia.Media.Colors.Gray;
            InputPin.IsAny = true;
            InputPin.Name = null;
        }

        public void Rebuild()
        {
            List<NodeHelper> nodes = new List<NodeHelper>();
            var thisNode = AddNodeToList(nodes, this);
            List<NodeHelper> firstNodes = new List<NodeHelper>();

            var totalRecipes = new ResultDataTable();
            var totalInput = new ResultDataTable();
            var totalOutput = new ResultDataTable();
            var totalResources = new ResultDataTable();

            totalRecipes.AddOrUpdateColumn("Name", "Name");
            totalInput.AddOrUpdateColumn("Name", "Name");
            totalOutput.AddOrUpdateColumn("Name", "Name");
            totalResources.AddOrUpdateColumn("Name", "Name");

            Error = null;


            try
            {

                // first is total recipes, that table contains all parameters of machines and list of machines

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    if (node.Input.Sum(m => m.Connections.Count) == 0)
                    {
                        firstNodes.Add(node);
                        SetupFirstNode(node);
                    }

                    if (node.Node is RecipeNodeVM recipeNode)
                    {
                        var row = new ResultTableRow();

                        RowNameVM rowNameModel = new RowNameVM()
                        {
                            Icon = recipeNode.Icon,
                            IconBackground = recipeNode.BackgroundColor,
                            Name = recipeNode.Title
                        };

                        totalRecipes.SetCell("Name", row, rowNameModel, null);

                        foreach (var parameter in recipeNode.Variables)
                        {
                            if (parameter.Name != null)
                                totalRecipes.AddToCell(parameter.Name, row, parameter.Value, null);
                        }
                        totalRecipes.Rows.Add(row);
                    }
                }

                // calculate all values with generation

                for (int i = 2; i < MAX_GENERATION; i++)
                {
                    bool stop = true;
                    foreach (var node in nodes.Where(m => m.Generation == i))
                    {
                        stop = false;
                        double percentage = 1;

                        foreach (var pin in node.Input)
                        {
                            if (node == thisNode) continue;

                            var request = pin.Connector.ValuePerSecond;

                            if (request > 0)
                            {
                                foreach (var connection in pin.Connections)
                                {
                                    request -= connection.ValuePerSecondResult;
                                    if (request <= 0)
                                        break;
                                }

                                if (request > 0)
                                {
                                    percentage = Math.Min(percentage, (pin.Connector.ValuePerSecond - request) / pin.Connector.ValuePerSecond);
                                }
                            }
                        }

                        foreach (var pin in node.Input)
                        {
                            if (node == thisNode) continue;

                            pin.ValuePerSecondLeft = pin.ValuePerSecondResult = percentage * pin.Connector.ValuePerSecond;
                            var request = pin.ValuePerSecondResult;

                            if (request > 0)
                            {

                                foreach (var connection in pin.Connections)
                                {
                                    if (request > connection.ValuePerSecondLeft)
                                    {
                                        connection.ValuePerSecondLeft = 0;
                                        request -= connection.ValuePerSecondLeft;
                                    }
                                    else
                                    {
                                        connection.ValuePerSecondLeft -= request;
                                        break;
                                    }
                                }
                            }
                        }

                        foreach (var pin in node.Output)
                        {
                            pin.ValuePerSecondLeft = pin.ValuePerSecondResult = percentage * pin.Connector.ValuePerSecond;
                        }
                    }
                    if (stop)
                        break;
                }

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    foreach (var pin in node.Input)
                    {
                        if (pin.Connections.Count == 0)
                        {
                            var row = totalInput.Rows.FirstOrDefault(m => m.Id == pin.Connector.Name);

                            if (row == null)
                            {
                                row = new ResultTableRow(pin.Connector.Name);
                                var nameModel = new RowNameVM();
                                nameModel.Name = pin.Connector.Name;
                                nameModel.Icon = pin.Connector.Icon;
                                nameModel.IconBackground = pin.Connector.BackgroundColor;
                                totalRecipes.SetCell("Name", row, nameModel, null);
                                totalInput.Rows.Add(row);
                            }

                            totalInput.AddToCell("Input", row, pin.ValuePerSecondResult, () => "Input " + pin.Connector.GetUnit());

                        }
                    }
                }

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    foreach (var pin in node.Output)
                    {
                        if (pin.Connections.Count == 0 || pin.Connections.All(m => m.Node == thisNode))
                        {
                            var row = totalOutput.Rows.FirstOrDefault(m => m.Id == pin.Connector.Name);

                            if (row == null)
                            {
                                row = new ResultTableRow(pin.Connector.Name);
                                var nameModel = new RowNameVM();
                                nameModel.Name = pin.Connector.Name;
                                nameModel.Icon = pin.Connector.Icon;
                                nameModel.IconBackground = pin.Connector.BackgroundColor;
                                totalRecipes.SetCell("Name", row, nameModel, null);
                                totalOutput.Rows.Add(row);
                            }

                            totalOutput.AddToCell("Output", row, pin.ValuePerSecondResult, () => "Output " + pin.Connector.GetUnit());

                        }
                    }
                }

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    foreach (var pin in node.Input)
                    {

                        var row = totalResources.Rows.FirstOrDefault(m => m.Id == pin.Connector.Name);

                        if (row == null)
                        {
                            row = new ResultTableRow(pin.Connector.Name);
                            var nameModel = new RowNameVM();
                            nameModel.Name = pin.Connector.Name;
                            nameModel.Icon = pin.Connector.Icon;
                            nameModel.IconBackground = pin.Connector.BackgroundColor;
                            totalRecipes.SetCell("Name", row, nameModel, null);
                            totalResources.Rows.Add(row);
                        }

                        totalResources.AddToCell("Input", row, pin.ValuePerSecondResult, () => "Input " + pin.Connector.GetUnit());
                        totalResources.AddToCell("Shortage", row, pin.ValuePerSecondLeft, () => "Shortage " + pin.Connector.GetUnit());
                    }

                    foreach (var pin in node.Output)
                    {
                        if (node == thisNode) continue;


                        var row = totalResources.Rows.FirstOrDefault(m => m.Id == pin.Connector.Name);

                        if (row == null)
                        {
                            row = new ResultTableRow(pin.Connector.Name);
                            var nameModel = new RowNameVM();
                            nameModel.Name = pin.Connector.Name;
                            nameModel.Icon = pin.Connector.Icon;
                            nameModel.IconBackground = pin.Connector.BackgroundColor;
                            totalRecipes.SetCell("Name", row, nameModel, null);
                            totalResources.Rows.Add(row);
                        }

                        totalResources.AddToCell("Output", row, pin.ValuePerSecondResult, () => "Output " + pin.Connector.GetUnit());
                        totalResources.AddToCell("Surplus", row, pin.ValuePerSecondLeft, () => "Surplus " + pin.Connector.GetUnit());
                    }
                }

            }
            catch (Exception ex)
            {
                Error = ex;
            }


            TotalResources = totalResources;
            TotalInput = totalInput;
            TotalOutput = totalOutput;
            TotalRecipes = totalRecipes;
        }



        private void SetupFirstNode(NodeHelper node)
        {
            foreach (var pin in node.Input)
            {
                pin.ValuePerSecondLeft = pin.ValuePerSecondResult = pin.Connector.ValuePerSecond;

            }

            foreach (var pin in node.Output)
            {
                pin.ValuePerSecondLeft = pin.ValuePerSecondResult = pin.Connector.ValuePerSecond;

            }

            SetNodeGeneration(node, 1);
        }

        private void SetNodeGeneration(NodeHelper node, int generation)
        {
            if (generation > MAX_GENERATION)
            {
                throw new NotSupportedException("Supported only " + MAX_GENERATION + " generations!");
            }

            if (FindLoop(node, node))
            {
                throw new NotSupportedException("Loops is not supported!");
            }

            if (generation > node.Generation)
            {
                node.Generation = generation;

                foreach (var item in node.Output)
                {
                    foreach (var connection in item.Connections)
                    {
                        SetNodeGeneration(connection.Node, generation + 1);
                    }
                }
            }
        }

        private bool FindLoop(NodeHelper node, NodeHelper current)
        {
            foreach (var pin in current.Input)
            {
                foreach (var connection in pin.Connections)
                {
                    if (node == connection.Node || FindLoop(node, connection.Node))
                        return true;
                }
            }
            return false;
        }

        private NodeHelper AddNodeToList(List<NodeHelper> nodes, NodeVM node)
        {
            var nodeHelper = nodes.FirstOrDefault(m => m.Node == node);
            if (nodeHelper == null)
            {
                nodeHelper = new NodeHelper(node);
                nodes.Add(nodeHelper);

                foreach (var pin in node.Input)
                {
                    var pinHelper = new PinHelper(nodeHelper, pin);
                    AddToList(nodes, pinHelper);
                    nodeHelper.Input.Add(pinHelper);
                }
            }
            return nodeHelper;
        }

        private void AddToList(List<NodeHelper> nodes, PinHelper pin)
        {
            foreach (var connection in Parent.Connections)
            {
                if (connection.Input == pin.Connector)
                {
                    foreach (var node in Parent.Nodes)
                    {
                        if (node.Output.Any(m => m == connection.Output))
                        {
                            var nodeHelper = AddNodeToList(nodes, node);
                            var outputPinHelper = new PinHelper(nodeHelper, connection.Output);
                            outputPinHelper.Connections.Add(pin);
                            pin.Connections.Add(outputPinHelper);
                            nodeHelper.Output.Add(outputPinHelper);
                        }
                    }
                }
            }

        }

        private class NodeHelper
        {
            public NodeVM Node { get; }
            public List<PinHelper> Input { get; } = new List<PinHelper>();
            public List<PinHelper> Output { get; } = new List<PinHelper>();
            public int Generation { get; set; }

            public NodeHelper(NodeVM node)
            {
                this.Node = node;
            }
        }

        private class PinHelper
        {
            public NodeHelper Node { get; }
            public ConnectorVM Connector { get; }

            /// <summary>
            /// The value that real consumes or produces
            /// </summary>
            public double ValuePerSecondResult { get; set; }
            /// <summary>
            /// The value that left from produces
            /// </summary>
            public double ValuePerSecondLeft { get; set; }

            public List<PinHelper> Connections { get; } = new List<PinHelper>();

            public PinHelper(NodeHelper node, ConnectorVM connector)
            {
                this.Node = node;
                this.Connector = connector;
            }
        }
    }

    public class RowNameVM
    {

        public Color IconBackground { get; set; }

        public string? Name { get; set; }

        public byte[]? Icon { get; set; }
    }

    public class ResultDataTable
    {
        public List<ResultTableColumn> Columns { get; } = new List<ResultTableColumn>();
        public List<ResultTableRow> Rows { get; } = new List<ResultTableRow>();

        public bool ExistsColumn(string columnName)
        {
            return Columns.Any(m => m.Name == columnName);
        }

        public void AddOrUpdateColumn(string name, string displayName)
        {
            GetColumnByName(name, () => displayName);
        }

        public void AddOrUpdateColumn(string name, Func<string> displayNameFactory)
        {
            GetColumnByName(name, displayNameFactory);
        }

        private ResultTableColumn GetColumnByName(string name, Func<string>? displayNameFactory)
        {
            var column = Columns.FirstOrDefault(m => m.Name == name);
            if (column == null)
            {
                int index = Columns.Count;
                Columns.Add(column = new ResultTableColumn(name, index));

                if (displayNameFactory != null)
                {
                    column.DisplayName = displayNameFactory();
                }
            }

            return column;
        }

        public void SetCell(string columnName, ResultTableRow row, object value, Func<string>? displayNameFactory)
        {
            var column = GetColumnByName(columnName, displayNameFactory);

            row.Set(column.Index, value);
        }

        public void AddToCell(string columnName, ResultTableRow row, double value, Func<string>? displayNameFactory)
        {
            var column = GetColumnByName(columnName, displayNameFactory);

            row.Add(column.Index, value);
        }
    }

    public class ResultTableColumn
    {
        public string Name { get; }
        public string DisplayName { get; set; }
        public int Index { get; }

        public int ColumnType { get; set; }

        public ResultTableColumn(string name, int index)
        {
            Name = name;
            DisplayName = name;
            Index = index;
        }

    }

    public class ResultTableRow
    {
        public List<ResultTableItemParameter> Parameters { get; } = new List<ResultTableItemParameter>();
        public string? Id { get; }

        public ResultTableRow(string? id = null)
        {
            this.Id = id;
        }

        public void Set(int index, object value)
        {
            while (index >= Parameters.Count)
            {
                Parameters.Add(new ResultTableItemParameter());
            }

            Parameters[index].Value = value;
        }

        public void Add(int index, double value)
        {
            while (index >= Parameters.Count)
            {
                Parameters.Add(new ResultTableItemParameter());
            }

            if (Parameters[index].Value is double doubleValue)
            {
                Parameters[index].Value = doubleValue + value;
            }
            else
            {
                Parameters[index].Value = value;
            }
        }

        public object? GetValue(int index)
        {
            if (index < Parameters.Count)
                return Parameters[index];
            return null;
        }

    }

    public partial class ResultTableItemParameter
    {
        public object? Value { get; set; }

        public ResultTableItemParameter()
        {

        }
    }
}
