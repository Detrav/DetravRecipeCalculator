using NodeEditor.Controls;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels.NodeEditorVMs
{
    public class GraphEditorFactoryVM : INodeFactory
    {
        public IDrawingNode CreateDrawing(string? name = null)
        {

            var drawing = new DrawingNodeViewModel();
            var settings = drawing.Settings;
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



            return drawing;
        }

        public IList<INodeTemplate> CreateTemplates()
        {
            return new ObservableCollection<INodeTemplate>()
            {
                //new NodeTemplateViewModel
                //{
                //    Title = "Begin",
                //    Template = CreateBegin(0, 0, 60, 60, "Begin"),
                //    Preview = CreateBegin(0, 0, 60, 60, "Begin")
                //},
                //new NodeTemplateViewModel
                //{
                //    Title = "End",
                //    Template = CreateEnd(0, 0, 60, 60, "End"),
                //    Preview = CreateEnd(0, 0, 60, 60, "End")
                //},
            };
        }

     
    }
}
