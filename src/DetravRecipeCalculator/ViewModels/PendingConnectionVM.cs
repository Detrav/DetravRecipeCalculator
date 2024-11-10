using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class PendingConnectionVM : ViewModelBase
    {
        private readonly GraphEditorVM _editor;

        [ObservableProperty]
        private ConnectorVM? source;

        public PendingConnectionVM(GraphEditorVM editor)
        {
            _editor = editor;

            StartCommand = new RelayCommand<ConnectorVM>(value => source = value);
            FinishCommand = new RelayCommand<ConnectorVM>(FinishConnect);
        }
        private void FinishConnect(ConnectorVM? target)
        {

            var connection = CanAddConnectionWithCheck(Source, target, _editor.Connections);

            if (connection != null)
            {
                _editor.AddConnection(connection);
                _editor.UndoRedo.PushState("Add connection");
            }



        }

        private static ConnectionVM? Connect(ConnectorOutVM output, ConnectorInVM input, IEnumerable<ConnectionVM> connections)
        {
            if (input.Connection == null)
            {

                if (output.IsAny) output = output.Parent.GetReplacementFor(output, input);
                else if (input.IsAny) input = input.Parent.GetReplacementFor(input, output);

                if (!connections.Any(m => m.Output == output && m.Input == input))
                {
                    var connection = new ConnectionVM(output, input);

                    return connection;
                }
            }

            return null;
        }


        internal static ConnectionVM? CanAddConnectionWithCheck(ConnectorVM? source, ConnectorVM? target, IEnumerable<ConnectionVM> connections)
        {
            if (target != null && source != null && (!string.IsNullOrWhiteSpace(source.Name) && source.Name == target.Name || source.IsAny == (!target.IsAny)))
            {
                if (source is ConnectorInVM inVM1 && target is ConnectorOutVM outVM1)
                    return Connect(outVM1, inVM1, connections);
                if (target is ConnectorInVM inVM2 && source is ConnectorOutVM outVM2)
                    return Connect(outVM2, inVM2, connections);
            }

            return null;
        }

        public ICommand FinishCommand { get; }
        public ICommand StartCommand { get; }
    }
}