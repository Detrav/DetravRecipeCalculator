using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Views;

public partial class SubGraphNodeView : UserControl
{
    public SubGraphNodeView()
    {
        InitializeComponent();
        tbEdit.Text = Xloc.Get("__SubGraphNode_Edit");
    }

    private async void Button_Edit_Click(object? sender, RoutedEventArgs e)
    {

        if (DataContext is SubGraphNodeVM vm && TopLevel.GetTopLevel(this) is Window owner)
        {
            var graph = vm.LoadSubGraph();
            

            var wnd = new GraphEditorWindow()
            {
                DataContext = graph,
            };

            if (await wnd.ShowDialog<bool>(owner))
            {
                vm.SaveSubGraph(graph);
                vm.Parent.UndoRedo.PushState("Update subgraph");
            }
        }
    }
}