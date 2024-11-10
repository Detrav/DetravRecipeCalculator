using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace DetravRecipeCalculator.Views;

public partial class EditResultTableSettingsView : Window
{
    List<ResourceRequestEditorVM> items = new List<ResourceRequestEditorVM>();
    public ResultTableNodeVM? Model { get; }

    public EditResultTableSettingsView()
    {
        InitializeComponent();

        tbBtnOk.Text = Xloc.Get("__Dialog_BtnOk");
        tbBtnCancel.Text = Xloc.Get("__Dialog_BtnCancel");

        tbNotes.Text = Xloc.Get("__EditResultTableSettingsWindow_Warning");
        Title = Xloc.Get("__EditResultTableSettingsWindow_Title");

        tbCurrenTime.Text = TimeType.Auto.GetLocalizedName();
    }

    public EditResultTableSettingsView(ResultTableNodeVM model)
        : this()
    {
        this.Model = model;
        tbCurrenTime.Text = model.TimeType.GetLocalizedName();

        foreach (var input in model.Input)
        {
            if (input.IsAny || string.IsNullOrWhiteSpace(input.Name))
                continue;

            var m = new ResourceRequestEditorVM()
            {
                Name = input.Name,
                IsSet = input.IsSet,
                ValueInCurrentTime = input.ValuePerTime.ToString(CultureInfo.InvariantCulture)
            };

            items.Add(m);
        }

        icItems.ItemsSource = items;
    }

    private void Button_Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}