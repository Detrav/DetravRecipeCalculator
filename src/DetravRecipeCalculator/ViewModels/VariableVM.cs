using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class VariableVM : ViewModelBase
    {
        [ObservableProperty]
        private string? name;
        [ObservableProperty]
        private double value;
        private readonly RecipeNodeVM parent;
        [ObservableProperty]
        private bool isEditing;
        [ObservableProperty]
        private string? editValue;

        public ICommand UpCommand { get; }
        public ICommand DownCommand { get; }
        public ICommand EditCommand { get; }

        public VariableVM(GraphEditorVM graphEditor, RecipeNodeVM node)
        {
            this.parent = node;
            UpCommand = new RelayCommand(() =>
            {
                Value += 1;
                graphEditor.UndoRedo.PushState("Set variables");
            });

            DownCommand = new RelayCommand(() =>
            {
                Value -= 1;
                graphEditor.UndoRedo.PushState("Set variables");
            });

            EditCommand = new RelayCommand<bool?>(cancel =>
            {
                if (cancel.GetValueOrDefault())
                {
                    IsEditing = false;
                    if (double.TryParse(EditValue, CultureInfo.InvariantCulture, out var v))
                    {
                        Value = v;
                        EditValue = null;
                        graphEditor.UndoRedo.PushState("Set variables");
                    }
                }
                else
                {
                    EditValue = Value.ToString(CultureInfo.InvariantCulture);
                    IsEditing = true;
                }
            });
        }

        partial void OnValueChanged(double value)
        {
            parent.RefreshTimeToCraft();
            foreach (var item in parent.Input)
                item.SetParameter(Name, Value);
            foreach (var item in parent.Output)
                item.SetParameter(Name, Value);
        }

    }
}
