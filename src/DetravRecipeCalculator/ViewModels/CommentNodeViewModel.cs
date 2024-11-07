using Avalonia;
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
    public partial class CommentNodeViewModel : NodeViewModel
    {
        public CommentNodeViewModel()
        {
            Title = "Comment";
        }

        [ObservableProperty]
        private Size size = new Size(200, 150);
        [ObservableProperty]
        private string? comment;


        public override NodeModel SaveState()
        {
            var model = base.SaveState();

            model.Size = Size;
            model.Comment = Comment;

            return model;
        }

        public override void RestoreState(PipelineVM pipeline, NodeModel model)
        {
            Size = model.Size;
            Comment = model.Comment;

            base.RestoreState(pipeline, model);
        }

    }
}
