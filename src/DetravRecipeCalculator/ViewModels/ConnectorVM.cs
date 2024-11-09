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
    public partial class ConnectorVM : ViewModelBase
    {
        /// <summary>
        /// Point to connect
        /// </summary>
        [ObservableProperty]
        private Point anchor;

        /// <summary>
        /// Color of background icon
        /// </summary>
        [ObservableProperty]
        private Color backgroundColor;

        /// <summary>
        /// Number of connections
        /// </summary>
        [ObservableProperty]
        private int connectionsNumber;

        /// <summary>
        /// Collor of pin
        /// </summary>
        [ObservableProperty]
        private Color connectorCollor = Colors.Black;

        /// <summary>
        /// Current icon
        /// </summary>
        [ObservableProperty]
        private byte[]? icon;

        /// <summary>
        /// Id for serialization
        /// </summary>
        [ObservableProperty]
        private string? id;

        /// <summary>
        /// Is connected or not
        /// </summary>
        [ObservableProperty]
        private bool isConnected;

        /// <summary>
        /// Is input or output
        /// </summary>
        [ObservableProperty]
        private bool isInput;

        /// <summary>
        /// Is unknonwn, means not in the recipe, mb deleted
        /// </summary>
        [ObservableProperty]
        private bool isUnknown;

        /// <summary>
        /// Display name
        /// </summary>
        [ObservableProperty]
        private string? name;

        /// <summary>
        /// Time to craft in seconds
        /// </summary>
        [ObservableProperty]
        private double timeToCraft;

        /// <summary>
        /// Current amount of craft
        /// </summary>
        [ObservableProperty]
        private double value;

        /// <summary>
        /// Current amount expression for calculation
        /// </summary>
        [ObservableProperty]
        private string? valueExpression;

        /// <summary>
        /// Amount per second
        /// </summary>
        [ObservableProperty]
        private double valuePerSecond;

        // time time, seconds, ticks or other
        [ObservableProperty]
        private TimeType timeType;

        /// <summary>
        /// display value in format {valuePerSecond} [{valuePerSecondResult}] q/s
        /// </summary>
        [ObservableProperty]
        private string? displayValuePersecond;

        /// <summary>
        /// Tooltop
        /// </summary>
        [ObservableProperty]
        private string? displayValuePersecondTip;

        /// <summary>
        /// Flag when on calculation requested resource more then available
        /// </summary>
        [ObservableProperty]
        private bool hasCalculationWarning;

        [ObservableProperty]
        private bool isAny;

        private readonly Dictionary<string, double> values = new Dictionary<string, double>();

        public void RestoreState(PinModel model)
        {
            Id = model.Id;
            IsAny = model.IsAny;
            Name = model.Name;
            AssignIcon();

        }

        private void AssignIcon()
        {
            var pipeline = Parent.Parent.Pipeline;

            var resource = pipeline.Resources.FirstOrDefault(m => m.Name == Name);

            if (resource != null)
            {
                Icon = resource.IconFiltered;
                ConnectorCollor = resource.ConnectorColorValue;
                BackgroundColor = resource.BackgroundColorValue;
                IsUnknown = false;
            }
            else
            {
                ConnectorCollor = DetravColorHelper.GetRandomColor(Name);
                IsUnknown = true;
            }
        }

        public PinModel SaveState()
        {
            var model = new PinModel();
            model.Id = Id = Guid.NewGuid().ToString();
            model.IsAny = IsAny;
            model.Name = Name;
            return model;
        }

        partial void OnConnectionsNumberChanged(int value)
        {
            IsConnected = value != 0;
        }

        partial void OnTimeToCraftChanged(double value)
        {
            UpdateValuePerSecond();
        }

        partial void OnValueChanged(double value)
        {
            UpdateValuePerSecond();
        }

        partial void OnValueExpressionChanged(string? value)
        {
            RefreshValue();
        }

        partial void OnTimeTypeChanged(TimeType value)
        {
            UpdateValuePerSecond();
            UpdateDisplayValuePersecond();
        }

        private void RefreshValue()
        {
            var newValue = ExpressionUtils.GetValue(ValueExpression, values);
            Value = newValue;
        }

        private void UpdateValuePerSecond()
        {
            var time = TimeToCraft;

            if (Math.Abs(time) < 0.0000001)
            {
                time = 0.0000001;
            }

            ValuePerSecond = Value / time * TimeType.GetTimeInSeconds();
        }

        public NodeVM Parent { get; }

        public ConnectorVM(NodeVM node)
        {

            Parent = node;
            UpdateDisplayValuePersecond();
        }

        public ConnectorVM(NodeVM node, string? name)
            : this(node)
        {
            this.Name = name;
            AssignIcon();
        }

        private void UpdateDisplayValuePersecond()
        {
            StringBuilder sb = new StringBuilder();


            sb.Append(GetFormated(ValuePerSecond));

            sb.Append(' ');

            sb.Append('/');
            sb.Append(TimeType.GetLocalizedShortValue());


            DisplayValuePersecond = sb.ToString();

            sb.Clear();

            sb.Append(ValuePerSecond).Append(' ');
            sb.Append('/');
            sb.Append(TimeType.GetLocalizedShortValue());


            DisplayValuePersecondTip = sb.ToString();

        }

        public static string GetFormated(double v)
        {

            if (Math.Abs(v) >= 100)
            {
                return string.Format("{0:0}", v);
            }
            else if (Math.Abs(v) >= 10)
            {
                return string.Format("{0:0.#}", v);
            }
            else if (Math.Abs(v) >= 1)
            {
                return string.Format("{0:0.##}", v);
            }
            else if (Math.Abs(v) >= 0.1)
            {
                return string.Format("{0:0.###}", v);
            }
            else
            {
                return string.Format("{0:0.####}", v);
            }
        }

        partial void OnValuePerSecondChanged(double value)
        {
            UpdateDisplayValuePersecond();
        }


        public void SetParameter(string? name, double value)
        {
            if (!String.IsNullOrEmpty(name))
                values[name] = value;
            RefreshValue();
        }
    }
}