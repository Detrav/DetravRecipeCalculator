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
    public ResultTableNodeVM Model { get; }

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
        tbCurrenTime.Text = model.Parent.TimeType.GetLocalizedName();

        foreach (var input in model.Input)
        {
            if (input.IsAny || string.IsNullOrWhiteSpace(input.Name))
                continue;

            var m = new ResourceRequestEditorVM(input)
            {
                Name = input.Name,
                IsSet = input.IsSet,
                ValueInCurrentTime = (input.Value * model.Parent.TimeType.GetTimeInSeconds()).ToString(CultureInfo.InvariantCulture)
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
        foreach (var item in items)
        {
            item.Pin.IsSet = item.IsSet;

            if (item.IsSet)
            {
                double value;

                double.TryParse(item.ValueInCurrentTime, CultureInfo.InvariantCulture, out value);

                value /= Model.Parent.TimeType.GetTimeInSeconds();

                item.Pin.Value = value;
            }
        }

        Close();

        Model.Parent.UndoRedo.PushState("Update requests");
    }
}