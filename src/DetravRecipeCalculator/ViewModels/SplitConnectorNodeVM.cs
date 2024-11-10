using DetravRecipeCalculator.Models;
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
            TimeToCraft = 1;

            foreach (var pin in Input)
                pin.Value = 1;
            foreach (var pin in Output)
                pin.Value = 1;
        }

        public override void RestoreState(NodeModel model)
        {
            base.RestoreState(model);

            foreach (var pin in Input)
                pin.Value = 1;
            foreach (var pin in Output)
                pin.Value = 1;
        }

        public override double ProviderRequests()
        {
            double sum = 0;
            foreach (var input in Input)
            {
                if (input.Connection != null)
                {
                    if (input.Connection.Parent is IntermediateNode nextNode)
                    {
                        sum += nextNode.ProviderRequests();
                    }
                    else
                    {
                        sum += input.Connection.GetValuePerSecond();
                    }
                }
            }

            return sum;
        }
    }
}
