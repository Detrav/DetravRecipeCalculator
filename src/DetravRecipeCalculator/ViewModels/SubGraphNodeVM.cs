﻿using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public class SubGraphNodeVM : NodeVM
    {
        private GraphModel? graphModel;

        public SubGraphNodeVM(GraphEditorVM parent) : base(parent)
        {
            TimeToCraft = 1;
        }


        public override void FixPins()
        {
            base.FixPins();

            SyncItems(Input);
            SyncItems(Output);
        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();
            model.GraphModel = graphModel;
            return model;
        }

        public override void RestoreState(NodeModel model)
        {
            graphModel = model.GraphModel;
            base.RestoreState(model);
            TimeToCraft = 1;
        }

        public override void Build()
        {
            base.Build();
        }

        private void SyncItems(ObservableCollection<ConnectorOutVM> pins)
        {
            var oldList = pins.ToList();
            pins.Clear();

            if (graphModel != null)
            {

                foreach (var item in graphModel.Inputs)
                {
                    var pin = oldList.FirstOrDefault(m => m.Name == item.Key);
                    if (pin == null)
                        pin = new ConnectorOutVM(this, item.Key);
                    pin.Value = item.Value;
                    pins.Add(pin);
                }
            }
        }

        private void SyncItems(ObservableCollection<ConnectorInVM> pins)
        {
            var oldList = pins.ToList();
            pins.Clear();

            if (graphModel != null)
            {
                foreach (var item in graphModel.Outputs)
                {
                    var pin = oldList.FirstOrDefault(m => m.Name == item.Key);
                    if (pin == null)
                        pin = new ConnectorInVM(this, item.Key);
                    pin.Value = item.Value;
                    pins.Add(pin);
                }
            }

        }

        public GraphEditorVM LoadSubGraph()
        {
            var vm = new GraphEditorVM();
            if (graphModel != null)
                vm.RestoreState(graphModel);
            return vm;
        }

        public void SaveSubGraph(GraphEditorVM graph)
        {
            graphModel = graph.SaveState() as GraphModel;
        }

    }
}
