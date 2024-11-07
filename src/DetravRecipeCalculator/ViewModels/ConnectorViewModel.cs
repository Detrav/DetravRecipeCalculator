using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.Views;
using Nodify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ConnectorViewModel : ViewModelBase
    {

        [ObservableProperty]
        private byte[]? icon;
        [ObservableProperty]
        private Color? connectorCollor;
        [ObservableProperty]
        private Color? backgroundColor;
        [ObservableProperty]
        private string? name;
        [ObservableProperty]
        private double value;
        [ObservableProperty]
        private string? valueExpression;
        [ObservableProperty]
        private int tier;
        [ObservableProperty]
        private int number;
        [ObservableProperty]
        private Point anchor;
        [ObservableProperty]
        private bool isConnected;
        [ObservableProperty]
        private bool isInput;
        [ObservableProperty]
        private double timeToCraft;
        [ObservableProperty]
        private double valuePerSecond;
        [ObservableProperty]
        private double valuePerSecondResult;
        [ObservableProperty]
        private int connectionsNumber;
        [ObservableProperty]
        private string? id;
        [ObservableProperty]
        private bool isUnknown;

        internal void RefreshValues(NodeViewModel nodeVM, ResourceValueVM item, ResourceVM? resource)
        {
            Name = item.Name;
            if (resource != null)
            {
                Icon = resource.IconFiltered;
                ConnectorCollor = resource.ConnectorColorValue;
                BackgroundColor = resource.BackgroundColorValue;
                ValueExpression = item.Value;
                IsUnknown = false;
            }
            else
            {
                IsUnknown = true;
            }
            if (nodeVM is RecipeNodeVM recipeNode)
            {
                TimeToCraft = recipeNode.TimeToCraft;
                Tier = recipeNode.Tier;
                Number = recipeNode.Number;
            }
        }


        partial void OnValueExpressionChanged(string? value)
        {
            RefreshValue();
        }

        partial void OnTierChanged(int value)
        {
            RefreshValue();
        }

        partial void OnNumberChanged(int value)
        {
            RefreshValue();
        }

        private void RefreshValue()
        {
            Value = ExpressionUtils.GetValue(ValueExpression, Tier, 0) * Number;
        }

        partial void OnTimeToCraftChanged(double value)
        {
            UpdateValuePerSecond();
        }

        private void UpdateValuePerSecond()
        {
            var time = TimeToCraft;

            if (Math.Abs(time) < 0.0000001)
            {
                time = 0.0000001;
            }

            ValuePerSecond = Value / time;
        }

        partial void OnValueChanged(double value)
        {
            UpdateValuePerSecond();
        }

        partial void OnConnectionsNumberChanged(int value)
        {
            IsConnected = value != 0;
        }

        public PinModel SaveState()
        {
            var model = new PinModel();
            model.Id = Id = Guid.NewGuid().ToString();
            model.Name = Name;
            return model;
        }

        public void RestoreState(PinModel model)
        {
            Id = model.Id;
            Name = model.Name;

        }
    }
}
