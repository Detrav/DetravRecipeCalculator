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
        private int generationCount;

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

            var totalRecipes = new ResultDataTable(Xloc.Get("__ResultTable_TitleRecipes"), TotalRecipes?.IsVisible);
            var totalInput = new ResultDataTable(Xloc.Get("__ResultTable_TitleInput"), TotalInput?.IsVisible);
            var totalOutput = new ResultDataTable(Xloc.Get("__ResultTable_TitleOutput"), TotalOutput?.IsVisible);
            var totalResources = new ResultDataTable(Xloc.Get("__ResultTable_TitleResources"), TotalResources?.IsVisible);

            totalRecipes.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));

            totalInput.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            totalInput.AddOrUpdateColumn("Input", Xloc.Get("__ResultTable_Table_Input") + " /" + TimeType.GetLocalizedShortValue());

            totalOutput.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            totalOutput.AddOrUpdateColumn("Output", Xloc.Get("__ResultTable_Table_Output") + " /" + TimeType.GetLocalizedShortValue());


            generationCount = 0;


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

                        var rowNameModel = new ResultTableCellName()
                        {
                            Icon = recipeNode.Icon,
                            IconBackground = recipeNode.BackgroundColor,
                            Name = recipeNode.Title
                        };

                        totalRecipes.SetCell("Name", row, rowNameModel);

                        foreach (var parameter in recipeNode.Variables)
                        {
                            if (parameter.Name != null)
                                totalRecipes.AddToCell(parameter.Name, row, parameter.Value);
                        }
                        totalRecipes.Rows.Add(row);
                    }
                }


                // push steps table
                totalResources.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));

                for (int i = 1; i < generationCount; i++)
                {
                    totalResources.AddOrUpdateColumn("Input" + i, Xloc.Get("__ResultTable_Table_Input") + " #" + i + " /" + TimeType.GetLocalizedShortValue());
                    totalResources.AddOrUpdateColumn("Shortage" + i, Xloc.Get("__ResultTable_Table_Shortage") + " #" + i + " /" + TimeType.GetLocalizedShortValue());
                    totalResources.AddOrUpdateColumn("Output" + i, Xloc.Get("__ResultTable_Table_Output") + " #" + i + " /" + TimeType.GetLocalizedShortValue());
                    totalResources.AddOrUpdateColumn("Surplus" + i, Xloc.Get("__ResultTable_Table_Surplus") + " #" + i + " /" + TimeType.GetLocalizedShortValue());
                }

                // calculate all values with generation

                for (int i = 2; i < generationCount; i++)
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

                            if (request > 0 && pin.Connections.Count > 0)
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

                            pin.ValuePerSecondLeft = pin.Connector.ValuePerSecond;
                            pin.ValuePerSecondResult = percentage * pin.Connector.ValuePerSecond;

                            var request = pin.ValuePerSecondResult;

                            if (pin.Connections.Count == 0)
                            {
                                pin.ValuePerSecondLeft = 0;
                            }
                            else
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
                                pin.ValuePerSecondLeft -= pin.ValuePerSecondResult;
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

                //push all inputs

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    foreach (var pin in node.Input)
                    {
                        if (pin.Connections.Count == 0)
                        {
                            totalInput.AddtToCellWithFindRow(pin, "Input", pin.ValuePerSecondResult);
                            //totalResources.AddtToCellWithFindRow(pin, "Input", pin.ValuePerSecondResult);
                        }

                        totalResources.AddtToCellWithFindRow(pin, "Input" + node.Generation, pin.ValuePerSecondResult);
                        totalResources.AddtToCellWithFindRow(pin, "Shortage" + node.Generation, pin.ValuePerSecondLeft);
                    }

                    foreach (var pin in node.Output)
                    {
                        if (pin.Connections.Count == 0 || pin.Connections.All(m => m.Node == thisNode))
                        {
                            totalOutput.AddtToCellWithFindRow(pin, "Output", pin.ValuePerSecondResult);
                            //totalResources.AddtToCellWithFindRow(pin, "Output", pin.ValuePerSecondResult);
                        }

                        totalResources.AddtToCellWithFindRow(pin, "Output" + node.Generation, pin.ValuePerSecondResult);
                        totalResources.AddtToCellWithFindRow(pin, "Surplus" + node.Generation, pin.ValuePerSecondLeft);
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
                pin.ValuePerSecondResult = pin.Connector.ValuePerSecond;
                pin.ValuePerSecondLeft = 0;

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

            if (generation > generationCount)
            {
                generationCount = generation;
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

                foreach (var pin in node.Output)
                {
                    var pinHelper = new PinHelper(nodeHelper, pin);
                    nodeHelper.Output.Add(pinHelper);
                }

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

                            var outputPinHelper = nodeHelper.Output.FirstOrDefault(m => m.Connector == connection.Output);
                            if (outputPinHelper != null)
                            {
                                outputPinHelper.Connections.Add(pin);
                                pin.Connections.Add(outputPinHelper);
                            }
                        }
                    }
                }
            }

        }


    }

    class NodeHelper
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

    class PinHelper
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

    public partial class ResultDataTable : ViewModelBase
    {
        [ObservableProperty]
        private bool isVisible = true;


        public string Title { get; }

        public ResultDataTable(string title, bool? isVisible)
        {
            this.Title = title;

            if (isVisible.HasValue)
            {
                IsVisible = isVisible.Value;
            }
        }

        public List<ResultTableColumn> Columns { get; } = new List<ResultTableColumn>();
        public List<ResultTableRow> Rows { get; } = new List<ResultTableRow>();

        public bool ExistsColumn(string columnName)
        {
            return Columns.Any(m => m.Name == columnName);
        }

        public void AddOrUpdateColumn(string name, string displayName)
        {
            var c = GetColumnByName(name);
            c.DisplayName = displayName;
        }

        private ResultTableColumn GetColumnByName(string name)
        {
            var column = Columns.FirstOrDefault(m => m.Name == name);
            if (column == null)
            {
                int index = Columns.Count;
                Columns.Add(column = new ResultTableColumn(name, index));
            }

            return column;
        }

        public void SetCell(string columnName, ResultTableRow row, ResultTableCell value)
        {
            var column = GetColumnByName(columnName);

            row.Set(column.Index, value);
        }

        public void AddToCell(string columnName, ResultTableRow row, double value)
        {
            var column = GetColumnByName(columnName);

            row.Add(column.Index, value);
        }

        internal void AddtToCellWithFindRow(PinHelper pin, string columnName, double value)
        {
            var row = Rows.FirstOrDefault(m => m.Id == pin.Connector.Name);

            if (row == null)
            {
                row = new ResultTableRow(pin.Connector.Name);
                var nameModel = new ResultTableCellName();
                nameModel.Name = pin.Connector.Name;
                nameModel.Icon = pin.Connector.Icon;
                nameModel.IconBackground = pin.Connector.BackgroundColor;
                SetCell("Name", row, nameModel);
                Rows.Add(row);
            }

            AddToCell(columnName, row, value);
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
        public List<ResultTableCell?> Cells { get; } = new List<ResultTableCell?>();
        public string? Id { get; }

        public ResultTableRow(string? id = null)
        {
            this.Id = id;
        }

        public void Set(int index, ResultTableCell value)
        {
            while (index >= Cells.Count)
            {
                Cells.Add(null);
            }

            Cells[index] = value;
        }

        public void Add(int index, double value)
        {
            while (index >= Cells.Count)
            {
                Cells.Add(null);
            }

            if (Cells[index] is ResultTableCellDouble cell)
            {
                cell.Value += value;
            }
            else
            {
                Cells[index] = new ResultTableCellDouble()
                {
                    Value = value,
                };
            }
        }

        public ResultTableCell? GetValue(int index)
        {
            if (index < Cells.Count)
                return Cells[index];
            return null;
        }

    }

    public class ResultTableCellName : ResultTableCell
    {
        public Color IconBackground { get; set; }

        public string? Name { get; set; }

        public byte[]? Icon { get; set; }
    }

    public class ResultTableCellDouble : ResultTableCell
    {
        public double Value { get; set; }
        public string ValueString => ConnectorVM.GetFormated(Value);
    }


    public abstract class ResultTableCell
    {

    }
}
