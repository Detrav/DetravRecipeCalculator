using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DetravRecipeCalculator.ViewModels;

namespace DetravRecipeCalculator.Views;

public partial class ResultTableNodeView : UserControl
{
    public ResultTableNodeView()
    {
        InitializeComponent();
    }

    private async void Button_Edit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {


        if (TopLevel.GetTopLevel(this) is Window wnd && DataContext is ResultTableNodeVM vm)
        {
            var editor = new EditResultTableSettingsView(vm);
            await editor.ShowDialog(wnd);
        }
    }
}