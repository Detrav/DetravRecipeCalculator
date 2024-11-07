using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DetravRecipeCalculator.ViewModels;
using Nodify;

namespace DetravRecipeCalculator.Views;

public partial class RecipeNodeView : UserControl
{
    public RecipeNodeView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            this.DataContext = RecipeNodeVM.DebugInstance;
        }
    }
}