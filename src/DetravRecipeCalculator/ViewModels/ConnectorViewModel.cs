using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ConnectorViewModel : ViewModelBase
    {
        public ConnectorViewModel()
        {

        }

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

        internal void RefreshValues(ResourceValueVM item, ResourceVM? resource)
        {
            Name = item.Name;
            if (resource != null)
            {
                Icon = resource.IconFiltered;
                ConnectorCollor = resource.ConnectorColorValue;
                BackgroundColor = resource.BackgroundColorValue;
                ValueExpression = item.Value;
            }
        }

        partial void OnValueExpressionChanged(string? value)
        {
            Value = ExpressionUtils.GetValue(ValueExpression, Tier, 0);
        }

        partial void OnTierChanged(int value)
        {
            Value = ExpressionUtils.GetValue(ValueExpression, Tier, 0);
        }
    }
}
