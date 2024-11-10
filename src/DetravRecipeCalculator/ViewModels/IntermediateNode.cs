using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public abstract partial class IntermediateNode : NodeVM
    {
        [ObservableProperty]
        private string? resourceName;

        protected IntermediateNode(GraphEditorVM parent, string? resourceName) : base(parent)
        {
            ResourceName = resourceName;
            RestorePins();
        }

        public override void RestoreState(NodeModel model)
        {
            ResourceName = model.ResourceName;
            base.RestoreState(model);
            RestorePins();
        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();
            model.ResourceName = ResourceName;
            return model;
        }

        private void RestorePins()
        {
            var resource = Parent.Pipeline.Resources.FirstOrDefault(m => m.Name == ResourceName);

            if (resource == null)
            {
                Input.Clear();
                Output.Clear();

                Input.Add(new ConnectorVM(this, true));
                Output.Add(new ConnectorVM(this, false));
            }
            else
            {
                for (int i = 0; i < Input.Count; i++)
                    if (Input[i].Name != ResourceName || Input[i].IsAny)
                    {
                        Input.RemoveAt(i);
                        i--;
                    }

                for (int i = 0; i < Output.Count; i++)
                    if (Output[i].Name != ResourceName || Output[i].IsAny)
                    {
                        Output.RemoveAt(i);
                        i--;
                    }

                if (Input.Count == 0)
                    Input.Add(new ConnectorVM(this, ResourceName, true));
                if (Output.Count == 0)
                    Output.Add(new ConnectorVM(this, ResourceName, false));

            }
        }

        public override ConnectorVM GetReplacementFor(ConnectorVM self, ConnectorVM other)
        {
            if (self.IsAny && !other.IsAny)
            {
                ResourceName = other.Name;

                var state = other.SaveState();

                foreach (var pin in Input)
                    pin.RestoreState(state);
                foreach (var pin in Output)
                    pin.RestoreState(state);

                return self;
            }

            return base.GetReplacementFor(self, other);
        }
    }
}

