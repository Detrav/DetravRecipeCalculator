using Avalonia.Controls;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using MsBox.Avalonia;
using System;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Views;

public partial class MainWindow : Window
{
    private bool shouldClose = false;

    public MainWindow()
    {
        InitializeComponent();
        Closing += MainWindow_Closing;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Config.Instance.SaveState(this);
        Config.Instance.Save();
        base.OnClosing(e);
    }

    protected override void OnOpened(EventArgs e)
    {
        Config.Instance.LoadState(this);
        base.OnOpened(e);
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (shouldClose)
            return;

        if (DataContext is MainVM vm)
        {
            if (vm.Pipeline != null && !vm.Pipeline.Saved)
            {
                e.Cancel = true;
                _ = ShowExitPrompt();
            }
        }
    }

    private async Task ShowExitPrompt()
    {
        switch (await MessageBoxManager.GetMessageBoxStandard(Xloc.Get("__Window_ClosingSave_Title"), Xloc.Get("__Window_ClosingSave_Text"), MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel).ShowDialogAsync(this))
        {
            case MsBox.Avalonia.Enums.ButtonResult.Yes:
                if (Content is MainView mainView)
                {
                    if (await mainView.SavePipelineAsync())
                    {
                        shouldClose = true;
                        Close();
                    }
                }
                break;

            case MsBox.Avalonia.Enums.ButtonResult.No:
                shouldClose = true;
                Close();
                break;
        }
    }
}