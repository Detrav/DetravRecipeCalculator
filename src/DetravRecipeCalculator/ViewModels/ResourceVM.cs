using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
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

        public XLocItem WindowTitle { get; } = new XLocItem("__Resource_WindowTitle");
        public XLocItem WindowOk { get; } = new XLocItem("__Dialog_BtnOk");
        public XLocItem WindowCancel { get; } = new XLocItem("__Dialog_BtnCancel");


        public XLocItem Icon { get; } = new XLocItem("__Resource_Icon");

        public XLocItem SelectIcon { get; } = new XLocItem("__Resource_SelectIcon");
        public XLocItem OpenIcon { get; } = new XLocItem("__Resource_OpenIcon");
        public XLocItem DeleteIcon { get; } = new XLocItem("__Resource_DeleteIcon");

        public XLocItem BackgroundColor { get; } = new XLocItem("__Resource_BackgroundColor");
        public XLocItem BackgroundColorTip { get; } = new XLocItem("__Resource_BackgroundColorTip");
        public XLocItem ForegroundColor { get; } = new XLocItem("__Resource_ForegroundColor");
        public XLocItem ForegroundColorTip { get; } = new XLocItem("__Resource_ForegroundColorTip");

        public XLocItem ConnectorColor { get; } = new XLocItem("__Resource_ConnectorColor");
        public XLocItem ConnectorColorTip { get; } = new XLocItem("__Resource_ConnectorColorTip");
        public XLocItem Name { get; } = new XLocItem("__Resource_Name");
        public XLocItem NameTip { get; } = new XLocItem("__Resource_NameTip");

    }

    public partial class ResourceVM : ViewModelBase, IUndoRedoObject
    {
        public ResourceModelLocalization Loc { get; } = ResourceModelLocalization.Instance;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string? backgroundColor;

        [ObservableProperty]
        private string? foregroundColor;

        [ObservableProperty]
        private string? connectorColor;

        [ObservableProperty]
        private byte[]? icon;
        [ObservableProperty]
        private bool saved;

        public void RestoreState(object state)
        {
            if(state is ResourceModel model)
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
    }
}
