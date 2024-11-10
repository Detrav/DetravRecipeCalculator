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
        public string? Comment { get; set; }

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<PinModel> Input { get; } = new List<PinModel>();

        [JsonConverter(typeof(PointJsonConverter))]
        public Point Location { get; set; }

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<PinModel> Output { get; } = new List<PinModel>();

        public string? ResourceName { get; set; }
        public string? RecipeId { get; set; }

        [JsonConverter(typeof(SizeJsonConverter))]
        public Size Size { get; set; }

        public string? Type { get; set; }

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public Dictionary<string, double> Variables { get; } = new Dictionary<string, double>();

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public Dictionary<string, string?> Parameters { get; } = new Dictionary<string, string?>();
    }
}