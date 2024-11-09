using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
            FinishCommand = new RelayCommand<ConnectorVM>(target =>
            {
                if (target != null && Source != null && (Source.Name == target.Name || Source.IsAny || target.IsAny) && Source != target)
                {
                    if (!editor.Connections.Any(m => m.Output == Source && m.Input == target))
                    {
                        if (Source.IsInput && !target.IsInput)
                        {
                            editor.AddConnection(new ConnectionVM(target, Source));
                            editor.UndoRedo.PushState("Add connection");
                        }
                        else if (!Source.IsInput && target.IsInput)
                        {
                            editor.AddConnection(new ConnectionVM(Source, target));
                            editor.UndoRedo.PushState("Add connection");
                        }
                    }
                }
            });
        }

        public ICommand FinishCommand { get; }
        public ICommand StartCommand { get; }
    }
}