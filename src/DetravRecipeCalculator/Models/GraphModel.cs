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
        public List<NodeModel> Nodes { get; } = new List<NodeModel>();

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<ConnectionModel> Connections { get; } = new List<ConnectionModel>();
    }
}
