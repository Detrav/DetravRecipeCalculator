using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ConnectorInVM : ConnectorVM
    {
        [ObservableProperty]
        private ConnectorOutVM? connection;

        public ConnectorInVM(NodeVM node) : base(node)
        {
        }

        public ConnectorInVM(NodeVM node, string? name) : base(node, name)
        {
        }

        partial void OnConnectionChanged(ConnectorOutVM? value)
        {
            IsConnected = Connection != null;
        }
    }
}
