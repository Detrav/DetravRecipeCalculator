using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using Nodify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Views;

public partial class GraphEditorWindow : Window
{
    Point lastClick;

    public GraphEditorWindow()
    {
        InitializeComponent();

        ContextFlyout!.Opened += ContextFlyout_Opened;

        miAddComment.Click += MiAddComment_Click;
        PointerMoved += GraphEditorWindow_PointerMoved;
        miDelete.Click += MiDelete_Click;
        miCopy.Click += MiCopy_Click;
        miCut.Click += MiCut_Click;
        miPaste.Click += MiPaste_Click;
        miRedo.Click += MiRedo_Click;
        miUndo.Click += MiUndo_Click;

        DataContextChanged += GraphEditorWindow_DataContextChanged;
    }

    private void GraphEditorWindow_DataContextChanged(object? sender, EventArgs e)
    {
        miAddNode.Items.Clear();

        if (DataContext is GraphEditorVM vm)
        {
            foreach (var recipe in vm.Pipeline.Recipes)
            {
                if (recipe.Id == null)
                {
                    recipe.Id = Guid.NewGuid().ToString();
                }

                var therecipe = recipe;
                miAddNode.Items.Add(new MenuItem()
                {
                    Header = therecipe.Name ?? "Unknown",
                    Command = new RelayCommand(() =>
                    {
                        var node = new RecipeNodeVM();
                        node.Location = lastClick;
                        node.RefreshValues(vm.Pipeline, therecipe);
                        vm.Nodes.Add(node);
                    })
                });
            }
        }
    }

    private void MiUndo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.UndoRedo.Undo();
    }

    private void MiRedo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.UndoRedo.Redo();
    }

    private async void MiPaste_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm && Clipboard != null && (await Clipboard.GetFormatsAsync()).Contains("GraphEditor_Nodes"))
        {
            try
            {
                var data = await Clipboard.GetDataAsync("GraphEditor_Nodes") as byte[];

                if (data != null)
                {
                    vm.Paste(data, lastClick);
                    vm.UndoRedo.PushState("Paste");
                }
            }
            catch (Exception ex)
            {
                await MessageBoxExtentions.ShowErrorAsync(ex, this);
            }
        }
    }

    private void MiCut_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.UndoRedo.PushState("Cut");
        }
    }

    private async void MiCopy_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            var data = vm.Copy();
            if (data != null && Clipboard != null && data.Length > 0)
            {
                var dataObject = new DataObject();
                dataObject.Set("GraphEditor_Nodes", data);
                await Clipboard.SetDataObjectAsync(dataObject);
            }
        }
    }

    private void MiDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            if (vm.SelectedNodes != null)
            {
                var nodes = vm.SelectedNodes.ToArray();

                foreach (var node in nodes)
                {
                    vm.Nodes.Remove(node);
                }
            }
            vm.UndoRedo.PushState("Delete");
        }
    }

    private void GraphEditorWindow_PointerMoved(object? sender, PointerEventArgs e)
    {
        lastClick = Editor.DpiScaledViewportTransform.Value.Invert().Transform(e.GetPosition(Editor));
    }

    private void MiAddComment_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            var comment = new CommentNodeViewModel();
            comment.Location = lastClick;
            vm.Nodes.Add(comment);
        }
    }



    private async void ContextFlyout_Opened(object? sender, System.EventArgs e)
    {

        if (DataContext is GraphEditorVM vm)
        {
            miUndo.IsEnabled = vm.UndoRedo.CanUndo;
            miRedo.IsEnabled = vm.UndoRedo.CanRedo;
        }
        else
            miUndo.IsEnabled = miRedo.IsEnabled = false;

        miCopy.IsEnabled = CanCopy();
        miCut.IsEnabled = CanCut();
        miDelete.IsEnabled = CanDelete();
        miPaste.IsEnabled = await CanPaste();

    }

    private bool CanDelete()
    {
        return DataContext is GraphEditorVM vm && vm.SelectedNodes != null && vm.SelectedNodes.Any();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Handled)
            return;
        bool Match(List<KeyGesture> gestures) => gestures.Any(g => g.Matches(e));

        var keymap = Application.Current!.PlatformSettings!.HotkeyConfiguration;

        if (Match(keymap.Undo))
        {
            MiUndo_Click(miUndo, e);
            e.Handled = true;
        }
        else if (Match(keymap.Redo))
        {
            MiRedo_Click(miRedo, e);
            e.Handled = true;
        }
        else if (Match(keymap.Copy))
        {
            MiCopy_Click(miCopy, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            MiDelete_Click(miDelete, e);
            e.Handled = true;
        }
        else if (Match(keymap.Paste))
        {
            MiPaste_Click(miPaste, e);
            e.Handled = true;
        }
        else if (Match(keymap.Cut))
        {
            MiCut_Click(miCut, e);
            e.Handled = true;
        }
        base.OnKeyUp(e);
    }

    public bool CanCopy()
    {

        return DataContext is GraphEditorVM vm && vm.SelectedNodes != null && vm.SelectedNodes.Any();
    }

    public bool CanCut()
    {
        return DataContext is GraphEditorVM vm && vm.SelectedNodes != null && vm.SelectedNodes.Any();
    }

    public async Task<bool> CanPaste()
    {
        return Clipboard != null && (await Clipboard.GetFormatsAsync()).Contains("GraphEditor_Nodes");
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {

        if (!e.Handled && e.InitialPressMouseButton == MouseButton.Right)
        {
            ContextRequestedEventArgs contextRequestedEventArgs = new ContextRequestedEventArgs(e);
            RaiseEvent(contextRequestedEventArgs);
            e.Handled = contextRequestedEventArgs.Handled;
        }

        if (!e.Handled)
            base.OnPointerReleased(e);
    }
}