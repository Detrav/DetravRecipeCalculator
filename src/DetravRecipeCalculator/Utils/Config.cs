using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public partial class Config
    {
        public Config()
        {
        }

        public static Config Instance { get; set; } = Load();

        public string AppDataDirectory { get; set; } =
#if DEBUG
    Environment.CurrentDirectory;

#else
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DetravRecipeCalculator");
#endif

        public static Config Load()
        {
            if (!Design.IsDesignMode && File.Exists("config.json"))
            {
                var result = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"), SourceGenerationContext.Default.Config);
                if (result != null)
                {
                    return result;
                }
            }

            return new Config();
        }

        public void CreateAppDataDirectoryIfNotExists()
        {
            if (!Directory.Exists(AppDataDirectory))
                Directory.CreateDirectory(AppDataDirectory);
        }

        public void LoadState(Window window, string? customName = null)
        {
            string name = customName ?? window.GetType().Name;

            if (WindowSavedStates.TryGetValue(name, out var state))
            {
                window.Width = state.Width;
                window.Height = state.Height;
            }
        }

        public void Save()
        {
            var text = JsonSerializer.Serialize(this, SourceGenerationContext.Default.Config);
            CreateAppDataDirectoryIfNotExists();
            File.WriteAllText(Path.Combine(AppDataDirectory, "config.json"), text);
        }

        public void SaveState(Window window, string? customName = null)
        {
            var state = new WindowSavedState();

            state.Width = window.Width;
            state.Height = window.Height;

            WindowSavedStates[customName ?? window.GetType().Name] = state;
        }
    }
}