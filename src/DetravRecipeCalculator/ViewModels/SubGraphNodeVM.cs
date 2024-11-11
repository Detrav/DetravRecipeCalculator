using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public class SubGraphNodeVM : NodeVM
    {
        private GraphModel? graphModel;

        public SubGraphNodeVM(GraphEditorVM parent) : base(parent)
        {
        }

        public override NodeModel SaveState()
        {
            var model = base.SaveState();
            model.GraphModel = graphModel;
            return model;
        }

        public override void RestoreState(NodeModel model)
        {
            graphModel = model.GraphModel;
            base.RestoreState(model);
        }

        public override void Build()
        {
            base.Build();
        }
    }
}
