using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Models
{
    public class ResourceValueModel
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}