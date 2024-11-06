using Avalonia;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class GraphEditorVM : ViewModelBase, IUndoRedoObject
    {
        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();

        [ObservableProperty]
        private bool saved;

        [ObservableProperty]
        private ObservableCollection<NodeViewModel>? selectedNodes = new ObservableCollection<NodeViewModel>();

        public UndoRedoManager UndoRedo { get; }
        public PipelineVM Pipeline { get; }

        public GraphEditorVM()
        {
            Pipeline = new PipelineVM();
            UndoRedo = new UndoRedoManager(this);
        }

        public GraphEditorVM(PipelineVM pipeline)
            : this()
        {
            Pipeline = pipeline;
            Nodes.Add(new CommentNodeViewModel());
        }

        public object SaveState()
        {
            return new object();
        }

        public void RestoreState(object state)
        {

        }

        public byte[]? Copy()
        {
            return new byte[] { 12 };
        }

        public void Paste(byte[] data, Point point)
        {

        }
    }
}
