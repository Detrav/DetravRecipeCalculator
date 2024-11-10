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
    public abstract partial class ConnectorVM : ViewModelBase
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
        /// Set or not by result table ... ?
        /// </summary>
        [ObservableProperty]
        private bool isSet;

        /// <summary>
        /// How many resources were requested
        /// </summary>
        [ObservableProperty]
        private double tempRequest;
        /// <summary>
        /// The amount of resources produced may be greater than requested, i.e. there may be surpluses
        /// </summary>
        [ObservableProperty]
        private double tempCurrentValue;

        protected ConnectorVM(NodeVM node)
        {
            Parent = node;
            IsAny = true;

        }



        protected ConnectorVM(NodeVM node, string? name)
            : this(node)
        {
            this.Name = name;
            AssignIcon();
        }


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
            model.IsSet = IsSet;
            return model;
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


        public double GetValuePerSecond()
        {
            return Value / Parent.TimeToCraft;
        }

        public void UpdateExpressions(IEnumerable<VariableVM> variables)
        {
            if (!string.IsNullOrWhiteSpace(ValueExpression))
            {
                Value = ExpressionUtils.GetValue(ValueExpression, variables);
            }
        }

        public void UpdateTooltips()
        {

            var timeType = Parent.Parent.TimeType;

            StringBuilder sb = new StringBuilder();

            double valuePerSecond = GetValuePerSecond();
            double timeInSeconds = timeType.GetTimeInSeconds();
            double valuePerTime = valuePerSecond * timeInSeconds;
            double requestPerSecond = TempRequest * timeInSeconds;
            double surplusPersecond = (TempCurrentValue - TempRequest) * timeInSeconds;
            double totalValue = TempCurrentValue * timeInSeconds;

            if (Math.Abs(totalValue) < 0.000000001)
            {

                sb.Append(GetFormated(valuePerTime));
            }
            else
            {
                sb.Append(GetFormated(requestPerSecond));

                if (Math.Abs(surplusPersecond) > 0.000000001)
                {
                    sb.Append("+").Append(GetFormated(surplusPersecond));
                }
            }

            sb.Append(' ');

            sb.Append('/');
            sb.Append(timeType.GetLocalizedShortValue());
            DisplayValuePerTime = sb.ToString();



            sb.Clear();

            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_Name"), Name));
            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_ValuePerSecond"), valuePerSecond, TimeType.Second.GetLocalizedShortValue()));
            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_ValuePerTime"), valuePerTime, timeType.GetLocalizedShortValue()));
            sb.AppendLine();
            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_RequestValue"), requestPerSecond, timeType.GetLocalizedShortValue()));
            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_SurplussValue"), surplusPersecond, timeType.GetLocalizedShortValue()));
            sb.AppendLine();
            sb.AppendLine(String.Format(Xloc.Get("__ConnectorTip_CurrentValue"), totalValue, timeType.GetLocalizedShortValue()));


            DisplayValuePerTimeTip = sb.ToString();


        }

    }
}