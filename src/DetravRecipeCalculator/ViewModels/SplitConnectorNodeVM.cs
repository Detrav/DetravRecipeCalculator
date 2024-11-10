using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public class SplitConnectorNodeVM : IntermediateNode
    {

        public SplitConnectorNodeVM(GraphEditorVM parent, string? resourceName) 
            : base(parent, resourceName)
        {

        }
    }
}
