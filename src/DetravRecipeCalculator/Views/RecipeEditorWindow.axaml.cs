using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using MsBox.Avalonia;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DetravRecipeCalculator.Views;

public partial class RecipeEditorWindow : Window
{
    public RecipeEditorWindow()
    {
        InitializeComponent();
    }

    public IEnumerable<string>? ResourceNames { get; set; }

    public AutoCompleteFilterPredicate<string> TextFilter { get; }
        = (search, item) => String.IsNullOrEmpty(search) || search.Length < 2 || item.Contains(search, StringComparison.OrdinalIgnoreCase);

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

    private void Button_AddInput_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is RecipeVM vm)
        {
            vm.Input.Add(new ResourceValueVM());
        }
    }

    private void Button_AddOutput_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is RecipeVM vm)
        {
            vm.Output.Add(new ResourceValueVM());
        }
    }

    private void Button_Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(false);
    }

    private void Button_ExpressionHelp_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var url = Xloc.Get("__Expressions_Help");
        Utils.ExpressionUtils.ShowDocumentation(url);
    }

    private void Button_IconDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is RecipeVM vm)
        {
            vm.Icon = null;
        }
    }

    private async void Button_IconOpen_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is RecipeVM vm)
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

    private void Button_InputDeleteResource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ResourceValueVM itemVM && DataContext is RecipeVM vm)
        {
            vm.Input.Remove(itemVM);
        }
    }

    private void Button_InputMoveDown_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ResourceValueVM itemVM && DataContext is RecipeVM vm)
        {
            var index = vm.Input.IndexOf(itemVM);
            if (index < vm.Input.Count - 1)
            {
                vm.Input[index] = vm.Input[index + 1];
                vm.Input[index + 1] = itemVM;
            }
        }
    }

    private void Button_InputMoveUp_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ResourceValueVM itemVM && DataContext is RecipeVM vm)
        {
            var index = vm.Input.IndexOf(itemVM);

            if (index > 0)
            {
                vm.Input[index] = vm.Input[index - 1];
                vm.Input[index - 1] = itemVM;
            }
        }
    }

    private void Button_Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(true);
    }

    private void Button_OutputDeleteResource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ResourceValueVM itemVM && DataContext is RecipeVM vm)
        {
            vm.Output.Remove(itemVM);
        }
    }

    private void Button_OutputMoveDown_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ResourceValueVM itemVM && DataContext is RecipeVM vm)
        {
            var index = vm.Output.IndexOf(itemVM);
            if (index < vm.Output.Count - 1)
            {
                vm.Output[index] = vm.Output[index + 1];
                vm.Output[index + 1] = itemVM;
            }
        }
    }

    private void Button_OutputMoveUp_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ResourceValueVM itemVM && DataContext is RecipeVM vm)
        {
            var index = vm.Output.IndexOf(itemVM);

            if (index > 0)
            {
                vm.Output[index] = vm.Output[index - 1];
                vm.Output[index - 1] = itemVM;
            }
        }
    }

    private async void Button_PasteIcon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is RecipeVM vm)
        {
            var newIcon = await ClipboardHelper.GetImageAsync(Clipboard);
            if (newIcon != null)
            {
                vm.Icon = newIcon;
            }
        }
    }

    private async void Button_SelectIcon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var wnd = new SelectIconWindow();

        if (await wnd.ShowDialog<bool>(this))
        {
            if (DataContext is RecipeVM vm)
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