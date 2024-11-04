using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Models
{
  
    public class RecipeModel
    {
        public string? Note { get;  set; }
        public string? TimeToCraft { get;  set; }
        public bool IsEnabled { get;  set; }
        public string? Name { get;  set; }
        public List<ResourceValueModel> Input { get; } = new List<ResourceValueModel>();
        public List<ResourceValueModel> Output { get; } = new List<ResourceValueModel>();
        public string? BackgroundColor { get;  set; }
        public string? ForegroundColor { get;  set; }
        public byte[]? Icon { get; set; }
    }
}
