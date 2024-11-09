using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public class ResourceModelLocalization
    {
        public static ResourceModelLocalization Instance { get; } = new ResourceModelLocalization();

        public XLocItem BackgroundColor { get; } = new XLocItem("__Resource_BackgroundColor");
        public XLocItem BackgroundColorTip { get; } = new XLocItem("__Resource_BackgroundColorTip");
        public XLocItem ConnectorColor { get; } = new XLocItem("__Resource_ConnectorColor");
        public XLocItem ConnectorColorTip { get; } = new XLocItem("__Resource_ConnectorColorTip");
        public XLocItem DeleteIcon { get; } = new XLocItem("__Resource_DeleteIcon");
        public XLocItem ForegroundColor { get; } = new XLocItem("__Resource_ForegroundColor");
        public XLocItem ForegroundColorTip { get; } = new XLocItem("__Resource_ForegroundColorTip");
        public XLocItem Icon { get; } = new XLocItem("__Resource_Icon");
        public XLocItem Name { get; } = new XLocItem("__Resource_Name");
        public XLocItem NameTip { get; } = new XLocItem("__Resource_NameTip");
        public XLocItem OpenIcon { get; } = new XLocItem("__Resource_OpenIcon");
        public XLocItem PasteIcon { get; } = new XLocItem("__Resource_PasteIcon");
        public XLocItem SelectIcon { get; } = new XLocItem("__Resource_SelectIcon");
        public XLocItem WindowCancel { get; } = new XLocItem("__Dialog_BtnCancel");
        public XLocItem WindowOk { get; } = new XLocItem("__Dialog_BtnOk");
        public XLocItem WindowTitle { get; } = new XLocItem("__Resource_WindowTitle");


        public XLocItem ShortResourceName { get; } = new XLocItem("__Resource_ShortResourceName");
        public XLocItem ShortResourceNameTip { get; } = new XLocItem("__Resource_ShortResourceNameTip");
    }

    public partial class ResourceVM : ViewModelBase, IUndoRedoObject
    {
        [ObservableProperty]
        private string? backgroundColor;

        [ObservableProperty]
        private Color backgroundColorValue = new Color(0x10, 0, 0, 0);

        [ObservableProperty]
        private string? connectorColor;

        [ObservableProperty]
        private Color connectorColorValue;

        [ObservableProperty]
        private string? foregroundColor;

        [ObservableProperty]
        private Color foregroundColorValue = Colors.White;

        [ObservableProperty]
        private byte[]? icon;

        [ObservableProperty]
        private byte[]? iconFiltered;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private bool saved;

        public ResourceVM()
        {
            OnConnectorColorChanged(null);
        }

        public ResourceModelLocalization Loc { get; } = ResourceModelLocalization.Instance;

        public void RestoreState(object state)
        {
            if (state is ResourceModel model)
            {
                IsEnabled = model.IsEnabled;
                Name = model.Name;
                BackgroundColor = model.BackgroundColor;
                ForegroundColor = model.ForegroundColor;
                ConnectorColor = model.ConnectorColor;
                Icon = model.Icon;
            }
        }

        public object SaveState()
        {
            var model = new ResourceModel();

            model.IsEnabled = IsEnabled;
            model.Name = Name;
            model.BackgroundColor = BackgroundColor;
            model.ForegroundColor = ForegroundColor;
            model.ConnectorColor = ConnectorColor;
            model.Icon = Icon;

            return model;
        }

        partial void OnBackgroundColorChanged(string? value)
        {
            BackgroundColorValue = DetravColorHelper.GetColorFormString(BackgroundColor, new Color(0x10, 0, 0, 0));
        }

        partial void OnConnectorColorChanged(string? value)
        {

            ConnectorColorValue = DetravColorHelper.GetColorFormString(ConnectorColor, DetravColorHelper.GetRandomColor(Name));
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

        partial void OnNameChanged(string? value)
        {
            OnConnectorColorChanged(ConnectorColor);
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