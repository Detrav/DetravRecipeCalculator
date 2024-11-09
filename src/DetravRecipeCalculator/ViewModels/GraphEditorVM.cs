using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class GraphEditorVM : ViewModelBase, IUndoRedoObject
    {
        private readonly ObservableCollection<ConnectionVM> _connections = new ObservableCollection<ConnectionVM>();

        [ObservableProperty]
        private bool saved;

        [ObservableProperty]
        private TimeType timeType;

        [ObservableProperty]
        private ObservableCollection<NodeVM>? selectedNodes = new ObservableCollection<NodeVM>();

        public GraphEditorVM()
        {
            Pipeline = new PipelineVM();
            UndoRedo = new UndoRedoManager(this);
            PendingConnection = new PendingConnectionVM(this);
            DisconnectConnectorCommand = new RelayCommand<ConnectorVM>(model =>
            {
                if (DeleteConnectionFor(model))
                    UndoRedo.PushState("Disconnect");
            });
            ItemsDragCompletedCommand = new RelayCommand(() =>
            {
                UndoRedo.PushState("Move node");
            });
        }

        public GraphEditorVM(PipelineVM pipeline)
            : this()
        {
            Pipeline = pipeline;
        }

        public IEnumerable<ConnectionVM> Connections => _connections;

        public ICommand DisconnectConnectorCommand { get; }

        public ICommand ItemsDragCompletedCommand { get; }

        public GraphEditorVMLoc Loc => GraphEditorVMLoc.Instance;

        public ObservableCollection<NodeVM> Nodes { get; } = new ObservableCollection<NodeVM>();

        public PendingConnectionVM PendingConnection { get; }

        public PipelineVM Pipeline { get; }

        public UndoRedoManager UndoRedo { get; }

        public static void RestoreState(GraphEditorVM parent, GraphModel model, List<NodeVM> nodes, List<ConnectionVM> connections)
        {
            foreach (var nodeModel in model.Nodes)
            {
                var vm = NodeViewModelFactory.Create(nodeModel.Type, parent);
                if (vm != null)
                {
                    vm.RestoreState(nodeModel);
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

        public static void SaveState(GraphModel model, IEnumerable<NodeVM> nodes, IEnumerable<ConnectionVM> connections)
        {




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
        }


        
        public void AddConnection(ConnectionVM connetion)
        {

            connetion.Input.ConnectionsNumber++;
            connetion.Output.ConnectionsNumber++;

            _connections.Add(connetion);
        }

        public byte[]? Copy(Point point)
        {
            if (SelectedNodes == null || SelectedNodes.Count == 0)
                return null;

            GraphModel model = new GraphModel();

            SaveState(model, SelectedNodes, Connections);

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

        public void DeleteConnetion(ConnectionVM connetion)
        {
            connetion.Input.ConnectionsNumber--;
            connetion.Output.ConnectionsNumber--;

            _connections.Remove(connetion);
        }

        public void DeleteNode(NodeVM node)
        {
            Nodes.Remove(node);

            foreach (var item in node.Input)
                DeleteConnectionFor(item);
            foreach (var item in node.Output)
                DeleteConnectionFor(item);
        }

        public void Disconnect(ConnectionVM? model)
        {
            if (model != null)
            {
                DeleteConnetion(model);
            }
        }

        public void Paste(byte[] data, Point point)
        {
            var model = JsonSerializer.Deserialize<GraphModel>(data, SourceGenerationContext.Default.GraphModel);

            if (model != null)
            {
                var nodesList = new List<NodeVM>();
                var connectionsList = new List<ConnectionVM>();
                RestoreState(this, model, nodesList, connectionsList);

                foreach (var item in nodesList)
                {
                    item.Location += point;
                    Nodes.Add(item);
                }
                foreach (var item in connectionsList)
                    _connections.Add(item);

                SelectedNodes = new ObservableCollection<NodeVM>(nodesList);
            }
        }

        public void RestoreState(object state)
        {
            if (state is GraphModel model)
            {
                Nodes.Clear();
                _connections.Clear();

                TimeType = model.TimeType;

                var nodesList = new List<NodeVM>();
                var connectionsList = new List<ConnectionVM>();
                RestoreState(this, model, nodesList, connectionsList);

                foreach (var item in nodesList)
                    Nodes.Add(item);
                foreach (var item in connectionsList)
                    _connections.Add(item);
            }
        }

        public object SaveState()
        {
            Build();

            GraphModel model = new GraphModel();
            model.TimeType = TimeType;
            SaveState(model, Nodes, Connections);
            return model;
        }

        private void Build()
        {
            foreach (var node in Nodes)
            {
                if (node is ResultTableNodeVM resultTableNode)
                {
                    resultTableNode.Rebuild();
                }
            }
        }

        private bool DeleteConnectionFor(ConnectorVM? model)
        {
            bool hasResult = false;
            if (model != null)
            {
                foreach (var connection in Connections.ToArray())
                {
                    if (connection.Output == model || connection.Input == model)
                    {
                        hasResult = true;
                        DeleteConnetion(connection);
                    }
                }
            }
            return hasResult;
        }

        partial void OnTimeTypeChanged(TimeType value)
        {
            foreach (var node in Nodes)
            {
                node.TimeType = TimeType;
            }
        }

        public class GraphEditorVMLoc
        {
            public static GraphEditorVMLoc Instance { get; } = new GraphEditorVMLoc();

            public XLocItem MenuAddComment { get; } = new XLocItem("__GraphEditor_MenuAddComment");
            public XLocItem MenuAddResultTable { get; } = new XLocItem("__GraphEditor_MenuAddResultTable");
            public XLocItem MenuAddNode { get; } = new XLocItem("__GraphEditor_MenuAddNode");
            public XLocItem MenuCopy { get; } = new XLocItem("__GraphEditor_MenuCopy");
            public XLocItem MenuCut { get; } = new XLocItem("__GraphEditor_MenuCut");
            public XLocItem MenuDelete { get; } = new XLocItem("__GraphEditor_MenuDelete");
            public XLocItem MenuEdit { get; } = new XLocItem("__GraphEditor_MenuEdit");
            public XLocItem MenuPaste { get; } = new XLocItem("__GraphEditor_MenuPaste");
            public XLocItem MenuRedo { get; } = new XLocItem("__GraphEditor_MenuRedo");
            public XLocItem MenuUndo { get; } = new XLocItem("__GraphEditor_MenuUndo");
            public XLocItem NodeCommentPlaceholder { get; } = new XLocItem("__GraphEditor_NodeCommentPlaceholder");
            public XLocItem NodeCommentTitle { get; } = new XLocItem("__GraphEditor_NodeCommentTitle");
            public XLocItem WindowCancel { get; } = new XLocItem("__Dialog_BtnCancel");
            public XLocItem WindowOk { get; } = new XLocItem("__Dialog_BtnOk");
            public XLocItem WindowTitle { get; } = new XLocItem("__GraphEditor_WindowTitle");


            public XLocItem TimeType_Title { get; } = new XLocItem("__GraphEditor_TimeType_Title");
            public XLocItem TimeType_Auto { get; } = new XLocItem("__Enum_TimeType_Auto");
            public XLocItem TimeType_Tick { get; } = new XLocItem("__Enum_TimeType_Tick");
            public XLocItem TimeType_Second { get; } = new XLocItem("__Enum_TimeType_Second");
            public XLocItem TimeType_Minute { get; } = new XLocItem("__Enum_TimeType_Minute");
            public XLocItem TimeType_Hour { get; } = new XLocItem("__Enum_TimeType_Hour");
            public XLocItem TimeType_Day { get; } = new XLocItem("__Enum_TimeType_Day");
            public XLocItem TimeType_Week { get; } = new XLocItem("__Enum_TimeType_Week");
            public XLocItem TimeType_Month { get; } = new XLocItem("__Enum_TimeType_Month");
            public XLocItem TimeType_Year { get; } = new XLocItem("__Enum_TimeType_Year");
            public XLocItem MenuExportPng { get; } = new XLocItem("__GraphEditor_MenuExportPng");

        }
    }
}