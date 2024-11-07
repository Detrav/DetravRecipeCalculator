using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class XLocItem : ViewModelBase
    {
        [ObservableProperty]
        private string? text;

        public XLocItem(string id)
        {
            this.Id = id;
            Xloc.Register(this);
        }

        public string Id { get; }
    }
}