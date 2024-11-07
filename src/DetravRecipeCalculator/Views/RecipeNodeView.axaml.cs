using Avalonia.Controls;
using DetravRecipeCalculator.ViewModels;

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