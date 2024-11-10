using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public class FlyoutCreateNodeItemVM
    {

        public FlyoutCreateNodeItemVM(string title, Func<NodeVM?> action, IEnumerable<NodeViewModelFactory.PinDiscriminator> pinDiscriminators)
        {
            this.Title = title;
            this.Action = action;
            PinDiscriminators = pinDiscriminators;
        }

        public string Title { get; }
        public Func<NodeVM?> Action { get; }
        public IEnumerable<NodeViewModelFactory.PinDiscriminator> PinDiscriminators { get; }
    }
}
