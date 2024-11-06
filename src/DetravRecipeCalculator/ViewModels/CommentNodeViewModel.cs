using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class CommentNodeViewModel : NodeViewModel
    {
        public CommentNodeViewModel()
        {
            Title = "Comment";
        }

        [ObservableProperty]
        private Size size = new Size(200, 150);
    }
}
