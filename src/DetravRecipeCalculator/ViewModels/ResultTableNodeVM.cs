using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ResultTableNodeVM : NodeVM
    {



        public ResultTableNodeVM(GraphEditorVM parent) : base(parent)
        {
            Title = Xloc.Get("__ResultTable_Title");
            TimeToCraft = 1;
        }

        public ResultTableNodeVM(GraphEditorVM parent, bool generate)
            : this(parent)
        {
            TimeToCraft = 1;
            if (generate)
            {
                var inputPin = new ConnectorInVM(this);
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


        public override ConnectorInVM GetReplacementFor(ConnectorInVM self, ConnectorOutVM other)
        {
            var pin = Input.FirstOrDefault(m => m.Name == other.Name);
            if (pin != null)
                return pin;

            pin = new ConnectorInVM(this, other.Name);
            Input.Add(pin);
            return pin;
        }


        public override void Build()
        {

            TotalInput.Clear();
            TotalOutput.Clear();
            TotalResources.Clear();
            TotalRecipes.Clear();

            TotalRecipes.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            TotalRecipes.AddOrUpdateColumn("Number", Xloc.Get("__ResultTable_TitleRecipes_Number"));

            TotalInput.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            TotalInput.AddOrUpdateColumn("Input", Xloc.Get("__ResultTable_Table_Input") + " /" + Parent.TimeType.GetLocalizedShortValue());

            TotalOutput.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));
            TotalOutput.AddOrUpdateColumn("Output", Xloc.Get("__ResultTable_Table_Output") + " /" + Parent.TimeType.GetLocalizedShortValue());


            generationCount = 0;


            Error = null;


            try
            {

                List<NodeVM> nodes = new List<NodeVM>();
                var thisNode = AddNodeToList(nodes, this);

                LoopProtectBackward(this, Array.Empty<NodeVM>());

                //reset nodes

                foreach (var node in nodes)
                {
                    node.Number = 1;
                    node.Generation = 0;


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


                // first is total recipes, that table contains all parameters of machines and list of machines

                foreach (var node in nodes)
                {
                    if (node == thisNode) continue;

                    //LoopProtectForward(node, Array.Empty<NodeVM>());

                    if (node.Input.All(m => m.Connection == null))
                    {
                        SetNodeGeneration(node, 1, nodes);
                    }
                }

                // push steps table
                TotalResources.AddOrUpdateColumn("Name", Xloc.Get("__ResultTable_Table_Name"));

                for (int i = 1; i < generationCount; i++)
                {
                    TotalResources.AddOrUpdateColumn("Input" + i, Xloc.Get("__ResultTable_Table_Input") + "#" + i + "/" + Parent.TimeType.GetLocalizedShortValue());
                    TotalResources.AddOrUpdateColumn("Output" + i, Xloc.Get("__ResultTable_Table_Output") + "#" + i + "/" + Parent.TimeType.GetLocalizedShortValue());
                }

                TotalResources.AddOrUpdateColumn("Total", Xloc.Get("__ResultTable_Table_Total"));
                //Begin calculate

                foreach (var pin in Input)
                {
                    if (!pin.IsSet)
                    {

                        if (pin.Connection == null)
                        {
                            pin.Value = 0;

                        }
                        else
                        {
                            if (pin.Connection.Parent is IntermediateNode intermediate)
                            {
                                pin.Value = intermediate.ProviderRequests();
                            }
                            else
                            {
                                pin.Value = pin.Connection.GetValuePerSecond();
                            }
                        }
                    }
                }

                base.Build();

                //End calculate

                foreach (var node in nodes)
                {
                    if (node == thisNode || node is IntermediateNode)
                        continue;

                    foreach (var pin in node.Input)
                    {

                        TotalResources.AddtToCellWithFindRow(pin, "Input" + node.Generation, pin.TempCurrentValue * Parent.TimeType.GetTimeInSeconds());
                        TotalResources.AddtToCellWithFindRow(pin, "Total", -pin.TempCurrentValue * Parent.TimeType.GetTimeInSeconds());
                    }

                    foreach (var pin in node.Output)
                    {

                        TotalResources.AddtToCellWithFindRow(pin, "Output" + node.Generation, pin.TempCurrentValue * Parent.TimeType.GetTimeInSeconds());
                        TotalResources.AddtToCellWithFindRow(pin, "Total", pin.TempCurrentValue * Parent.TimeType.GetTimeInSeconds());
                    }
                }

                FillInputOutputTables(TotalInput, TotalOutput, TotalResources);

                foreach (var node in nodes)
                {
                    if (node == thisNode || node is IntermediateNode)
                        continue;

                    if (node is RecipeNodeVM recipeNode)
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

            ConnectorInVM? inputPin = null;
            foreach (var pin in Input)
            {
                if (pin.IsAny)
                {
                    inputPin = pin;
                    break;
                }
            }

            if (inputPin == null)
                Input.Insert(0, inputPin = new ConnectorInVM(this));

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

        //private void LoopProtectForward(NodeVM node, IEnumerable<NodeVM> loopProtect)
        //{
        //    if (loopProtect.Contains(node)) throw new NotSupportedException("Loops is not supported!");

        //    loopProtect = loopProtect.Append(node);

        //    foreach (var pin in node.Output)
        //    {
        //        foreach (var connection in pin.Connections)
        //        {
        //            LoopProtectForward(connection.Parent, loopProtect);
        //        }
        //    }
        //}

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


        private void SetNodeGeneration(NodeVM node, int generation, List<NodeVM> nodes)
        {

            if (generation > node.Generation)
            {
                node.Generation = generation;

                foreach (var item in node.Output)
                {
                    foreach (var connection in item.Connections)
                    {
                        if (nodes.Contains(connection.Parent))
                            SetNodeGeneration(connection.Parent, generation + 1, nodes);
                    }
                }
            }

            if (generation > generationCount)
            {
                generationCount = generation;
            }
        }

        private NodeVM AddNodeToList(List<NodeVM> nodes, NodeVM node)
        {
            if (nodes.Contains(node))
                return node;

            nodes.Add(node);

            foreach (var pin in node.Input)
            {
                if (pin.Connection != null)
                    AddNodeToList(nodes, pin.Connection.Parent);
            }


            return node;
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

        internal void AddtToCellWithFindRow(ConnectorVM pin, string columnName, double value)
        {
            var row = Rows.FirstOrDefault(m => m.Id == pin.Name);

            if (row == null)
            {
                row = new ResultTableRow(pin.Name);
                var nameModel = new ResultTableCellName();
                nameModel.Name = pin.Name;
                nameModel.Icon = pin.Icon;
                nameModel.IconBackground = pin.BackgroundColor;
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
