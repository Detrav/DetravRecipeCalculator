using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DetravRecipeCalculator.ViewModels;

public partial class MainVM : ViewModelBase
{
    [ObservableProperty]
    private PipelineVM? pipeline;

    public string Greeting => "Welcome to Avalonia!";

    public MainViewModelLocalization Loc { get; } = new MainViewModelLocalization();

    public MenuLocalization Menu { get; } = new MenuLocalization();
    

    public IEnumerable<string> GetAllResourceNames()
    {
        List<string> result = new List<string>();
        if (Pipeline != null)
        {
            foreach (ResourceVM revm in Pipeline.Resources)
            {
                result.Add(revm.Name ?? "");
            }
        }

        return result.Distinct().ToArray()!;
    }

    public class MainViewModelLocalization
    {
        public XLocItem Create { get; } = new XLocItem("__MainView_Create");
        public XLocItem Delete { get; } = new XLocItem("__MainView_Delete");
        public XLocItem Edit { get; } = new XLocItem("__MainView_Edit");
        public XLocItem EnableDisable { get; } = new XLocItem("__MainView_EnableDisable");
        public XLocItem Filter { get; } = new XLocItem("__MainView_Filter");
        public XLocItem Step1 { get; } = new XLocItem("__MainView_Step1");
        public XLocItem Step2 { get; } = new XLocItem("__MainView_Step2");
        public XLocItem Step3 { get; } = new XLocItem("__MainView_Step3");
        public XLocItem Step4 { get; } = new XLocItem("__MainView_Step4");
        public XLocItem Refresh { get; } = new XLocItem("__MainView_Refresh");
    }

    public class MenuLocalization
    {
        public XLocItem Edit { get; } = new XLocItem("__Main_Menu_Edit");
        public XLocItem Edit_Redo { get; } = new XLocItem("__Main_Menu_Edit_Redo");
        public XLocItem Edit_Undo { get; } = new XLocItem("__Main_Menu_Edit_Undo");
        public XLocItem File { get; } = new XLocItem("__Main_Menu_File");
        public XLocItem File_Exit { get; } = new XLocItem("__Main_Menu_File_Exit");
        public XLocItem File_New { get; } = new XLocItem("__Main_Menu_File_New");
        public XLocItem File_Open { get; } = new XLocItem("__Main_Menu_File_Open");
        public XLocItem File_Save { get; } = new XLocItem("__Main_Menu_File_Save");
        public XLocItem File_SaveAs { get; } = new XLocItem("__Main_Menu_File_SaveAs");
        public XLocItem Help { get; } = new XLocItem("__Main_Menu_Help");
        public XLocItem Help_About { get; } = new XLocItem("__Main_Menu_Help_About");
        public XLocItem Tools { get; } = new XLocItem("__Main_Menu_Tools");
        public XLocItem Tools_Language { get; } = new XLocItem("__Main_Menu_Tools_Language");
    }
}