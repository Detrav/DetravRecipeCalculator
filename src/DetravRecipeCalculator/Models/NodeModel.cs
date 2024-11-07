using Avalonia;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Models
{
    public class NodeModel
    {
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<PinModel> Input { get; } = new List<PinModel>();
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<PinModel> Output { get; } = new List<PinModel>();
        [JsonConverter(typeof(PointJsonConverter))]
        public Point Location { get; set; }
        [JsonConverter(typeof(SizeJsonConverter))]
        public Size Size { get; set; }
        public string? Comment { get; set; }
        public string? RecipeId { get; set; }
        public int Number { get; set; }
        public int Tier { get; set; }
        public string? Type { get; set; }
    }
}
