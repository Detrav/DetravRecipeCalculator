using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Models
{
    public class PinModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }
        public bool IsSet { get; set; }
    }
}