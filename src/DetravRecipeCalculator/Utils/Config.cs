using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public partial class Config
    {
        public static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        public static Config Instance { get; set; } = Load();

        public Config()
        {

        }

        public static Config Load()
        {
            if (File.Exists("config.json"))
            {
                var result = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"), SERIALIZER_OPTIONS);
                if (result != null)
                {
                    return result;
                }
            }

            return new Config();
        }

        public void Save()
        {
            var text = JsonSerializer.Serialize(this, SERIALIZER_OPTIONS);
            File.WriteAllText("config.json", text);
        }

        public void SaveSate(Window window, string? customName = null)
        {
            var state = new WindowSavedState();

            state.Width = window.Width;
            state.Height = window.Height;

            WindowSavedStates[customName ?? window.GetType().Name] = state;

        }

        public void LoadSate(Window window, string? customName = null)
        {
            string name = customName ?? window.GetType().Name;

            if (WindowSavedStates.TryGetValue(name, out var state))
            {
                window.Width = state.Width;
                window.Height = state.Height;
            }
        }
    }
}
