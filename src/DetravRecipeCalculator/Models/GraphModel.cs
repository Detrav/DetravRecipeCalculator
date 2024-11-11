using Avalonia;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Models
{
    public class GraphModel
    {
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<ConnectionModel> Connections { get; } = new List<ConnectionModel>();

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<NodeModel> Nodes { get; } = new List<NodeModel>();
        public TimeType TimeType { get; set; }
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public Dictionary<string, double> Inputs { get; } = new Dictionary<string, double>();
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public Dictionary<string, double> Outputs { get; } = new Dictionary<string, double>();
        public double ViewportZoom { get; set; }
        [JsonConverter(typeof(PointJsonConverter))]
        public Point ViewportLocation { get; set; }
    }
}