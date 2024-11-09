using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using Nodify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DetravRecipeCalculator.Views;

public partial class ResultTableView : UserControl
{

    public ResultTableView()
    {
        InitializeComponent();


    }

    FlatTreeDataGridSource<ResultTableRow>? source;

    protected override void OnDataContextChanged(EventArgs e)
    {


        if (DataContext is ResultDataTable table)
        {
            if (source == null)
            {
                source = new FlatTreeDataGridSource<ResultTableRow>(table.Rows);
                dbGrid.Source = source;
            }
            else
            {
                source.Items = table.Rows;
            }

            foreach (var column in table.Columns)
            {

                var c = FirstOrDefault(source.Columns, column);
                if (c == null)
                {
                    var currentColumn = column;

                    c = new TemplateColumn<ResultTableRow>(column.DisplayName, new FuncDataTemplate<ResultTableRow>((model, ns) =>
                    {
                        return new ContentControl()
                        {
                            Content = model?.GetValue(currentColumn.Index) is ResultTableItemParameter parameter ? parameter.Value : null,
                        };
                    }), options: new TemplateColumnOptions<ResultTableRow>()
                    {
                        CompareAscending = (x, y) => CompareColumnAscending(x, y, currentColumn.Index),
                        CompareDescending = (x, y) => CompareColumnDescending(x, y, currentColumn.Index),
                        CanUserSortColumn = true,
                        CanUserResizeColumn = true,
                    });
                    c.Tag = column;
                    source.Columns.Add(c);
                }
                else
                {
                    c.Header = column.DisplayName;
                }
            }

            for (int i = 0; i < source.Columns.Count; i++)
            {
                var c = source.Columns[i];
                var column = table.Columns.FirstOrDefault(m => c is TemplateColumn<ResultTableRow> result && c.Tag is ResultTableColumn oldColumn && oldColumn.Index == m.Index);
                if (column == null)
                {
                    source.Columns.RemoveAt(i);
                    i--;
                }
            }
        }

        base.OnDataContextChanged(e);
    }

    private int CompareColumnDescending(ResultTableRow? x, ResultTableRow? y, int index)
    {
        return -CompareColumnAscending(x, y, index);
    }

    private int CompareColumnAscending(ResultTableRow? x, ResultTableRow? y, int index)
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;

        var v1 = x.GetValue(index);
        var v2 = y.GetValue(index);

        if (v1 == null) return v2 == null ? 0 : -1;
        if (v2 == null) return 1;

        if (v1 is double doubleValue1 && v2 is double doubleValue2)
            return Comparer<double>.Default.Compare(doubleValue1, doubleValue2);

        if (v1 is ViewModels.RowNameVM name1 && v2 is ViewModels.RowNameVM name2)
            return Comparer<string>.Default.Compare(name1.Name, name2.Name);

        return 0;
    }

    private TemplateColumn<ResultTableRow>? FirstOrDefault(ColumnList<ResultTableRow> columns, ResultTableColumn column)
    {
        foreach (var c in columns)
        {
            if (c is TemplateColumn<ResultTableRow> result && c.Tag is ResultTableColumn oldColumn && oldColumn.Index == column.Index)
                return result;
        }
        return null;
    }
}