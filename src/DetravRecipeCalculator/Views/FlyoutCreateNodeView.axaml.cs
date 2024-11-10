using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DetravRecipeCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static DetravRecipeCalculator.Utils.NodeViewModelFactory;

namespace DetravRecipeCalculator.Views;

public partial class FlyoutCreateNodeView : UserControl
{
    public FlyoutCreateNodeView()
    {
        InitializeComponent();
        cbContextFilter.IsChecked = true;
        cbContextFilter.IsCheckedChanged += CbContextFilter_IsCheckedChanged;
        tbFilter.TextChanged += TbFilter_TextChanged;
        lbNodes.Background = Brushes.Transparent;
    }

    readonly List<FlyoutCreateNodeItemVM> items = new List<FlyoutCreateNodeItemVM>();
    List<FlyoutCreateNodeItemVM> itemsFiltered = new List<FlyoutCreateNodeItemVM>();

    public ConnectorVM? ConnectorContext { get; set; }


    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Dispatcher.UIThread.Post(() => Reset(), DispatcherPriority.Background);



    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ConnectorContext = null;
    }





    private void TbFilter_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateFilters();
    }

    private void CbContextFilter_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateFilters();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (Apply())
                e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (lbNodes.SelectedIndex > 0)
                lbNodes.SelectedIndex -= 1;
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            if (lbNodes.SelectedIndex < lbNodes.ItemCount - 1)
                lbNodes.SelectedIndex += 1;
            e.Handled = true;
        }
        //else if (e.Key == Key.Escape)
        //{
        //    var parent = FindParent<Popup>(this);
        //    if (parent != null)
        //    {
        //        parent.Close();
        //    }
        //    e.Handled = true;
        //}
    }

    private bool Apply()
    {
        if (lbNodes.SelectedItem is FlyoutCreateNodeItemVM item)
        {
            var node = item.Action();

            if (node != null)
            {
                node.Parent.PendingConnection.TryConnect(ConnectorContext, node);
            }
        }

        var parent = FindParent<Popup>(this);
        if (parent != null)
        {
            parent.Close();
        }
        return true;
    }

    private T? FindParent<T>(StyledElement? element)
        where T : StyledElement
    {
        if (element == null)
            return default;
        if (element is T result)
            return result;
        return FindParent<T>(element.Parent);
    }

    private void Reset()
    {
        tbFilter.Text = "";
        tbFilter.Focus();
        UpdateFilters();
    }

    private void UpdateFilters()
    {
        var textFiltered = tbFilter.Text?.Trim();
        var query = items.AsQueryable();

        if (!String.IsNullOrWhiteSpace(textFiltered))
            query = query.Where(m => m.Title.Contains(textFiltered, StringComparison.OrdinalIgnoreCase));

        if (cbContextFilter.IsChecked.GetValueOrDefault())
        {
            if (ConnectorContext != null)
            {
                bool isInput = ConnectorContext is ConnectorInVM;
                if (ConnectorContext.IsAny)
                {
                    query = query.Where(m => m.PinDiscriminators.Any(m => (m.IsInput != isInput) && !m.IsAny));
                }
                else
                {
                    query = query.Where(m => 
                    m.PinDiscriminators.Any(m => (m.IsInput != isInput) && (m.IsAny || m.Name == ConnectorContext.Name)));
                }
            }
        }

        lbNodes.ItemsSource = itemsFiltered = query.ToList();
    }

    public void RegisterNode(string title, Func<NodeVM> action, IEnumerable<PinDiscriminator> pinDiscriminators)
    {
        items.Add(new FlyoutCreateNodeItemVM(title, action, pinDiscriminators));
    }

    internal void ClearNodes()
    {
        items.Clear();
    }



    private void Grid_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.ClickCount > 1)
        {
            if (Apply())
                e.Handled = true;
        }


    }
}