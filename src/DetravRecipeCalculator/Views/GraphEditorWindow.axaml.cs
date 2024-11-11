using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using Nodify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.VisualTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Windows.Markup;
using System.Xml.Linq;
using Avalonia.Platform.Storage;
using org.matheval.Implements;

namespace DetravRecipeCalculator.Views;

public partial class GraphEditorWindow : Window
{
    private Point lastClick;

    private string? lastText;

    public GraphEditorWindow()
    {
        InitializeComponent();
        ContextFlyout!.Opened += ContextFlyout_Opened;

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
        Config.Instance.SaveState(this);
        base.OnClosing(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Config.Instance.LoadState(this);
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
        else if (e.Key == Key.F1)
        {
            var url = Xloc.Get("__LanguageHelpUrl");
            Utils.ExpressionUtils.ShowDocumentation(url);
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




        if (Editor.IsMouseCaptureWithin)
        {
            FixPan();
        }


        //if(Editor.IsMouseCaptureWithin)
        //    Editor.IsMouseCaptureWithin = false;
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
        foCreateNode.ClearNodes();

        if (DataContext is GraphEditorVM vm)
        {

            vm.PendingConnection.ShowFlyoutMenu = model =>
            {
                if (ContextFlyout is Flyout flyout)
                {
                    foCreateNode.ConnectorContext = model.Source;
                    flyout.ShowAt(this, true);
                }

            };

            foCreateNode.RegisterNode(Xloc.Get("__CreateNode_Splitter"), () =>
            {
                var node = new SplitConnectorNodeVM(vm, null);
                node.Location = lastClick;
                vm.Nodes.Add(node);
                vm.UndoRedo.PushState("Add split");
                return node;
            }, NodeViewModelFactory.GetPinDiscrimantors<SplitConnectorNodeVM>() );
            foCreateNode.RegisterNode(Xloc.Get("__CreateNode_ResultTable"), () =>
            {
                var node = new ResultTableNodeVM(vm, true);
                node.Location = lastClick;
                vm.Nodes.Add(node);
                vm.UndoRedo.PushState("Add Result table");
                return node;
            }, NodeViewModelFactory.GetPinDiscrimantors<ResultTableNodeVM>());

            foCreateNode.RegisterNode(Xloc.Get("__CreateNode_Comment"), () =>
            {
                var comment = new CommentNodeVM(vm);
                comment.Location = lastClick;
                vm.Nodes.Add(comment);
                vm.UndoRedo.PushState("Add comment");
                return comment;
            }, NodeViewModelFactory.GetPinDiscrimantors<CommentNodeVM>());

            foCreateNode.RegisterNode(Xloc.Get("__CreateNode_SubGraph"), () =>
            {
                var comment = new SubGraphNodeVM(vm);
                comment.Location = lastClick;
                vm.Nodes.Add(comment);
                vm.UndoRedo.PushState("Add subgraph");
                return comment;
            }, NodeViewModelFactory.GetPinDiscrimantors<SubGraphNodeVM>());

            foreach (var recipe in vm.Pipeline.Recipes)
            {
                if (recipe.Id == null)
                {
                    recipe.Id = Guid.NewGuid().ToString();
                }

                var therecipe = recipe;

                foCreateNode.RegisterNode(Xloc.Get("__CreateNode_Recipe") + therecipe.Name, () =>
                {
                    var node = new RecipeNodeVM(vm, therecipe);
                    node.Location = lastClick;
                    vm.Nodes.Add(node);
                    vm.UndoRedo.PushState("Add node " + therecipe.Name);
                    return node;
                }, NodeViewModelFactory.GetPinDiscrimantors<RecipeNodeVM>(therecipe));
            }

            
        }
    }

    private void GraphEditorWindow_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (ContextFlyout == null || !ContextFlyout.IsOpen)
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
        {
            vm.TimeType = TimeType.Auto;
            vm.UndoRedo.PushState("Update time");
        }

    }
    private void MenuItem_TimeType_Tick_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Tick;
            vm.UndoRedo.PushState("Update time");
        }
    }
    private void MenuItem_TimeType_Second_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Second;
            vm.UndoRedo.PushState("Update time");
        }
    }
    private void MenuItem_TimeType_Minute_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Minute;
            vm.UndoRedo.PushState("Update time");
        }
    }
    private void MenuItem_TimeType_Hour_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Hour;
            vm.UndoRedo.PushState("Update time");
        }
    }
    private void MenuItem_TimeType_Day_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Day;
            vm.UndoRedo.PushState("Update time");
        }
    }
    private void MenuItem_TimeType_Week_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Week;
            vm.UndoRedo.PushState("Update time");
        }
    }

    private void MenuItem_TimeType_Month_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Month;
            vm.UndoRedo.PushState("Update time");
        }
    }

    private void MenuItem_TimeType_Year_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is GraphEditorVM vm)
        {
            vm.TimeType = TimeType.Year;
            vm.UndoRedo.PushState("Update time");
        }
    }




    private void ExportToPNG(string fileName)
    {
        //LineConnection
        //ItemContainer

        //NodifyEditor editor = Editor;
        //editor.ClipToBounds = false;

        //var zoom = editor.ViewportZoom;
        //var location = editor.ViewportLocation;

        //var extent = editor.ItemsExtent;
        //editor.ViewportZoom = 0.1;
        //editor.ViewportLocation = extent.Position - new Point(15, 15);

        //using var bitmap = new RenderTargetBitmap(new PixelSize((int)(extent.Width + 30), (int)(extent.Height + 30)), new Vector(96, 96) * 10);


        //renderElement.Measure

        //bitmap.Render(renderElement);
        //bitmap.Save("test.png");


        //editor.ViewportZoom = zoom;
        //editor.ViewportLocation = location;

        //Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

        NodifyEditor editor = Editor;
        var zoom = editor.ViewportZoom;
        var location = editor.ViewportLocation;

        var extent = editor.ItemsExtent;
        editor.ViewportZoom = 1;
        editor.ViewportLocation = extent.Position - new Point(15, 15);
        var savedBounds = renderElement.Bounds;

        renderElement.Measure(new Size(extent.Width + 200, extent.Height + 200));
        renderElement.Arrange(new Rect(0, 0, extent.Width + 200, extent.Height + 200));

        using var bitmap = new RenderTargetBitmap(new PixelSize((int)(extent.Width + 30), (int)(extent.Height + 30)));
        bitmap.Render(renderElement);
        bitmap.Save(fileName);

        editor.ViewportZoom = zoom;
        editor.ViewportLocation = location;
        renderElement.Arrange(savedBounds);
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    private async void MenuItem_ExportPng_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        var fpso = new FilePickerSaveOptions()
        {
            DefaultExtension = ".png",
            FileTypeChoices = new[] { FilePickerFileTypes.ImagePng },
            ShowOverwritePrompt = true,
            SuggestedFileName = "pipeline",
            Title = Xloc.Get("__Window_File_SaveAsTitle")
        };

        var tl = TopLevel.GetTopLevel(this);

        if (tl != null)
        {
            //fpso.SuggestedStartLocation = await tl.StorageProvider.TryGetFolderFromPathAsync(lastFolder ?? "");

            var file = await tl.StorageProvider.SaveFilePickerAsync(fpso);

            if (file != null)
            {
                try
                {

                    var path = file.TryGetLocalPath();
                    ExportToPNG(path!);
                }
                catch (Exception ex)
                {
                    await MessageBoxExtentions.ShowErrorAsync(ex, this);
                }
            }
        }

    }

    private void LineConnection_Split(object? sender, RoutedEventArgs e)
    {
        if (e is ConnectionEventArgs args)
        {
            var location = args.SplitLocation;

            if (args.Connection is ConnectionVM cvm)
            {
                if (!cvm.Input.IsAny && !cvm.Output.IsAny)
                {
                    var graph = cvm.Input.Parent.Parent;

                    var middleNode = new SplitConnectorNodeVM(graph, cvm.Input.Name);
                    middleNode.Location = location + new Point(-20, -10);
                    var connection1 = new ConnectionVM(cvm.Output, middleNode.Input.First());
                    var connection2 = new ConnectionVM(middleNode.Output.First(), cvm.Input);

                    graph.Nodes.Add(middleNode);

                    graph.Disconnect(cvm);
                    graph.AddConnection(connection1);
                    graph.AddConnection(connection2);

                    if (graph.SelectedNodes != null)
                    {
                        graph.SelectedNodes.Clear();
                        graph.SelectedNodes.Add(middleNode);

                        // i hate components


                        FixPan();
                    }

                    graph.UndoRedo.PushState("Add split");

                }

            }
        }

        e.Handled = true;
    }

    private void FixPan()
    {
        Editor.GetType().GetProperty(nameof(NodifyEditor.IsMouseCaptureWithin))!.SetValue(Editor, false);
    }

    private void Flyout_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
    }





    //private IEnumerable<T> GetChildren<T>(IAvaloniaList<Visual> visualChildren)
    //    where T : Visual
    //{
    //    foreach (var ch in visualChildren)
    //    {
    //        if (ch is T tReuslt)
    //        {
    //            yield return tReuslt;
    //        }
    //        else
    //        {
    //            foreach(var chch in GetChildren(ch.VisualChildren))
    //        }

    //    }
    //}

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
    //}\


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