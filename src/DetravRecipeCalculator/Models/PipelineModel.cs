using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Models
{
    public class PipelineModel
    {
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<RecipeModel> Recipes { get; } = new List<RecipeModel>();

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public List<ResourceModel> Resources { get; } = new List<ResourceModel>();
        public GraphModel? Graph { get; set; }
    }
}