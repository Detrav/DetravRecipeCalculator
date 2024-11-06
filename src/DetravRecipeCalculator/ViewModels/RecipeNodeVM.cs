using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        [ObservableProperty]
        private Color? backgroundColor;

        [ObservableProperty]
        private byte[]? icon;

        [ObservableProperty]
        private string? id;

        [ObservableProperty]
        private string? note;

        [ObservableProperty]
        private int tier = 1;

        [ObservableProperty]
        private double timeToCraft;

        [ObservableProperty]
        private string? timeToCraftExpression;

        public ICommand UpTierCommand { get; }
        public ICommand DownTierCommand { get; }

        public RecipeNodeVM()
        {
            UpTierCommand = new RelayCommand(() => Tier++);
            DownTierCommand = new RelayCommand(() => Tier--);
        }

        public ObservableCollection<ConnectorViewModel> Input { get; } = new ObservableCollection<ConnectorViewModel>();

        public ObservableCollection<ConnectorViewModel> Output { get; } = new ObservableCollection<ConnectorViewModel>();

        public void RefreshValues(PipelineVM pipeline, RecipeVM? recipe = null)
        {
            if (recipe == null)
            {
                recipe = pipeline.Recipes.FirstOrDefault(m => m.Id == Id);
            }

            if (recipe != null)
            {
                Id = recipe.Id;
                Title = recipe.Name;
                TimeToCraftExpression = recipe.TimeToCraft;
                Icon = recipe.IconFiltered;
                BackgroundColor = recipe.BackgroundColorValue;
                Note = recipe.Note;

                foreach (var item in recipe.Input)
                {
                    // todo replace old values
                    var resource = pipeline.Resources.FirstOrDefault(m => m.Name == item.Name);
                    var pin = new ConnectorViewModel();
                    pin.Tier = Tier;
                    Input.Add(pin);
                    pin.RefreshValues(item, resource);
                }

                foreach (var item in recipe.Output)
                {
                    // todo replace old values
                    var resource = pipeline.Resources.FirstOrDefault(m => m.Name == item.Name);
                    var pin = new ConnectorViewModel();
                    pin.Tier = Tier;
                    Output.Add(pin);
                    pin.RefreshValues(item, resource);
                }
            }
        }

        partial void OnTierChanged(int value)
        {
            TimeToCraft = ExpressionUtils.GetValue(TimeToCraftExpression, Tier, 1);

            foreach (var item in Input)
                item.Tier = Tier;
        }

        partial void OnTimeToCraftExpressionChanged(string? value)
        {
            TimeToCraft = ExpressionUtils.GetValue(TimeToCraftExpression, Tier, 1);
        }
    }
}