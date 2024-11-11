using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using MsBox.Avalonia.Enums;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public class RecipeModelLocalization
    {
        public static RecipeModelLocalization Instance { get; } = new RecipeModelLocalization();

        public XLocItem AddResourceTip { get; } = new XLocItem("__Recipe_AddResourceTip");
        public XLocItem BackgroundColor { get; } = new XLocItem("__Recipe_BackgroundColor");
        public XLocItem BackgroundColorTip { get; } = new XLocItem("__Recipe_BackgroundColorTip");
        public XLocItem DeleteIcon { get; } = new XLocItem("__Recipe_DeleteIcon");
        public XLocItem Enabled { get; } = new XLocItem("__Recipe_Enabled");
        public XLocItem EnabledTip { get; } = new XLocItem("__Recipe_EnabledTip");
        public XLocItem ForegroundColor { get; } = new XLocItem("__Recipe_ForegroundColor");
        public XLocItem ForegroundColorTip { get; } = new XLocItem("__Recipe_ForegroundColorTip");
        public XLocItem Icon { get; } = new XLocItem("__Recipe_Icon");
        public XLocItem InputTitle { get; } = new XLocItem("__Recipe_InputTitle");
        public XLocItem IsDesabled { get; } = new XLocItem("__Recipe_IsDisabled");
        public XLocItem IsEnabled { get; } = new XLocItem("__Recipe_IsEnabled");
        public XLocItem Name { get; } = new XLocItem("__Recipe_Name");
        public XLocItem NameTip { get; } = new XLocItem("__Recipe_NameTip");
        public XLocItem Note { get; } = new XLocItem("__Recipe_Note");
        public XLocItem NoteTip { get; } = new XLocItem("__Recipe_NoteTip");
        public XLocItem OpenIcon { get; } = new XLocItem("__Recipe_OpenIcon");
        public XLocItem OutputTitle { get; } = new XLocItem("__Recipe_OutputTitle");
        public XLocItem PasteIcon { get; } = new XLocItem("__Recipe_PasteIcon");
        public XLocItem SelectIcon { get; } = new XLocItem("__Recipe_SelectIcon");
        public XLocItem TimeToCraft { get; } = new XLocItem("__Recipe_TimeToCraft");
        public XLocItem TimeToCraftHelpTip { get; } = new XLocItem("__Recipe_TimeToCraftHelpTip");
        public XLocItem TimeToCraftTip { get; } = new XLocItem("__Recipe_TimeToCraftTip");
        public XLocItem WindowCancel { get; } = new XLocItem("__Dialog_BtnCancel");
        public XLocItem WindowOk { get; } = new XLocItem("__Dialog_BtnOk");
        public XLocItem WindowTitle { get; } = new XLocItem("__Recipe_WindowTitle");

        public XLocItem Variables { get; } = new XLocItem("__Recipe_Variables");
        public XLocItem VariablesTip { get; } = new XLocItem("__Recipe_VariablesTip");
    }

    public partial class RecipeVM : ViewModelBase, IUndoRedoObject
    {
        [ObservableProperty]
        private string? backgroundColor;

        [ObservableProperty]
        private Color backgroundColorValue = new Color(0x10, 0, 0, 0);

        [ObservableProperty]
        private string? foregroundColor;

        [ObservableProperty]
        private Color foregroundColorValue = Colors.White;

        [ObservableProperty]
        private byte[]? icon;

        [ObservableProperty]
        private byte[]? iconFiltered;

        [ObservableProperty]
        private string? id;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string? note;

        [ObservableProperty]
        private bool saved;

        [ObservableProperty]
        private string? timeToCraft;
        [ObservableProperty]
        private string? variables;

        public RecipeVM()
        {
        }

        public ObservableCollection<ResourceValueVM> Input { get; } = new ObservableCollection<ResourceValueVM>();

        public RecipeModelLocalization Loc { get; } = RecipeModelLocalization.Instance;

        public ObservableCollection<ResourceValueVM> Output { get; } = new ObservableCollection<ResourceValueVM>();

        public void RestoreState(object state)
        {
            if (state is RecipeModel model)
            {
                Id = model.Id;
                Name = model.Name;
                IsEnabled = model.IsEnabled;
                TimeToCraft = model.TimeToCraft;
                BackgroundColor = model.BackgroundColor;
                ForegroundColor = model.ForegroundColor;
                Note = model.Note;
                Icon = model.Icon;
                Variables = model.Variables;

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

            model.Id = Id;
            model.Name = Name;
            model.IsEnabled = IsEnabled;
            model.TimeToCraft = TimeToCraft;
            model.BackgroundColor = BackgroundColor;
            model.ForegroundColor = ForegroundColor;
            model.Note = Note;
            model.Icon = Icon;
            model.Variables = Variables;

            foreach (var item in Input)
                model.Input.Add(item.SaveState());
            foreach (var item in Output)
                model.Output.Add(item.SaveState());

            return model;
        }

        partial void OnBackgroundColorChanged(string? value)
        {
            BackgroundColorValue = DetravColorHelper.GetColorFormString(BackgroundColor, new Color(0x10, 0, 0, 0));
        }

        partial void OnForegroundColorChanged(string? value)
        {
            ForegroundColorValue = DetravColorHelper.GetColorFormString(ForegroundColor, Colors.White);
        }

        partial void OnForegroundColorValueChanged(Color value)
        {
            UpdateBitmap();
        }

        partial void OnIconChanged(byte[]? value)
        {
            UpdateBitmap();
        }

        private void UpdateBitmap()
        {
            if (Icon == null || Icon.Length < 4)
            {
                IconFiltered = null;
            }
            else if (ForegroundColorValue == Colors.White)
            {
                IconFiltered = Icon;
            }
            else
            {
                try
                {
                    var c = ForegroundColorValue;
                    using var filter = SKImageFilter.CreateColorFilter(SKColorFilter.CreateBlendMode(new SKColor(c.R, c.G, c.B, c.A), SKBlendMode.Modulate));
                    using var bmp = SKBitmap.Decode(Icon);
                    using var img = SKImage.FromBitmap(bmp);
                    using var img2 = img.ApplyImageFilter(filter, new SKRectI(0, 0, img.Width, img.Height), new SKRectI(0, 0, img.Width, img.Height), out SKRectI subSet, out SKPoint point);
                    using var data = img2.Encode(SKEncodedImageFormat.Png, 90);
                    IconFiltered = data.ToArray();
                }
                catch
                {
                    IconFiltered = null;
                }
            }
        }




    }
}