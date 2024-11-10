using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static DetravRecipeCalculator.ViewModels.GraphEditorVM;

namespace DetravRecipeCalculator.ViewModels
{
    public abstract partial class NodeVM : ViewModelBase
    {
        [ObservableProperty]
        private Point location;

        [ObservableProperty]
        private string? title;

        [ObservableProperty]
        private int generation;
        [ObservableProperty]
        private double number;

        [ObservableProperty]
        private string? quantityTitle;
        [ObservableProperty]
        private string? timeToCraftTitle;
        [ObservableProperty]
        private string? parametersToolTip;

        [ObservableProperty]
        private double timeToCraft = 1;

        

        public GraphEditorVMLoc Loc => GraphEditorVMLoc.Instance;

        public NodeVM(GraphEditorVM parent)
        {
            this.Parent = parent;
            Number = 1;
        }



        public ObservableCollection<ConnectorInVM> Input { get; } = new ObservableCollection<ConnectorInVM>();
        public ObservableCollection<ConnectorOutVM> Output { get; } = new ObservableCollection<ConnectorOutVM>();
        public GraphEditorVM Parent { get; }


        public virtual void RestoreState(NodeModel model)
        {
            Location = model.Location;

            Input.Clear();
            foreach (var itemModel in model.Input)
            {
                var item = new ConnectorInVM(this);
                item.RestoreState(itemModel);
                Input.Add(item);
            }
            Output.Clear();
            foreach (var itemModel in model.Output)
            {
                var item = new ConnectorOutVM(this);
                item.RestoreState(itemModel);
                Output.Add(item);
            }
        }

        public virtual NodeModel SaveState()
        {
            var model = new NodeModel();
            model.Type = NodeViewModelFactory.GetName(this);
            model.Location = Location;

            foreach (var item in Input)
            {
                if (!item.IsAny)
                    model.Input.Add(item.SaveState());
            }

            foreach (var item in Output)
            {
                if (!item.IsAny)
                    model.Output.Add(item.SaveState());
            }

            return model;
        }

        public virtual ConnectorOutVM GetReplacementFor(ConnectorOutVM self, ConnectorInVM other)
        {
            //self.RestoreState(other.SaveState());
            return self;
        }

        public virtual ConnectorInVM GetReplacementFor(ConnectorInVM self, ConnectorOutVM other)
        {
            //self.RestoreState(other.SaveState());
            return self;
        }

        public virtual void UpdateToolTips()
        {
            foreach (var pin in Input)
                pin.UpdateTooltips();
            foreach (var pin in Output)
                pin.UpdateTooltips();


        }


        public virtual void Build()
        {

            // calculate percentage

            double requestPercentage;

            if (Output.Count == 0)
            {
                // no need to calculate it is a request node
                requestPercentage = 1;
            }
            else
            {
                requestPercentage = 0;

                foreach (var pinOut in Output)
                {

                    requestPercentage = Math.Max(requestPercentage, pinOut.TempRequest / pinOut.GetValuePerSecond());
                }

                foreach (var pinOut in Output)
                {
                    pinOut.TempCurrentValue = requestPercentage * pinOut.GetValuePerSecond();
                }
            }
            Number = requestPercentage;

            // scale input by percentage

            foreach (var pinIn in Input)
            {
                pinIn.TempCurrentValue = requestPercentage * pinIn.GetValuePerSecond();

                if (pinIn.Connection != null)
                {
                    pinIn.Connection.TempRequest -= pinIn.TempRequest;
                    pinIn.Connection.TempRequest += pinIn.TempCurrentValue;
                }
                pinIn.TempRequest = pinIn.TempCurrentValue;
            }

            // process all input nodes

            foreach (var pinin in Input)
            {
                if (pinin.Connection != null)
                {
                    pinin.Connection.Parent.Build();
                }
            }
        }

        public virtual void UpdateExpressions()
        {

        }
    }
}