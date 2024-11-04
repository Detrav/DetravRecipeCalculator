using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    partial class Config
    {
        public string? CurrentLocale { get; set; }
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public Dictionary<string, WindowSavedState> WindowSavedStates { get; } = new Dictionary<string, WindowSavedState>();
    }
}
