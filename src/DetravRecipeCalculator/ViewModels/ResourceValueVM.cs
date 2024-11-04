using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DetravRecipeCalculator.ViewModels
{
    public class ResourceValueVMLocalization
    {
        public static ResourceValueVMLocalization Instance { get; } = new ResourceValueVMLocalization();

        public XLocItem NameTip { get; } = new XLocItem("__ResourceValue_NameTip");
        public XLocItem ValueTip { get; } = new XLocItem("__ResourceValue_ValueTip");

        public XLocItem MoveUpTip { get; } = new XLocItem("__ResourceValue_MoveUpTip");
        public XLocItem MoveDownTip { get; } = new XLocItem("__ResourceValue_MoveDownTip");
        public XLocItem DeleteTip { get; } = new XLocItem("__ResourceValue_DeleteTip");
        public XLocItem HelpTip { get; } = new XLocItem("__ResourceValue_HelpTip");
    }

    public partial class ResourceValueVM : ViewModelBase, IUndoRedoObject
    {
        [ObservableProperty]
        private string? name;
        [ObservableProperty]
        private string? value;
        [ObservableProperty]
        private bool saved;

        public ResourceValueVMLocalization Loc { get; } = ResourceValueVMLocalization.Instance;

        public void RestoreState(object state)
        {
            if (state is ResourceValueModel model)
            {
                Name = model.Name;
                Value = model.Value;
            }
        }

        public object SaveState()
        {
            var model = new ResourceValueModel();
            model.Name = Name;
            model.Value = Value;
            return model;
        }
    }
}
