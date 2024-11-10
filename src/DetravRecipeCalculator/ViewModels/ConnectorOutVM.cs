using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ConnectorOutVM : ConnectorVM
    {
        public ConnectorOutVM(NodeVM node) : base(node)
        {
            Connections.CollectionChanged += Connections_CollectionChanged;
        }

        public ConnectorOutVM(NodeVM node, string? name) : base(node, name)
        {
            Connections.CollectionChanged += Connections_CollectionChanged;
        }

        private void Connections_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsConnected = Connections.Count != 0;
        }

        /// <summary>
        /// Number of connections
        /// </summary>
        public ObservableCollection<ConnectorInVM> Connections { get; } = new ObservableCollection<ConnectorInVM>();
    }
}
