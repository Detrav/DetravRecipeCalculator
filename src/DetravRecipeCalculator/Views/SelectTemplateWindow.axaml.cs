using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace DetravRecipeCalculator.Views;

public partial class SelectTemplateWindow : Window
{
    private readonly List<string> templatesJson = new List<string>();

    public string? JsonValue { get; set; }

    public SelectTemplateWindow()
    {
        InitializeComponent();

        Title = Xloc.Get("__SelectTemplateWindow_Title");
        tbCancel.Text = Xloc.Get("__Dialog_BtnCancel");
        tbOk.Text = Xloc.Get("__Dialog_BtnOk");

        AddTemplate("{}", Xloc.Get("__SelectTemplateWindow_Empty"));

        var dir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "templates");

        foreach (var json in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
        {
            AddTemplate(File.ReadAllText(json), Path.GetFileNameWithoutExtension(json));
        }

    }

    private void AddTemplate(string json, string name)
    {
        templatesJson.Add(json);
        tamplates.Items.Add(name);
    }

    private void Button_Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int i = tamplates.SelectedIndex;

        if (i >= 0 && i < templatesJson.Count)
        {
            JsonValue = templatesJson[i];
        }
        Close(true);
    }

    private void Button_Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(false);
    }
}