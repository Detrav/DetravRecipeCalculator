using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class RecipeNodeVM : NodeViewModel
    {
        public static RecipeNodeVM DebugInstance { get; } = CreateDebugInstance();

        private static RecipeNodeVM CreateDebugInstance()
        {
            var result = new RecipeNodeVM()
            {
                Title = "DebugNode",
                Number = 1,
                Tier = 1,
                TimeToCraftExpression = "10",
            };

            result.Input.Add(new ConnectorViewModel()
            {
                Tier = 1,
                Number = 1,
                IsInput = true,
                Name = "Coal",
                ValueExpression = "1",
                ConnectorCollor = Colors.Black
            });

            result.Output.Add(new ConnectorViewModel()
            {
                Tier = 1,
                Number = 1,
                IsInput = false,
                Name = "EU",
                ValueExpression = "2000",
                ConnectorCollor = Colors.Wheat
            });

            return result;
        }

        [ObservableProperty]
        private Color? backgroundColor;

        [ObservableProperty]
        private byte[]? icon;

        [ObservableProperty]
        private string? recipeId;

        [ObservableProperty]
        private string? note;

        [ObservableProperty]
        private int tier = 1;

        [ObservableProperty]
        private double timeToCraft;

        [ObservableProperty]
        private string? timeToCraftExpression;

        [ObservableProperty]
        private int number = 1;



        public ICommand UpTierCommand { get; }
        public ICommand DownTierCommand { get; }

        public ICommand UpNumberCommand { get; }
        public ICommand DownNumberCommand { get; }

        public RecipeNodeVM()
        {
            UpTierCommand = new RelayCommand(() => Tier++);
            DownTierCommand = new RelayCommand(() => Tier--);
            UpNumberCommand = new RelayCommand(() => Number++);
            DownNumberCommand = new RelayCommand(() => Number--);
        }

        public override void RefreshValues(PipelineVM pipeline, RecipeVM? recipe = null)
        {
            if (recipe == null)
            {
                recipe = pipeline.Recipes.FirstOrDefault(m => m.Id == RecipeId);
            }

            if (recipe != null)
            {
                RecipeId = recipe.Id;
                Title = recipe.Name;
                TimeToCraftExpression = recipe.TimeToCraft;
                Icon = recipe.IconFiltered;
                BackgroundColor = recipe.BackgroundColorValue;
                Note = recipe.Note;

                SyncItems(pipeline, Input, recipe.Input, true);
                SyncItems(pipeline, Output, recipe.Output, false);
            }
        }

        private void SyncItems(PipelineVM pipeline, ObservableCollection<ConnectorViewModel> pins, ObservableCollection<ResourceValueVM> recipe, bool input)
        {
            var oldList = pins.ToList();
            pins.Clear();


            foreach (var item in recipe)
            {
                var pin = oldList.FirstOrDefault(m => m.Name == item.Name);
                var resource = pipeline.Resources.FirstOrDefault(m => m.Name == item.Name);

                if (pin == null)
                {
                    pin = new ConnectorViewModel();
                }
                else
                {
                    oldList.Remove(pin);
                }

                pin.IsInput = input;
                pin.RefreshValues(this, item, resource);

                pins.Add(pin);
            }

            foreach (var item in oldList)
            {
                pins.Add(item);
            }

        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();


            model.RecipeId = RecipeId;
            model.Tier = Tier;
            model.Number = Number;


            return model;
        }

        public override void RestoreState(PipelineVM pipeline, NodeModel model)
        {
            RecipeId = model.RecipeId;
            Tier = model.Tier;
            Number = model.Number;

            base.RestoreState(pipeline, model);
        }

        partial void OnTierChanged(int value)
        {
            RefreshTimeToCraft();

            foreach (var item in Input)
                item.Tier = Tier;

            foreach (var item in Output)
                item.Tier = Tier;
        }

        partial void OnNumberChanged(int value)
        {
            RefreshTimeToCraft();

            foreach (var item in Input)
                item.Number = Number;

            foreach (var item in Output)
                item.Number = Number;
        }

        partial void OnTimeToCraftExpressionChanged(string? value)
        {
            RefreshTimeToCraft();
        }

        private void RefreshTimeToCraft()
        {
            TimeToCraft = ExpressionUtils.GetValue(TimeToCraftExpression, Tier, 1) * Number;
        }

        partial void OnTimeToCraftChanged(double value)
        {
            foreach (var item in Input)
                item.TimeToCraft = TimeToCraft;

            foreach (var item in Output)
                item.TimeToCraft = TimeToCraft;
        }
    }
}