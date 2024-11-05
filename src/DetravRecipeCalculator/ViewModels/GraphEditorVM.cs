using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.ViewModels.NodeEditorVMs;
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
            Editor.Factory = new GraphEditorFactoryVM();
            Editor.Drawing = Editor.Factory.CreateDrawing();
            Editor.Templates = Editor.Factory.CreateTemplates();
        }

        public GraphEditorVM(PipelineVM pipeline)
            : this()
        {
            foreach (var item in pipeline.Recipes)
            {
                if (item.IsEnabled)
                {
                    var template = new NodeTemplateViewModel();
                    Editor.Templates!.Add(template);

                    template.Title = item.Name;
                    var node = new NodeViewModel();

                    //node.Name = item.Name;
                    node.Width = 60;
                    node.Height = 60;

                    node.Content = item.Name;

                    int i = 0;

                    foreach (var inputResource in item.Input)
                    {
                        node.AddPin(0, 15 * i, 15, 15, PinAlignment.Left, inputResource.Name);
                        i++;
                    }

                    foreach (var outputResource in item.Output)
                    {
                        node.AddPin(10, 15 * i, 15, 15, PinAlignment.Right, outputResource.Name);
                        i++;
                    }

                    template.Preview = template.Template = node;
                }
            }
        }

    }
}
