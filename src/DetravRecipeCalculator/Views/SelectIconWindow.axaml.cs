using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;

namespace DetravRecipeCalculator.Views;

public partial class SelectIconWindow : Window
{
    public SelectIconWindow()
    {
        InitializeComponent();
        DataContext = SelectIconVM.Instance;
    }

    public SelectIconItemVM? Result { get; set; }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        Config.Instance.LoadSate(this);
        base.OnLoaded(e);
    }
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Config.Instance.SaveSate(this);
        base.OnClosing(e);
    }

    private void Button_Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(false);
    }

    private void Button_Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(true);
    }

    private void Grid_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Grid grid && DataContext is SelectIconVM vm && grid.DataContext is SelectIconItemVM itemvm)
        {
            foreach (var item in vm.Icons)
            {
                item.IsSelected = false;
            }

            itemvm.IsSelected = true;

            Result = itemvm;
        }
    }
}