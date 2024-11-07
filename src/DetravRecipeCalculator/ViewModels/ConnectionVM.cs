using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ConnectionVM : ViewModelBase
    {
        public ConnectionVM(ConnectorVM output, ConnectorVM input)
        {
            this.Output = output;
            this.Input = input;
        }

        public ConnectorVM Input { get; }
        public ConnectorVM Output { get; }

        public ConnectionModel SaveState()
        {
            ConnectionModel model = new ConnectionModel();
            // new guid for not serialized input and output
            model.InputId = Input.Id ?? Guid.NewGuid().ToString();
            model.OutputId = Output.Id ?? Guid.NewGuid().ToString();
            return model;
        }
    }
}