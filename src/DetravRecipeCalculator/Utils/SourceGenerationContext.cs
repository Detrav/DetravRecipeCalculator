using DetravRecipeCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Config))]
    [JsonSerializable(typeof(PipelineModel))]
    [JsonSerializable(typeof(GraphModel))]
    public partial class SourceGenerationContext : JsonSerializerContext
    {
        public static SourceGenerationContext MyDefaults { get; } = new SourceGenerationContext(new System.Text.Json.JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        });
    }
}