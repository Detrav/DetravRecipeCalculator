using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class CommentNodeVM : NodeVM
    {
        [ObservableProperty]
        private string? comment;

        [ObservableProperty]
        private Size size = new Size(200, 150);

        public CommentNodeVM(GraphEditorVM parent)
                            : base(parent)
        {
            Title = Loc.NodeCommentTitle.Text;
        }

        public override void RestoreState(NodeModel model)
        {
            Size = model.Size;
            Comment = model.Comment;

            base.RestoreState(model);
        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();

            model.Size = Size;
            model.Comment = Comment;

            return model;
        }
    }
}