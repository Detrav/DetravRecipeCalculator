using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Views;

public partial class GraphEditorWindow : Window
{
    private Point lastClick;

    private string? lastText;

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

    private bool CanDelete()
    {
        return DataContext is GraphEditorVM vm && vm.SelectedNodes != null && vm.SelectedNodes.Any();
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
                        var node = new RecipeNodeVM(vm);
                        node.Location = lastClick;
                        node.RefreshValues(therecipe);
                        vm.Nodes.Add(node);
                        vm.UndoRedo.PushState("Add node " + therecipe.Name);
                    })
                });
            }
        }
    }

    private void GraphEditorWindow_PointerMoved(object? sender, PointerEventArgs e)
    {
        lastClick = Editor.DpiScaledViewportTransform.Value.Invert().Transform(e.GetPosition(Editor));
    }

    private void LineConnection_Disconnect(object? sender, RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm && sender is Control control && control.DataContext is ConnectionVM connectionVM)
        {
            vm.Disconnect(connectionVM);
            vm.UndoRedo.PushState("Disconnect");
            e.Handled = true;
        }
    }

    private void MiAddComment_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            var comment = new CommentNodeVM(vm);
            comment.Location = lastClick;
            vm.Nodes.Add(comment);
            vm.UndoRedo.PushState("Add comment");
        }
    }

    private async void MiCopy_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            var data = vm.Copy(lastClick);
            if (data != null && Clipboard != null && data.Length > 0)
            {
                var dataObject = new DataObject();
                dataObject.Set("GraphEditor_Nodes", data);
                await Clipboard.SetDataObjectAsync(dataObject);
            }
        }
    }

    private async void MiCut_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            var data = vm.Cut(lastClick);
            if (data != null && Clipboard != null && data.Length > 0)
            {
                var dataObject = new DataObject();
                dataObject.Set("GraphEditor_Nodes", data);
                await Clipboard.SetDataObjectAsync(dataObject);
                vm.UndoRedo.PushState("Cut");
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
                    vm.DeleteNode(node);
                }
            }
            vm.UndoRedo.PushState("Delete");
        }
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

    private void MiRedo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.UndoRedo.Redo();
    }

    private void MiUndo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.UndoRedo.Undo();
    }

    private void TextBox_Comment_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
    {
        lastText = (sender as TextBox)?.Text;
    }

    private void TextBox_Comment_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var newText = (sender as TextBox)?.Text;
        if (DataContext is GraphEditorVM vm && lastText != newText)
        {
            vm.UndoRedo.PushState("Change comment");
        }
    }

    private void MenuItem_TimeType_Auto_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Auto;

    }
    private void MenuItem_TimeType_Tick_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Tick;
    }
    private void MenuItem_TimeType_Second_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Second;
    }
    private void MenuItem_TimeType_Minute_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Minute;
    }
    private void MenuItem_TimeType_Hour_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Hour;
    }
    private void MenuItem_TimeType_Day_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Day;
    }
    private void MenuItem_TimeType_Week_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Week;
    }

    private void MenuItem_TimeType_Month_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Month;
    }

    private void MenuItem_TimeType_Year_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
            vm.TimeType = TimeType.Year;
    }

    //private void LineConnection_Disconnect(object? sender, Nodify.ConnectionEventArgs e)
    //{
    //    if (DataContext is GraphEditorVM vm && sender is Control control && control.DataContext is ConnectionVM connectionVM)
    //    {
    //        vm.Disconnect(connectionVM);
    //        e.Handled = true;
    //    }
    //}

    //private void Binding_1(object? sender, Nodify.ConnectionEventArgs e)
    //{
    //}

    //private void LineConnection_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    //{
    //    LineConnection
    //    if ((e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
    //    {
    //        if (DataContext is GraphEditorVM vm && sender is Control control && control.DataContext is ConnectionVM connectionVM)
    //        {
    //            vm.Disconnect(connectionVM);
    //            e.Handled = true;
    //        }
    //    }
    //}
}