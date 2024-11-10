using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.Views;
using Nodify;
using org.matheval.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ConnectorVM : ViewModelBase
    {
        private readonly Dictionary<string, double> values = new Dictionary<string, double>();

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
        public ObservableCollection<ConnectorVM> Connections { get; } = new ObservableCollection<ConnectorVM>();

        /// <summary>
        /// Collor of pin
        /// </summary>
        [ObservableProperty]
        private Color connectorCollor = Colors.Black;

        /// <summary>
        /// display value in format {valuePerSecond} [{valuePerSecondResult}] q/s
        /// </summary>
        [ObservableProperty]
        private string? displayValuePerTime;

        /// <summary>
        /// Tooltop
        /// </summary>
        [ObservableProperty]
        private string? displayValuePerTimeTip;

        /// <summary>
        /// Flag when on calculation requested resource more then available
        /// </summary>
        [ObservableProperty]
        private bool hasCalculationWarning;

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

        [ObservableProperty]
        private bool isAny;

        /// <summary>
        /// Is connected or not
        /// </summary>
        [ObservableProperty]
        private bool isConnected;

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

        // time time, seconds, ticks or other
        [ObservableProperty]
        private TimeType timeType;

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

        /// <summary>
        /// Abount per specified time
        /// </summary>
        [ObservableProperty]
        private double valuePerTime;

        /// <summary>
        /// Set or not by result table ... ?
        /// </summary>
        [ObservableProperty]
        private bool isSet;

        /// <summary>
        /// For caluclation
        /// </summary>
        [ObservableProperty]
        private double tempRequest;
        /// <summary>
        /// For caluclation
        /// </summary>
        [ObservableProperty]
        private double tempCurrentValue;

        public ConnectorVM(NodeVM node, bool isInput)
        {
            this.IsInput = isInput;
            TimeType = node.TimeType;
            Parent = node;
            IsAny = true;
            UpdateDisplayValuePerTime();

            Connections.CollectionChanged += Connections_CollectionChanged;
        }

        private void Connections_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsConnected = Connections.Count != 0;
        }

        public ConnectorVM(NodeVM node, string? name, bool isInput)
            : this(node, isInput)
        {
            this.Name = name;
            AssignIcon();
        }

        public bool IsInput { get; }
        public NodeVM Parent { get; }


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

        public void RestoreState(PinModel model)
        {
            Id = model.Id;
            IsAny = true;
            Name = model.Name;
            TimeToCraft = model.TimeToCraft;
            Value = model.Value;
            IsSet = model.IsSet;
            AssignIcon();
        }

        public PinModel SaveState()
        {
            var model = new PinModel();
            model.Id = Id = Guid.NewGuid().ToString();
            model.Name = Name;
            model.Value = Value;
            model.TimeToCraft = TimeToCraft;
            model.IsSet = IsSet;
            return model;
        }

        public void SetParameter(string? name, double value)
        {
            if (!String.IsNullOrEmpty(name))
                values[name] = value;
            RefreshValue();
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
                IsAny = false;
            }
            else
            {
                ConnectorCollor = DetravColorHelper.GetRandomColor(Name);
                IsAny = true;
            }
        }

        partial void OnTimeToCraftChanged(double value)
        {
            UpdateValuePerSecond();
        }

        partial void OnTimeTypeChanged(TimeType value)
        {
            UpdateValuePerTime();
            UpdateDisplayValuePerTime();
        }

        partial void OnValueChanged(double value)
        {
            UpdateValuePerSecond();
        }

        partial void OnValueExpressionChanged(string? value)
        {
            RefreshValue();
        }

        partial void OnValuePerSecondChanged(double value)
        {
            UpdateValuePerTime();
        }

        partial void OnValuePerTimeChanged(double value)
        {
            UpdateDisplayValuePerTime();
        }

        private void RefreshValue()
        {
            var newValue = ExpressionUtils.GetValue(ValueExpression, values);
            Value = newValue;
        }

        private void UpdateDisplayValuePerTime()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GetFormated(ValuePerTime));

            sb.Append(' ');

            sb.Append('/');
            sb.Append(TimeType.GetLocalizedShortValue());

            DisplayValuePerTime = sb.ToString();

            sb.Clear();

            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_Name"), Name));
            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_ValuePerSecond"), ValuePerSecond, TimeType.Second.GetLocalizedShortValue()));
            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_ValuePerTime"), ValuePerTime, TimeType.GetLocalizedShortValue()));

            DisplayValuePerTimeTip = sb.ToString();
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

        private void UpdateValuePerTime()
        {
            ValuePerTime = ValuePerSecond * TimeType.GetTimeInSeconds();
        }
    }
}