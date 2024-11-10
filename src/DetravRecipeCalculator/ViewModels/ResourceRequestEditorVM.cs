using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class ResourceRequestEditorVM : ViewModelBase
    {
        [ObservableProperty]
        private bool isSet;
        [ObservableProperty]
        private string? name;
        [ObservableProperty]
        private string? valueInCurrentTime;
    }
}
