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
        private TimeType timeType;

        [ObservableProperty]
        private int generation;
        [ObservableProperty]
        private double number;

        public GraphEditorVMLoc Loc => GraphEditorVMLoc.Instance;

        public NodeVM(GraphEditorVM parent)
        {
            this.Parent = parent;
            TimeType = parent.TimeType;
        }



        public ObservableCollection<ConnectorVM> Input { get; } = new ObservableCollection<ConnectorVM>();
        public ObservableCollection<ConnectorVM> Output { get; } = new ObservableCollection<ConnectorVM>();
        public GraphEditorVM Parent { get; }

        partial void OnTimeTypeChanged(TimeType value)
        {
            foreach (var item in Input)
                item.TimeType = TimeType;
            foreach (var item in Output)
                item.TimeType = TimeType;
        }


        public virtual void RestoreState(NodeModel model)
        {
            Location = model.Location;

            Input.Clear();
            foreach (var itemModel in model.Input)
            {
                var item = new ConnectorVM(this, true);
                item.RestoreState(itemModel);
                Input.Add(item);
            }
            Output.Clear();
            foreach (var itemModel in model.Output)
            {
                var item = new ConnectorVM(this, false);
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

        public virtual ConnectorVM GetReplacementFor(ConnectorVM self, ConnectorVM other)
        {
            self.RestoreState(other.SaveState());
            return self;
        }


        public virtual void Build(IEnumerable<NodeVM>? tempList = null)
        {
            if (tempList == null) tempList = Array.Empty<NodeVM>();
            else if (tempList.Contains(this)) return;
            tempList = tempList.Append(this);

            double requestPercentage;

            if (Output.Count == 0)
            {
                requestPercentage = 1;
            }
            else
            {
                requestPercentage = 0;

                foreach (var pinOut in Output)
                {
                    requestPercentage = Math.Max(requestPercentage, pinOut.TempRequest / pinOut.ValuePerSecond);
                }

                Number = requestPercentage;

                foreach (var pinOut in Output)
                {
                    pinOut.TempCurrentValue = requestPercentage * pinOut.ValuePerSecond;
                }
            }

            foreach (var pinIn in Input)
            {
                pinIn.TempCurrentValue = requestPercentage * pinIn.ValuePerSecond;

                foreach (var pinOut in pinIn.Connections)
                {
                    pinOut.TempRequest -= pinIn.TempRequest;
                    pinOut.TempRequest += pinIn.TempCurrentValue;
                }
                pinIn.TempRequest = pinIn.TempCurrentValue;
            }

            foreach (var pinin in Input)
            {
                foreach (var pinOut in pinin.Connections)
                {
                    pinOut.Parent.Build(tempList);
                }
            }
        }
    }
}