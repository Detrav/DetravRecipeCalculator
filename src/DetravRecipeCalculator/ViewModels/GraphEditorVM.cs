using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class GraphEditorVM : ViewModelBase
    {
        public EditorViewModel Editor { get; } = new EditorViewModel();

        public GraphEditorVM()
        {
            var drawing = Editor.Drawing = new DrawingNodeViewModel();
            var settings = Editor.Drawing.Settings = new DrawingNodeSettingsViewModel();
            settings.SnapX = 15;
            settings.SnapY = 15;
            settings.EnableSnap = true;
            settings.EnableGrid = true;
            settings.EnableMultiplePinConnections = true;
            settings.EnableGrid = true;
            settings.GridCellHeight = 15;
            settings.GridCellWidth = 15;

            drawing.Name = "test";
            drawing.X = 0;
            drawing.Y = 0;
            drawing.Width = 9000;
            drawing.Height = 6000;
            drawing.Nodes = new ObservableCollection<INode>();
            drawing.Connectors = new ObservableCollection<IConnector>();

        }

    }
}
