using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


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
        private double timeToCraft;

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

                SyncItems(Input, recipe.Input, true);
                SyncItems(Output, recipe.Output, false);
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
                SyncItems(Input, Array.Empty<ResourceValueVM>(), true);
                SyncItems(Output, Array.Empty<ResourceValueVM>(), false);
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

            result.Input.Add(new ConnectorVM(result, true)
            {
                Name = "Coal",
                ValueExpression = "1",
                ConnectorCollor = Colors.Black
            });

            result.Output.Add(new ConnectorVM(result, false)
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


        partial void OnTimeToCraftChanged(double value)
        {
            foreach (var item in Input)
                item.TimeToCraft = TimeToCraft;

            foreach (var item in Output)
                item.TimeToCraft = TimeToCraft;
        }

        partial void OnTimeToCraftExpressionChanged(string? value)
        {
            RefreshTimeToCraft();
        }

        public void RefreshTimeToCraft()
        {

            TimeToCraft = ExpressionUtils.GetValue(TimeToCraftExpression, Variables.Where(m => m.Name != null).ToDictionary(m => m.Name!, m => m.Value));
        }

        private void SyncItems(ObservableCollection<ConnectorVM> pins, IEnumerable<ResourceValueVM> recipe, bool input)
        {
            var oldList = pins.ToList();
            pins.Clear();

            foreach (var item in recipe)
            {
                var pin = oldList.FirstOrDefault(m => m.Name == item.Name);
                if (pin == null)
                    pin = new ConnectorVM(this, item.Name, input);
                else
                    oldList.Remove(pin);

                pin.TimeToCraft = TimeToCraft;
                pin.ValueExpression = item.Value;
                pins.Add(pin);
            }

            //foreach (var pin in oldList)
            //{
            //    var resource = Parent.Pipeline.Resources.FirstOrDefault(m => m.Name == pin.Name);
            //    pin.TimeToCraft = TimeToCraft;
            //    pins.Add(pin);
            //}
        }
    }
}