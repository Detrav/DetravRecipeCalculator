using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class SplitConnectorNodeVM : NodeVM
    {
        [ObservableProperty]
        private string? resourceName;

        public SplitConnectorNodeVM(GraphEditorVM parent, string? resourceName)
            : base(parent)
        {
            this.resourceName = resourceName;
            TimeToCraft = 1;
            FixPins();
        }

        public override void FixPins()
        {
            base.FixPins();

            if (string.IsNullOrWhiteSpace(ResourceName))
            {
                ResourceName = null;
                // no serialization any, just clear and add one
                Input.Clear();
                Output.Clear();

                Input.Add(new ConnectorInVM(this));
                Output.Add(new ConnectorOutVM(this));
            }
            else
            {
                for (int i = 0; i < Input.Count; i++)
                {
                    string? name = Input[i].Name;
                    if (Input[i].IsAny || ResourceName != name)
                    {
                        Input.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < Output.Count; i++)
                {
                    string? name = Output[i].Name;
                    if (Input[i].IsAny || ResourceName != name)
                    {
                        Output.RemoveAt(i);
                        i--;
                    }
                }

                while (Input.Count > 1) Input.RemoveAt(Input.Count - 1);
                while (Output.Count > 1) Output.RemoveAt(Output.Count - 1);

                if (Input.Count < 1) Input.Add(new ConnectorInVM(this, ResourceName));
                if (Output.Count < 1) Output.Add(new ConnectorOutVM(this, ResourceName));
            }

            Input[0].Value = 1;
            Output[0].Value = 1;
        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();
            model.ResourceName = ResourceName;
            return model;
        }

        public override void RestoreState(NodeModel model)
        {
            ResourceName = model.ResourceName;
            base.RestoreState(model);
        }

        public override bool GetReplacementForAny(ConnectorInVM self, ConnectorOutVM other, [NotNullWhen(true)] out ConnectorInVM? newSelf)
        {
            if (Input.Contains(self) && self.IsAny && !other.IsAny && other.Name != null)
            {
                Setup(self, other, other.Name);
                newSelf = self;
                return true;
            }
            else
            {
                newSelf = null;
            }

            return false;

        }

        public override bool GetReplacementForAny(ConnectorOutVM self, ConnectorInVM other, [NotNullWhen(true)] out ConnectorOutVM? newSelf)
        {
            if (Output.Contains(self) && self.IsAny && !other.IsAny && other.Name != null)
            {
                Setup(self, other, other.Name);
                newSelf = self;
                return true;
            }
            else
            {
                newSelf = null;
            }

            return false;
        }

        private void Setup(ConnectorVM self, ConnectorVM other, string resourceName)
        {
            this.ResourceName = resourceName;

            var state = other.SaveState();

            foreach (var pin in Input)
                pin.RestoreState(state);
            foreach (var pin in Output)
                pin.RestoreState(state);
            FixPins();
        }

        public override void RequestResources()
        {
            base.RequestResources();


            var pinIn = Input.FirstOrDefault();
            var pinOut = Output.FirstOrDefault();

            if (pinIn != null && pinOut != null)
            {
                if (pinIn.Connection == null)
                {
                    pinIn.Value = 1;
                }
                else
                {
                    pinIn.Value = pinIn.Connection.Value / pinIn.Connection.Parent.TimeToCraft;
                }
                pinOut.Value = pinIn.Value;
            }
        }

    }
}
