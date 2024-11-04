using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{

    public class RecipeModelLocalization
    {
        public static RecipeModelLocalization Instance { get; } = new RecipeModelLocalization();

        public XLocItem WindowTitle { get; } = new XLocItem("__Recipe_WindowTitle");
        public XLocItem WindowOk { get; } = new XLocItem("__Dialog_BtnOk");
        public XLocItem WindowCancel { get; } = new XLocItem("__Dialog_BtnCancel");

        public XLocItem Name { get; } = new XLocItem("__Recipe_Name");
        public XLocItem NameTip { get; } = new XLocItem("__Recipe_NameTip");
        public XLocItem TimeToCraft { get; } = new XLocItem("__Recipe_TimeToCraft");
        public XLocItem TimeToCraftTip { get; } = new XLocItem("__Recipe_TimeToCraftTip");

        public XLocItem Enabled { get; } = new XLocItem("__Recipe_Enabled");
        public XLocItem EnabledTip { get; } = new XLocItem("__Recipe_EnabledTip");
        public XLocItem BackgroundColor { get; } = new XLocItem("__Recipe_BackgroundColor");
        public XLocItem BackgroundColorTip { get; } = new XLocItem("__Recipe_BackgroundColorTip");
        public XLocItem ForegroundColor { get; } = new XLocItem("__Recipe_ForegroundColor");
        public XLocItem ForegroundColorTip { get; } = new XLocItem("__Recipe_ForegroundColorTip");
        public XLocItem Note { get; } = new XLocItem("__Recipe_Note");
        public XLocItem NoteTip { get; } = new XLocItem("__Recipe_NoteTip");

        public XLocItem TimeToCraftHelpTip { get; } = new XLocItem("__Recipe_TimeToCraftHelpTip");
        public XLocItem AddResourceTip { get; } = new XLocItem("__Recipe_AddResourceTip");


        public XLocItem InputTitle { get; } = new XLocItem("__Recipe_InputTitle");
        public XLocItem OutputTitle { get; } = new XLocItem("__Recipe_OutputTitle");

        public XLocItem IsEnabled { get; } = new XLocItem("__Recipe_IsEnabled");
        public XLocItem IsDesabled { get; } = new XLocItem("__Recipe_IsDisabled");
        public XLocItem Icon { get; } = new XLocItem("__Recipe_Icon");

        public XLocItem SelectIcon { get; } = new XLocItem("__Recipe_SelectIcon");
        public XLocItem OpenIcon { get; } = new XLocItem("__Recipe_OpenIcon");
        public XLocItem DeleteIcon { get; } = new XLocItem("__Recipe_DeleteIcon");


    }

    public partial class RecipeVM : ViewModelBase, IUndoRedoObject
    {
        [ObservableProperty]
        private string? name;
        [ObservableProperty]
        private bool isEnabled;
        [ObservableProperty]
        private string? timeToCraft;
        [ObservableProperty]
        private string? backgroundColor;
        [ObservableProperty]
        private string? foregroundColor;
        [ObservableProperty]
        private string? note;
        [ObservableProperty]
        private bool saved;

        public ObservableCollection<ResourceValueVM> Input { get; } = new ObservableCollection<ResourceValueVM>();
        public ObservableCollection<ResourceValueVM> Output { get; } = new ObservableCollection<ResourceValueVM>();

        public RecipeModelLocalization Loc { get; } = RecipeModelLocalization.Instance;
        [ObservableProperty]
        public byte[]? icon;

        public RecipeVM()
        {

        }
        public void RestoreState(object state)
        {
            if (state is RecipeModel model)
            {
                Name = model.Name;
                IsEnabled = model.IsEnabled;
                TimeToCraft = model.TimeToCraft;
                BackgroundColor = model.BackgroundColor;
                ForegroundColor = model.ForegroundColor;
                Note = model.Note;
                Icon = model.Icon;

                Input.Clear();
                Output.Clear();

                foreach (var item in model.Input)
                {
                    var vm = new ResourceValueVM();
                    vm.RestoreState(item);
                    Input.Add(vm);
                }

                foreach (var item in model.Output)
                {
                    var vm = new ResourceValueVM();
                    vm.RestoreState(item);
                    Output.Add(vm);
                }
            }
        }

        public object SaveState()
        {
            var model = new RecipeModel();

            model.Name = Name;
            model.IsEnabled = IsEnabled;
            model.TimeToCraft = TimeToCraft;
            model.BackgroundColor = BackgroundColor;
            model.ForegroundColor = ForegroundColor;
            model.Note = Note;
            model.Icon = Icon;

            foreach (var item in Input)
            {
                if (item.SaveState() is ResourceValueModel resourceValueModel)
                {
                    model.Input.Add(resourceValueModel);
                }
            }

            foreach (var item in Output)
            {
                if (item.SaveState() is ResourceValueModel resourceValueModel)
                {
                    model.Output.Add(resourceValueModel);
                }
            }



            return model;
        }

        //public string GetVizualizeText()
        //{
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine(Xloc.Get("__Recipe_Name") + " " + Name);
        //    sb.AppendLine(Xloc.Get("__Recipe_TimeToCraft") + " " + TimeToCraft);
        //    if (IsEnabled) sb.AppendLine(Xloc.Get("__Recipe_IsEnabled"));
        //    else sb.AppendLine(Xloc.Get("__Recipe_IsDisabled"));
        //    sb.AppendLine(Xloc.Get("__Recipe_Color") + " " + Color);
        //    sb.AppendLine(Xloc.Get("__Recipe_Note")); if (!String.IsNullOrWhiteSpace(Note)) sb.AppendLine(Note);

        //    sb.AppendLine(Xloc.Get("__Recipe_InputTitle"));

        //    foreach(var item in Input)
        //    {
        //        sb.Append(item.Name).Append(": ").AppendLine(item.Value);
        //    }

        //    sb.AppendLine(Xloc.Get("__Recipe_OutputTitle"));

        //    foreach (var item in Output)
        //    {
        //        sb.Append(item.Name).Append(": ").AppendLine(item.Value);
        //    }

        //    return sb.ToString();
        //}

    }
}
