using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using MsBox.Avalonia.Base;
using Nodify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ResultTableNodeVM : NodeVM
    {



        public ResultTableNodeVM(GraphEditorVM parent) : base(parent)
        {
            Title = Xloc.Get("__ResultTable_Title");
        }

        public ResultTableNodeVM(GraphEditorVM parent, bool generate)
            : this(parent)
        {
            if (generate)
            {
                var inputPin = new ConnectorVM(this, true);
                inputPin.ConnectorCollor = Colors.Gray;
                inputPin.IsAny = true;
                inputPin.Name = null;
                Input.Add(inputPin);
            }
        }


        public ResultDataTable TotalInput { get; } = new ResultDataTable(Xloc.Get("__ResultTable_TitleInput"), true);
        public ResultDataTable TotalOutput { get; } = new ResultDataTable(Xloc.Get("__ResultTable_TitleOutput"), true);
        public ResultDataTable TotalResources { get; } = new ResultDataTable(Xloc.Get("__ResultTable_TitleResources"), false);
        public ResultDataTable TotalRecipes { get; } = new ResultDataTable(Xloc.Get("__ResultTable_TitleRecipes"), false);

        [ObservableProperty]
        private Exception? error;

        private const int MAX_GENERATION = 100;
        private int generationCount;

        public override ConnectorVM GetReplacementFor(ConnectorVM self, ConnectorVM other)
        {
            if (self.IsInput)
            {
                var pin = Input.FirstOrDefault(m => m.Name == other.Name);
                if (pin != null)
                    return pin;

                pin = new ConnectorVM(this, other.Name, true);
                Input.Add(pin);
                return pin;
            }
            else
            {
                return base.GetReplacementFor(self, other);
            }
        }

        public void Rebuild()
        {


            TotalInput.Clear();
            TotalOutput.Clear();
            TotalResources.Clear();
            TotalRecipes.Clear();

            TotalRecipes.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            TotalRecipes.AddOrUpdateColumn("Number", Xloc.Get("__ResultTable_TitleRecipes_Number"));

            TotalInput.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            TotalInput.AddOrUpdateColumn("Input", Xloc.Get("__ResultTable_Table_Input") + " /" + TimeType.GetLocalizedShortValue());

            TotalOutput.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            TotalOutput.AddOrUpdateColumn("Output", Xloc.Get("__ResultTable_Table_Output") + " /" + TimeType.GetLocalizedShortValue());


            generationCount = 0;


            Error = null;


            try
            {

                List<NodeHelper> nodes = new List<NodeHelper>();
                var thisNode = AddNodeToList(nodes, this, 0);


                // first is total recipes, that table contains all parameters of machines and list of machines

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    if (node.Input.Sum(m => m.Connections.Count) == 0)
                    {
                        SetNodeGeneration(node, 1);
                    }
                }

                // push steps table
                TotalResources.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));

                for (int i = 1; i < generationCount; i++)
                {
                    TotalResources.AddOrUpdateColumn("Input" + i, Xloc.Get("__ResultTable_Table_Input") + "#" + i + "/" + TimeType.GetLocalizedShortValue());
                    TotalResources.AddOrUpdateColumn("Output" + i, Xloc.Get("__ResultTable_Table_Output") + "#" + i + "/" + TimeType.GetLocalizedShortValue());
                }

                TotalResources.AddOrUpdateColumn("Total", Xloc.Get("__ResultTable_Table_Total"));
                //Begin calculate

                foreach (var pinInput in thisNode.Input)
                {
                    double requestPerSecond = 0;
                    foreach (var pinOutput in pinInput.Connections)
                    {
                        requestPerSecond += pinOutput.Connector.ValuePerSecond;
                    }

                    if (!pinInput.Connector.IsSet)
                    {
                        pinInput.Connector.ValuePerSecond = requestPerSecond;
                    }
                    pinInput.Connector.TimeToCraft = 1;
                }

                CalculateRequest(thisNode);

                //End calculate

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    foreach (var pin in node.Input)
                    {

                        TotalResources.AddtToCellWithFindRow(pin, "Input" + node.Generation, pin.CurrentValue * TimeType.GetTimeInSeconds());
                        TotalResources.AddtToCellWithFindRow(pin, "Total", -pin.CurrentValue * TimeType.GetTimeInSeconds());
                    }

                    foreach (var pin in node.Output)
                    {

                        TotalResources.AddtToCellWithFindRow(pin, "Output" + node.Generation, pin.CurrentValue * TimeType.GetTimeInSeconds());
                        TotalResources.AddtToCellWithFindRow(pin, "Total", pin.CurrentValue * TimeType.GetTimeInSeconds());
                    }
                }

                FillInputOutputTables(TotalInput, TotalOutput, TotalResources);

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    if (node.Node is RecipeNodeVM recipeNode)
                    {
                        var row = new ResultTableRow();

                        var rowNameModel = new ResultTableCellName()
                        {
                            Icon = recipeNode.Icon,
                            IconBackground = recipeNode.BackgroundColor,
                            Name = recipeNode.Title
                        };

                        TotalRecipes.SetCell("Name", row, rowNameModel);

                        TotalRecipes.AddToCell("Number", row, node.Number);

                        foreach (var parameter in recipeNode.Variables)
                        {
                            if (parameter.Name != null)
                                TotalRecipes.AddToCell(parameter.Name, row, parameter.Value);
                        }
                        TotalRecipes.Rows.Add(row);
                    }
                }

            }
            catch (Exception ex)
            {
                Error = ex;
            }


            TotalResources.Container = new ResultDataTableContainer(TotalResources);// OnPropertyChanged(nameof(TotalResources));
            TotalInput.Container = new ResultDataTableContainer(TotalInput);//OnPropertyChanged(nameof(TotalInput));
            TotalOutput.Container = new ResultDataTableContainer(TotalOutput);//OnPropertyChanged(nameof(TotalOutput));
            TotalRecipes.Container = new ResultDataTableContainer(TotalRecipes);//OnPropertyChanged(nameof(TotalRecipes));
        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();


            model.Parameters["IsTotalInputVisibile"] = TotalInput.IsVisible ? "t" : "f";
            model.Parameters["IsTotalOutputVisibile"] = TotalOutput.IsVisible ? "t" : "f";
            model.Parameters["IsTotalRecipesVisibile"] = TotalRecipes.IsVisible ? "t" : "f";
            model.Parameters["IsTotalResourcesVisibile"] = TotalResources.IsVisible ? "t" : "f";

            return model;
        }

        public override void RestoreState(NodeModel model)
        {
            string? strValue;
            if (model.Parameters.TryGetValue("IsTotalInputVisibile", out strValue)) TotalInput.IsVisible = strValue == "t";
            if (model.Parameters.TryGetValue("IsTotalOutputVisibile", out strValue)) TotalOutput.IsVisible = strValue == "t";
            if (model.Parameters.TryGetValue("IsTotalRecipesVisibile", out strValue)) TotalRecipes.IsVisible = strValue == "t";
            if (model.Parameters.TryGetValue("IsTotalResourcesVisibile", out strValue)) TotalResources.IsVisible = strValue == "t";

            base.RestoreState(model);

            ConnectorVM? inputPin = null;
            foreach (var pin in Input)
            {
                if (pin.IsAny)
                {
                    inputPin = pin;
                    break;
                }
            }

            if (inputPin == null)
                Input.Insert(0, inputPin = new ConnectorVM(this, true));

            inputPin.ConnectorCollor = Colors.Gray;
            inputPin.IsAny = true;
            inputPin.Name = null;
        }

        private static void FillInputOutputTables(ResultDataTable totalInput, ResultDataTable totalOutput, ResultDataTable totalResources)
        {
            var column = totalResources.Columns.FirstOrDefault(m => m.Name == "Total");

            if (column == null)
                return;



            foreach (var row in totalResources.Rows)
            {
                string id = row.Id ?? "Unk";
                if (row.GetValue(column.Index) is ResultTableCellDouble cell && row.GetValue(0) is ResultTableCellName cellName)
                {
                    if (cell.Value < -0.000000000001)
                    {
                        var newRow = new ResultTableRow(id);
                        totalInput.SetCell("Name", newRow, cellName);
                        totalInput.AddToCell("Input", newRow, -cell.Value);
                        totalInput.Rows.Add(newRow);
                    }
                    else if (cell.Value > 0.000000000001)
                    {
                        var newRow = new ResultTableRow(id);
                        totalOutput.SetCell("Name", newRow, cellName);
                        totalOutput.AddToCell("Output", newRow, cell.Value);
                        totalOutput.Rows.Add(newRow);
                    }
                }
            }
        }

        private void CalculateRequest(NodeHelper node)
        {
            double requestPercentage;

            if (node.Output.Count == 0)
            {
                requestPercentage = 1;
            }
            else
            {
                requestPercentage = 0;

                foreach (var pinOut in node.Output)
                {
                    requestPercentage = Math.Max(requestPercentage, pinOut.Request / pinOut.Connector.ValuePerSecond);
                }

                node.Number = requestPercentage;

                foreach (var pinOut in node.Output)
                {
                    pinOut.CurrentValue = requestPercentage * pinOut.Connector.ValuePerSecond;
                }
            }

            foreach (var pinIn in node.Input)
            {
                pinIn.CurrentValue = requestPercentage * pinIn.Connector.ValuePerSecond;

                foreach (var pinOut in pinIn.Connections)
                {
                    pinOut.Request -= pinIn.Request;
                    pinOut.Request += pinIn.CurrentValue;
                }
                pinIn.Request = pinIn.CurrentValue;
            }

            foreach (var pinin in node.Input)
            {
                foreach (var pinOut in pinin.Connections)
                {
                    CalculateRequest(pinOut.Node);
                }
            }

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

        private NodeHelper AddNodeToList(List<NodeHelper> nodes, NodeVM node, int generationLimit)
        {
            if (generationLimit > MAX_GENERATION)
                throw new NotSupportedException("Loops is not supporeted");

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
                    if (pin.IsAny)
                        continue;

                    var pinHelper = new PinHelper(nodeHelper, pin);
                    AddToListConnections(nodes, pinHelper, pinHelper.Connector, generationLimit);
                    nodeHelper.Input.Add(pinHelper);
                }
            }
            return nodeHelper;
        }

        private void AddToListConnections(List<NodeHelper> nodes, PinHelper pin, ConnectorVM connector, int generationLimit)
        {
            if (generationLimit > MAX_GENERATION)
                throw new NotSupportedException("Loops is not supporeted");

            foreach (var connection in Parent.Connections)
            {
                if (connection.Input == connector)
                {
                    var node = connection.Output.Parent;


                    if (node is IntermediateNode)
                    {
                        foreach (var intermediateInputConnector in node.Input)
                        {
                            AddToListConnections(nodes, pin, intermediateInputConnector, generationLimit + 1);
                        }
                    }
                    else
                    {
                        var nodeHelper = AddNodeToList(nodes, node, generationLimit + 1);

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

    class NodeHelper
    {
        public NodeVM Node { get; }
        public List<PinHelper> Input { get; } = new List<PinHelper>();
        public List<PinHelper> Output { get; } = new List<PinHelper>();
        public int Generation { get; set; }
        public double Number { get; set; }

        public NodeHelper(NodeVM node)
        {
            this.Node = node;
        }
    }

    class PinHelper
    {
        public NodeHelper Node { get; }
        public ConnectorVM Connector { get; }


        public double CurrentValue { get; set; }

        public double Request { get; set; }

        public List<PinHelper> Connections { get; } = new List<PinHelper>();


        public PinHelper(NodeHelper node, ConnectorVM connector)
        {
            this.Node = node;
            this.Connector = connector;
        }
    }

    public class ResultDataTableContainer
    {
        public ResultDataTableContainer(ResultDataTable table)
        {
            this.Table = table;
        }

        public ResultDataTable Table { get; }
    }


    public partial class ResultDataTable : ViewModelBase
    {
        [ObservableProperty]
        private bool isVisible = true;

        [ObservableProperty]
        ResultDataTableContainer container;

        public string Title { get; }

        public ResultDataTable(string title, bool isVisible)
        {
            this.Title = title;
            container = new ResultDataTableContainer(this);
            IsVisible = isVisible;

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

        public void Clear()
        {
            Columns.Clear();
            Rows.Clear();

            OnPropertyChanged(nameof(Columns));
            OnPropertyChanged(nameof(Rows));
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
