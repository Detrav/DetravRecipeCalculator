using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                var item = new ConnectorVM(this);
                item.RestoreState(itemModel);
                item.TimeType = TimeType;
                item.IsInput = true;
                Input.Add(item);
            }
            Output.Clear();
            foreach (var itemModel in model.Output)
            {
                var item = new ConnectorVM(this);
                item.RestoreState(itemModel);
                item.TimeType = TimeType;
                item.IsInput = false;
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
                if (item.IsConnected && item.SaveState() is PinModel itemModel)
                    model.Input.Add(itemModel);
            }

            foreach (var item in Output)
            {
                if (item.IsConnected && item.SaveState() is PinModel itemModel)
                    model.Output.Add(itemModel);
            }

            return model;
        }
    }
}