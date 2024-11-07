using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using MsBox.Avalonia;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace DetravRecipeCalculator.Views;

public partial class SelectIconWindow : Window
{
    private const string ICONS_PACK_FOLDER = "game-icons";

    private const string ICONS_PACK_URL = "https://game-icons.net/archives/png/zip/ffffff/transparent/game-icons.net.png.zip";

    public SelectIconWindow()
    {
        InitializeComponent();
        DataContext = SelectIconVM.Instance;
        Opened += SelectIconWindow_Opened;
    }

    public SelectIconItemVM? Result { get; set; }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Config.Instance.SaveSate(this);
        base.OnClosing(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Config.Instance.LoadSate(this);
        base.OnLoaded(e);
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

    private async void SelectIconWindow_Opened(object? sender, EventArgs e)
    {
        var targetDir = Path.Combine(Config.Instance.AppDataDirectory, ICONS_PACK_FOLDER);
        if (Directory.Exists(targetDir))
            return;
        if (Design.IsDesignMode)
            return;

        if (DataContext is SelectIconVM vm)
        {
            try
            {
                IsEnabled = false;
                var result = await MessageBoxManager.GetMessageBoxStandard(Xloc.Get("__SelectIcon_DownloadPrompt"), Xloc.Get("__SelectIcon_DownloadText"), MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowDialogAsync(this);

                if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    using var client = new HttpClient();
                    var zip = await client.GetByteArrayAsync(ICONS_PACK_URL);
                    using var ms = new MemoryStream(zip);
                    Directory.CreateDirectory(targetDir);
                    ZipFile.ExtractToDirectory(ms, targetDir);
                    vm.ReloadIcons();
                }
            }
            catch (Exception ex)
            {
                await MessageBoxExtentions.ShowErrorAsync(ex, this);
            }
            finally
            {
                IsEnabled = true;
            }
        }
    }
}