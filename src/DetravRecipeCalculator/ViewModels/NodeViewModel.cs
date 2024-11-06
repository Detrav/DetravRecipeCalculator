using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;


namespace DetravRecipeCalculator.ViewModels
{
    public abstract partial class NodeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? title;
        [ObservableProperty]
        private Point? location;
    }
}
