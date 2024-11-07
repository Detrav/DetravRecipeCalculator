using Avalonia;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using Nodify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class GraphEditorVM : ViewModelBase, IUndoRedoObject
    {
        private readonly ObservableCollection<ConnectionVM> _connections = new ObservableCollection<ConnectionVM>();

        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();

        public IEnumerable<ConnectionVM> Connections => _connections;

        public PendingConnectionVM PendingConnection { get; }

        [ObservableProperty]
        private bool saved;

        [ObservableProperty]
        private ObservableCollection<NodeViewModel>? selectedNodes = new ObservableCollection<NodeViewModel>();

        public ICommand DisconnectConnectorCommand { get; }


        public UndoRedoManager UndoRedo { get; }
        public PipelineVM Pipeline { get; }

        public GraphEditorVM()
        {
            Pipeline = new PipelineVM();
            UndoRedo = new UndoRedoManager(this);
            PendingConnection = new PendingConnectionVM(this);
            DisconnectConnectorCommand = new RelayCommand<ConnectorViewModel>(DeleteConnectionFor);
        }

        public void Disconnect(ConnectionVM? model)
        {
            if (model != null)
            {
                DeleteConnetion(model);
            }
        }

        private void DeleteConnectionFor(ConnectorViewModel? model)
        {
            if (model != null)
            {
                foreach (var connection in Connections.ToArray())
                {
                    if (connection.Output == model || connection.Input == model)
                    {
                        DeleteConnetion(connection);
                    }
                }
            }
        }


        public GraphEditorVM(PipelineVM pipeline)
            : this()
        {
            Pipeline = pipeline;
            //Nodes.Add(new CommentNodeViewModel());
        }

        public object SaveState()
        {

            return SaveState(Nodes, Connections);
        }

        public static GraphModel SaveState(IEnumerable<NodeViewModel> nodes, IEnumerable<ConnectionVM> connections)
        {
            var model = new GraphModel();


            foreach (var node in nodes)
            {
                var nodeModel = node.SaveState();
                model.Nodes.Add(nodeModel);
            }

            foreach (var connection in connections)
            {
                var nodeConnection = connection.SaveState();

                if (nodes.SelectMany(m => m.Input).Any(m => m.Id == nodeConnection.InputId) &&
                    nodes.SelectMany(m => m.Output).Any(m => m.Id == nodeConnection.OutputId))
                {
                    model.Connections.Add(nodeConnection);
                }
            }

            return model;
        }

        public static void RestoreState(GraphModel model, List<NodeViewModel> nodes, List<ConnectionVM> connections, PipelineVM pipeline)
        {
            foreach (var nodeModel in model.Nodes)
            {
                var vm = NodeViewModelFactory.Create(nodeModel.Type);
                if (vm != null)
                {
                    vm.RestoreState(pipeline, nodeModel);
                    nodes.Add(vm);
                }
            }

            foreach (var connectionModel in model.Connections)
            {
                var input = nodes.SelectMany(x => x.Input).FirstOrDefault(m => m.Id == connectionModel.InputId);
                var output = nodes.SelectMany(x => x.Output).FirstOrDefault(m => m.Id == connectionModel.OutputId);

                if (input != null && output != null)
                {
                    input.ConnectionsNumber++;
                    output.ConnectionsNumber++;
                    connections.Add(new ConnectionVM(output, input));
                }
            }

            foreach (var node in nodes)
            {
                foreach (var item in node.Input)
                    item.Id = null;
                foreach (var item in node.Output)
                    item.Id = null;
            }
        }

        public void RestoreState(object state)
        {
            if (state is GraphModel model)
            {
                Nodes.Clear();
                _connections.Clear();

                var nodesList = new List<NodeViewModel>();
                var connectionsList = new List<ConnectionVM>();
                RestoreState(model, nodesList, connectionsList, Pipeline);

                foreach (var item in nodesList)
                    Nodes.Add(item);
                foreach (var item in connectionsList)
                    _connections.Add(item);

            }
        }

        public byte[]? Copy(Point point)
        {
            if (SelectedNodes == null || SelectedNodes.Count == 0)
                return null;

            var model = SaveState(SelectedNodes, Connections);

            foreach (var item in model.Nodes)
            {
                item.Location -= point;
            }

            try
            {
                var str = JsonSerializer.Serialize(model, Utils.SourceGenerationContext.Default.GraphModel);

                return Encoding.UTF8.GetBytes(str);
            }
            catch
            {
                return null;
            }
        }

        public void Paste(byte[] data, Point point)
        {

            var model = JsonSerializer.Deserialize<GraphModel>(data, SourceGenerationContext.Default.GraphModel);

            if (model != null)
            {

                var nodesList = new List<NodeViewModel>();
                var connectionsList = new List<ConnectionVM>();
                RestoreState(model, nodesList, connectionsList, Pipeline);

                foreach (var item in nodesList)
                {
                    item.Location += point;
                    Nodes.Add(item);
                }
                foreach (var item in connectionsList)
                    _connections.Add(item);

                SelectedNodes = new ObservableCollection<NodeViewModel>(nodesList);
            }
        }

        public void AddConnection(ConnectionVM connetion)
        {
            connetion.Input.ConnectionsNumber++;
            connetion.Output.ConnectionsNumber++;

            _connections.Add(connetion);
        }

        public void DeleteConnetion(ConnectionVM connetion)
        {
            connetion.Input.ConnectionsNumber--;
            connetion.Output.ConnectionsNumber--;

            _connections.Remove(connetion);
        }

        public void DeleteNode(NodeViewModel node)
        {
            Nodes.Remove(node);

            foreach (var item in node.Input)
                DeleteConnectionFor(item);
            foreach (var item in node.Output)
                DeleteConnectionFor(item);

        }

        public byte[]? Cut(Point lastClick)
        {
            var result = Copy(lastClick);

            if (result != null && result.Length > 0 && SelectedNodes != null)
            {

                foreach (var node in SelectedNodes.ToArray())
                {
                    DeleteNode(node);
                }
            }

            return result;
        }
    }
}
