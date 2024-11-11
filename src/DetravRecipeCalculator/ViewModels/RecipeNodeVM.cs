using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;


namespace DetravRecipeCalculator.ViewModels
{
    public partial class RecipeNodeVM : NodeVM
    {
        [ObservableProperty]
        private Color backgroundColor;

        [ObservableProperty]
        private byte[]? icon;

        [ObservableProperty]
        private string? note;

        [ObservableProperty]
        private string? recipeId;

        [ObservableProperty]
        private string? timeToCraftExpression;
        [ObservableProperty]
        private bool isUnknown;

        public ObservableCollection<VariableVM> Variables { get; } = new ObservableCollection<VariableVM>();

        public RecipeNodeVM(GraphEditorVM parent)
            : base(parent)
        {

        }

        public RecipeNodeVM(GraphEditorVM parent, RecipeVM? recipe = null) : this(parent)
        {
            AssignRecipe(recipe);

        }

        private void AssignRecipe(RecipeVM? recipe)
        {
            if (recipe != null)
            {
                RecipeId = recipe.Id;
                Title = recipe.Name;
                TimeToCraftExpression = recipe.TimeToCraft;
                Icon = recipe.IconFiltered;
                BackgroundColor = recipe.BackgroundColorValue;
                Note = recipe.Note;

                SyncItems(Input, recipe.Input);
                SyncItems(Output, recipe.Output);
                IsUnknown = false;

                foreach (var kv in ExpressionUtils.Split(recipe.Variables))
                {

                    var v1 = new VariableVM(Parent, this);
                    Variables.Add(v1);
                    v1.Name = kv.Key;
                    v1.Value = kv.Value;
                }
            }
            else
            {
                Title = RecipeId;
                SyncItems(Input, Array.Empty<ResourceValueVM>());
                SyncItems(Output, Array.Empty<ResourceValueVM>());
                IsUnknown = true;
            }
        }

        public static RecipeNodeVM DebugInstance { get; } = CreateDebugInstance();



        public override void RestoreState(NodeModel model)
        {
            RecipeId = model.RecipeId;

            base.RestoreState(model);

            var recipe = Parent.Pipeline.Recipes.FirstOrDefault(m => m.Id == RecipeId);

            AssignRecipe(recipe);

            foreach (var kv in model.Variables)
            {
                var v = Variables.FirstOrDefault(m => m.Name == kv.Key);

                if (v != null)
                {
                    v.Value = kv.Value;
                }
            }
        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();

            model.RecipeId = RecipeId;

            foreach (var variable in Variables)
            {
                if (variable.Name != null)
                    model.Variables[variable.Name] = variable.Value;
            }

            return model;
        }

        private static RecipeNodeVM CreateDebugInstance()
        {
            var result = new RecipeNodeVM(new GraphEditorVM())
            {
                Title = "DebugNode",
                TimeToCraftExpression = "10",
            };

            result.Input.Add(new ConnectorInVM(result)
            {
                Name = "Coal",
                ValueExpression = "1",
                ConnectorCollor = Colors.Black
            });

            result.Output.Add(new ConnectorOutVM(result)
            {
                Name = "EU",
                ValueExpression = "2000",
                ConnectorCollor = Colors.Wheat
            });

            result.Variables.Add(new VariableVM(result.Parent, result)
            {
                Name = "Tier",
                Value = 1
            });

            return result;
        }

        private void SyncItems(ObservableCollection<ConnectorOutVM> pins, IEnumerable<ResourceValueVM> recipe)
        {
            var oldList = pins.ToList();
            pins.Clear();

            foreach (var item in recipe)
            {
                var pin = oldList.FirstOrDefault(m => m.Name == item.Name);
                if (pin == null)
                    pin = new ConnectorOutVM(this, item.Name);
                else
                    oldList.Remove(pin);

                pin.ValueExpression = item.Value;
                pins.Add(pin);
            }
        }

        private void SyncItems(ObservableCollection<ConnectorInVM> pins, IEnumerable<ResourceValueVM> recipe)
        {
            var oldList = pins.ToList();
            pins.Clear();

            foreach (var item in recipe)
            {
                var pin = oldList.FirstOrDefault(m => m.Name == item.Name);
                if (pin == null)
                    pin = new ConnectorInVM(this, item.Name);
                else
                    oldList.Remove(pin);

                pin.ValueExpression = item.Value;
                pins.Add(pin);
            }

        }


        public override void UpdateExpressions()
        {
            base.UpdateExpressions();

            TimeToCraft = ExpressionUtils.GetValue(TimeToCraftExpression, Variables);

            foreach (var pin in Input)
                pin.UpdateExpressions(Variables);

            foreach (var pin in Output)
                pin.UpdateExpressions(Variables);
        }

        public override void UpdateToolTips()
        {
            var sb = new StringBuilder();




            foreach (var variable in Variables)
            {
                variable.ValueFormated = variable.Name + ": " + ConnectorVM.GetFormated(variable.Value);

                sb.Append(variable.Name).Append(": ").Append(variable.Value).AppendLine();
            }

            sb.AppendLine();

            var qf = Xloc.Get("__NodeTip_Quantity");
            var tf = Xloc.Get("__NodeTip_TimeToCraft");

            TimeToCraftTitle = string.Format(tf, ConnectorVM.GetFormated(TimeToCraft), TimeType.Second.GetLocalizedShortValue());
            QuantityTitle = string.Format(qf, ConnectorVM.GetFormated(Number));


            sb.AppendLine(string.Format(tf, TimeToCraft, TimeType.Second.GetLocalizedShortValue()));
            sb.AppendLine(string.Format(qf, Number));

            ParametersToolTip = sb.ToString();


            base.UpdateToolTips();
        }

        
    }
}