using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using DetravRecipeCalculator.Utils;
using DetravRecipeCalculator.ViewModels;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DetravRecipeCalculator.Views;

public partial class MainView : UserControl
{
    public static FilePickerFileType TextJson { get; } = new FilePickerFileType("Json files")
    {
        Patterns = new string[1] { "*.json" },
        AppleUniformTypeIdentifiers = new string[1] { "public.json" },
        MimeTypes = new string[1] { "application/json" }
    };

    public MainView()
    {
        InitializeComponent();

        foreach (var locale in Xloc.AvailableLocales)
        {
            var lLocale = locale;
            var item = new MenuItem();
            var loc = Xloc.GetLocale(locale);
            item.Header = loc.Get("__LanguageName", false);
            item.Click += (s, e) =>
            {
                Xloc.SwitchLocalization(lLocale);
                Config.Instance.CurrentLocale = lLocale;
                Config.Instance.Save();
            };
            miLanguages.Items.Add(item);
        }
    }

    private async void MenuItem_About_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await MessageBoxManager.GetMessageBoxStandard(Xloc.Get("__Window_About"), @$"
Detrav Recipe Calculator

{Xloc.Get("__Window_About_Version")}: {GetType().Assembly.GetName().Version}

{Xloc.Get("__Window_About_Author")}: Detrav / Witaly Ezepchuk / Vitaliy Ezepchuk
", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info).ShowDialogAsync(this);
    }

    private async void MenuItem_New_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        bool canCreate = true;

        if (DataContext is MainViewModel vm)
        {
            if (vm.Pipeline != null && !vm.Pipeline.Saved)
            {
                canCreate = await ShowSavePrompt();
            }

            if (canCreate)
            {
                vm.Pipeline = new PipelineVM();
            }
        }
    }

    private async Task<bool> ShowSavePrompt()
    {
        switch (await MessageBoxManager.GetMessageBoxStandard(Xloc.Get("__Window_Saving_Title"), Xloc.Get("__Window_Saving_Text"), MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel, MsBox.Avalonia.Enums.Icon.Warning).ShowDialogAsync(this))
        {
            case MsBox.Avalonia.Enums.ButtonResult.Yes:
                if (await SavePipelineAsync())
                    return true;
                break;
            case MsBox.Avalonia.Enums.ButtonResult.No:
                return true;
        }

        return false;
    }

    private async void MenuItem_Open_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        bool canCreate = true;

        if (DataContext is MainViewModel vm)
        {
            if (vm.Pipeline != null && !vm.Pipeline.Saved)
            {
                canCreate = await ShowSavePrompt();
            }

            if (canCreate)
            {
                var fpoo = new FilePickerOpenOptions()
                {
                    AllowMultiple = false,
                    FileTypeFilter = new[] { TextJson, FilePickerFileTypes.All },
                    Title = Xloc.Get("__Window_File_OpenTitle")
                };
                var tl = TopLevel.GetTopLevel(this);

                if (tl != null)
                {
                    var file = (await tl.StorageProvider.OpenFilePickerAsync(fpoo)).FirstOrDefault();

                    if (file != null)
                    {
                        var path = file.TryGetLocalPath();

                        if (File.Exists(path))
                        {
                            try
                            {
                                vm.Pipeline = PipelineVM.Load(path);
                            }
                            catch (Exception ex)
                            {
                                await MessageBoxExtentions.ShowErrorAsync(ex, this);
                            }
                        }
                    }
                }
            }
        }
    }

    private async void MenuItem_Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SavePipelineAsync();
    }

    private async void MenuItem_SaveAs_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SaveAsPipelineAsync();
    }

    private async Task<bool> SaveAsPipelineAsync()
    {
        if (DataContext is MainViewModel vm && vm.Pipeline != null)
        {
            var path = vm.Pipeline.FilePath;

            var lastFileName = Path.GetFileName(path) ?? "unknown.json";
            var lastFolder = Path.GetDirectoryName(path);

            var fpso = new FilePickerSaveOptions()
            {
                DefaultExtension = ".json",
                FileTypeChoices = new[] { TextJson, FilePickerFileTypes.All },
                ShowOverwritePrompt = true,
                SuggestedFileName = lastFileName,
                Title = Xloc.Get("__Window_File_SaveAsTitle")

            };

            var tl = TopLevel.GetTopLevel(this);

            if (tl != null)
            {
                fpso.SuggestedStartLocation = await tl.StorageProvider.TryGetFolderFromPathAsync(lastFolder ?? "");

                var file = await tl.StorageProvider.SaveFilePickerAsync(fpso);

                if (file != null)
                {
                    try
                    {
                        vm.Pipeline.FilePath = file.TryGetLocalPath();
                        return vm.Pipeline.Save();
                    }
                    catch (Exception ex)
                    {
                        await MessageBoxExtentions.ShowErrorAsync(ex, this);
                    }
                }

            }
            return false;

        }
        return true;
    }

    private void MenuItem_Exit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window wnd)
        {
            wnd.Close();
        }
    }

    public async Task<bool> SavePipelineAsync()
    {
        try
        {

            if (DataContext is MainViewModel vm && vm.Pipeline != null)
            {
                return vm.Pipeline.Save() || await SaveAsPipelineAsync();
            }
        }
        catch (Exception ex)
        {
            await MessageBoxExtentions.ShowErrorAsync(ex, this);
        }
        return true;
    }

    private async void Button_AddRecipe_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Pipeline != null)
        {
            var item = new RecipeVM();
            item.IsEnabled = true;

            RecipeEditorWindow wnd = new RecipeEditorWindow();
            wnd.ResourceNames = vm.GetAllResourceNames();
            wnd.DataContext = item;

            if (TopLevel.GetTopLevel(this) is Window owner)
            {
                if (await wnd.ShowDialog<bool>(owner))
                {

                    vm.Pipeline.Recipes.Add(item);
                    vm.Pipeline.UndoRedo.PushState("Add recipe");

                }
            }
        }
    }

    private async void Button_EditRecipe_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        if (DataContext is MainViewModel vm && vm.Pipeline != null && vm.Pipeline.SelectedRecipe != null)
        {
            var item = new RecipeVM();
            item.RestoreState(vm.Pipeline.SelectedRecipe.SaveState());

            RecipeEditorWindow wnd = new RecipeEditorWindow();
            wnd.ResourceNames = vm.GetAllResourceNames();
            wnd.DataContext = item;

            if (TopLevel.GetTopLevel(this) is Window owner)
            {
                if (await wnd.ShowDialog<bool>(owner))
                {
                    vm.Pipeline.SelectedRecipe.RestoreState(item.SaveState());
                    vm.Pipeline.UndoRedo.PushState("Edit recipe");
                }
            }
        }
    }

    private void Button_DeleteRecipe_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        if (DataContext is MainViewModel vm && vm.Pipeline != null && vm.Pipeline.SelectedRecipe != null)
        {
            vm.Pipeline.Recipes.Remove(vm.Pipeline.SelectedRecipe);

            if (vm.Pipeline.Resources.Count == 0 && vm.Pipeline.Recipes.Count == 0)
                Focus();
            vm.Pipeline.UndoRedo.PushState("Delete recipe");
        }
    }


    private void Button_EnableDisableRecipe_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        if (DataContext is MainViewModel vm && vm.Pipeline != null && vm.Pipeline.SelectedRecipe != null)
        {
            vm.Pipeline.SelectedRecipe.IsEnabled = !vm.Pipeline.SelectedRecipe.IsEnabled;
            vm.Pipeline.UndoRedo.PushState("Enable/Disable recipe");
        }
    }

    private void MenuItem_Undo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Pipeline != null) vm.Pipeline.UndoRedo.Undo();
    }

    private void MenuItem_Redo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Pipeline != null) vm.Pipeline.UndoRedo.Redo();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Handled)
            return;
        bool Match(List<KeyGesture> gestures) => gestures.Any(g => g.Matches(e));

        var keymap = Application.Current!.PlatformSettings!.HotkeyConfiguration;

        if (Match(keymap.Undo))
        {
            if (DataContext is MainViewModel vm && vm.Pipeline != null) vm.Pipeline.UndoRedo.Undo();
            e.Handled = true;
        }
        else if (Match(keymap.Redo))
        {
            if (DataContext is MainViewModel vm && vm.Pipeline != null) vm.Pipeline.UndoRedo.Redo();
            e.Handled = true;
        }
        base.OnKeyUp(e);
    }

    private void Button_DeleteSelectedResource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Pipeline != null && vm.Pipeline.SelectedResource != null)
        {
            if (!vm.Pipeline.SelectedResource.IsEnabled)
            {
                vm.Pipeline.Resources.Remove(vm.Pipeline.SelectedResource);

                if (vm.Pipeline.Resources.Count == 0 && vm.Pipeline.Recipes.Count == 0)
                    Focus();

                vm.Pipeline.UndoRedo.PushState("Delete recipe");
            }
        }
    }

    private async void Button_CreateSelectedResource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Pipeline != null)
        {
            var item = new ResourceVM();
            if (TopLevel.GetTopLevel(this) is Window owner)
            {
                while (true)
                {
                    Resource2EditorWindow wnd = new Resource2EditorWindow();
                    wnd.DataContext = item;


                    if (await wnd.ShowDialog<bool>(owner))
                    {
                        if (vm.Pipeline.Resources.Any(m => m.Name == item.Name))
                        {
                            await MessageBoxManager.GetMessageBoxStandard(Xloc.Get("__Errors_Title"), Xloc.Get("__Errors_DuplicateResourceName"), icon: MsBox.Avalonia.Enums.Icon.Error).ShowDialogAsync(this);
                            continue;
                        }

                        vm.Pipeline.Resources.Add(item);
                        vm.Pipeline.UndoRedo.PushState("Add resource");
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    private async void Button_EditSelectedResource_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Pipeline != null && vm.Pipeline.SelectedResource != null)
        {
            var item = new ResourceVM();
            item.RestoreState(vm.Pipeline.SelectedResource.SaveState());
            if (TopLevel.GetTopLevel(this) is Window owner)
            {
                while (true)
                {

                    Resource2EditorWindow wnd = new Resource2EditorWindow();
                    wnd.DataContext = item;


                    if (await wnd.ShowDialog<bool>(owner))
                    {
                        var oldName = vm.Pipeline.SelectedResource.Name;
                        var newName = item.Name;

                        if (oldName != newName)
                        {
                            if (vm.Pipeline.Resources.Any(m => m.Name == newName))
                            {
                                await MessageBoxManager.GetMessageBoxStandard(Xloc.Get("__Errors_Title"), Xloc.Get("__Errors_DuplicateResourceName"), icon: MsBox.Avalonia.Enums.Icon.Error).ShowDialogAsync(this);
                                continue;
                            }

                            foreach (var recipe in vm.Pipeline.Recipes)
                            {
                                foreach (var item2 in recipe.Input)
                                    if (item2.Name == oldName)
                                        item2.Name = newName;

                                foreach (var item2 in recipe.Output)
                                    if (item2.Name == oldName)
                                        item2.Name = newName;
                            }
                        }

                        vm.Pipeline.SelectedResource.RestoreState(item.SaveState());
                        vm.Pipeline.UndoRedo.PushState("Edit resource");
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    private async void Button_Step4_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;

        if (owner == null) 
            return;

        var vm = new GraphEditorVM();
        var wnd = new GraphEditorWindow()
        {
            DataContext = vm,
        };
        
        if (await wnd.ShowDialog<bool>(owner))
        {

        }
    }
}
