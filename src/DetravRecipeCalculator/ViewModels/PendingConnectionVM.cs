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
            var source = Source;
            if (target != null && source != null && source.IsInput == (!target.IsInput) && (source.Name == target.Name || source.IsAny == (!target.IsAny)))
            {
                if (source.IsInput) Connect(target, source);
                else Connect(source, target);
            }

        }

        private void Connect(ConnectorVM output, ConnectorVM input)
        {
            if (output.IsAny) output = output.Parent.GetReplacementFor(output, input);
            else if (input.IsAny) input = input.Parent.GetReplacementFor(input, output);

            if (!_editor.Connections.Any(m => m.Output == output && m.Input == input))
            {
                var connection = new ConnectionVM(output, input);

                _editor.AddConnection(connection);
                _editor.UndoRedo.PushState("Add connection");
            }
        }

        public ICommand FinishCommand { get; }
        public ICommand StartCommand { get; }
    }
}