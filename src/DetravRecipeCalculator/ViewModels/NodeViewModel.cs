using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DetravRecipeCalculator.ViewModels
{
    public abstract partial class NodeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? title;
        [ObservableProperty]
        private Point location;


        public ObservableCollection<ConnectorViewModel> Input { get; } = new ObservableCollection<ConnectorViewModel>();
        public ObservableCollection<ConnectorViewModel> Output { get; } = new ObservableCollection<ConnectorViewModel>();


        public virtual void RestoreState(PipelineVM pipeline, NodeModel model)
        {

            Location = model.Location;

            Input.Clear();
            foreach (var itemModel in model.Input)
            {
                var item = new ConnectorViewModel();
                item.RestoreState(itemModel);
                item.IsInput = true;
                Input.Add(item);
            }
            Output.Clear();
            foreach (var itemModel in model.Output)
            {
                var item = new ConnectorViewModel();
                item.RestoreState(itemModel);
                item.IsInput = false;
                Output.Add(item);
            }

            RefreshValues(pipeline);

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


        public virtual void RefreshValues(PipelineVM pipeline, RecipeVM? recipe = null)
        {

        }

    }
}
