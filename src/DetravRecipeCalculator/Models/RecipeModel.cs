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
        public string? BackgroundColor { get; set; }
        public string? ForegroundColor { get; set; }
        public byte[]? Icon { get; set; }
        public string? Id { get; set; }

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<ResourceValueModel> Input { get; } = new List<ResourceValueModel>();

        public bool IsEnabled { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<ResourceValueModel> Output { get; } = new List<ResourceValueModel>();

        public string? TimeToCraft { get; set; }
        public string? Variables { get; set; }
    }
}