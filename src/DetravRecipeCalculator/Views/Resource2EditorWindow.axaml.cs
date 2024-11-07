using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;

namespace DetravRecipeCalculator.Views;

public partial class Resource2EditorWindow : Window
{
    public Resource2EditorWindow()
    {
        InitializeComponent();
    }

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

    private void Button_IconDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ResourceVM vm)
        {
            vm.Icon = null;
        }
    }

    private async void Button_IconOpen_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ResourceVM vm)
        {
            var fpoo = new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImagePng, FilePickerFileTypes.All },
                Title = Xloc.Get("__Window_File_OpenTitle")
            };

            var file = (await StorageProvider.OpenFilePickerAsync(fpoo)).FirstOrDefault();
            if (file != null)
            {
                try
                {
                    var bmp = File.ReadAllBytes(file.TryGetLocalPath()!);
                    SKBitmap.Decode(bmp).Dispose();

                    vm.Icon = bmp;
                }
                catch (Exception ex)
                {
                    await MessageBoxExtentions.ShowErrorAsync(ex, this);
                }
            }
        }
    }

    private async void Button_IconPaste_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ResourceVM vm)
        {
            var newIcon = await ClipboardHelper.GetImageAsync(Clipboard);
            if (newIcon != null)
            {
                vm.Icon = newIcon;
            }
        }
    }

    private void Button_Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(true);
    }

    private async void Button_SelectIcon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var wnd = new SelectIconWindow();

        if (await wnd.ShowDialog<bool>(this))
        {
            if (DataContext is ResourceVM vm)
            {
                try
                {
                    var bmp = File.ReadAllBytes(wnd.Result!.Path!);
                    SKBitmap.Decode(bmp).Dispose();
                    vm.Icon = bmp;
                }
                catch (Exception ex)
                {
                    await MessageBoxExtentions.ShowErrorAsync(ex, this);
                }
            }
        }
    }
}