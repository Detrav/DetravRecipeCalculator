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
        private ConnectorViewModel? source;

        public PendingConnectionVM(GraphEditorVM editor)
        {
            _editor = editor;

            StartCommand = new RelayCommand<ConnectorViewModel>(value => source = value);
            FinishCommand = new RelayCommand<ConnectorViewModel>(target =>
            {
                if (target != null && Source != null && Source.Name == target.Name && Source != target)
                {


                    if (!editor.Connections.Any(m => m.Output == Source && m.Input == target))
                    {
                        if (Source.IsInput && !target.IsInput)
                        {
                            editor.AddConnection(new ConnectionVM(target, Source));
                        }
                        else if (!Source.IsInput && target.IsInput)
                        {
                            editor.AddConnection(new ConnectionVM(Source, target));
                        }
                    }
                }
            });

        }

        public ICommand StartCommand { get; }
        public ICommand FinishCommand { get; }
    }
}
