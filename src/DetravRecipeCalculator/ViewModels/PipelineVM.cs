using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Models;
using DetravRecipeCalculator.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class PipelineVM : ViewModelBase, IUndoRedoObject
    {
        [ObservableProperty]
        private string? displayName;

        [ObservableProperty]
        private string? filePath;

        [ObservableProperty]
        private string? filterForRecipes;

        [ObservableProperty]
        private string? filterForResources;

        [ObservableProperty]
        private RecipeNodeVM? recipeNodePreview;

        [ObservableProperty]
        private IEnumerable<RecipeVM>? recipesFiltered;

        [ObservableProperty]
        private IEnumerable<ResourceVM>? resourcesFiltered;

        [ObservableProperty]
        private bool saved;

        [ObservableProperty]
        private RecipeVM? selectedRecipe;

        [ObservableProperty]
        private ResourceVM? selectedResource;

        public PipelineVM()
        {
            UpdateDisplayName();
            RefreshRecipesFilters();
            RefreshResourcesFilters();
            Recipes.CollectionChanged += Recipes_CollectionChanged;
            Resources.CollectionChanged += Resources_CollectionChanged;
            UndoRedo = new UndoRedoManager(this);
        }

        public ObservableCollection<RecipeVM> Recipes { get; } = new ObservableCollection<RecipeVM>();
        public ObservableCollection<ResourceVM> Resources { get; } = new ObservableCollection<ResourceVM>();
        public UndoRedoManager UndoRedo { get; }
        public GraphModel? Graph { get; set; }

        public static PipelineVM Load(string path)
        {
            return LoadFromJson(File.ReadAllText(path), path);
        }

        public static PipelineVM LoadFromJson(string json, string? path = null)
        {
            var result = new PipelineVM();
            var mdl = System.Text.Json.JsonSerializer.Deserialize<PipelineModel>(json, SourceGenerationContext.Default.PipelineModel);
            if (mdl == null)
                throw new Exception(Xloc.Get("__Errors_CantLoadFile"));
            result.RestoreState(mdl);
            result.FilePath = path;
            result.Saved = path != null;
            result.RefreshRecipesFilters();
            result.RefreshResourcesFilters();
            result.UndoRedo.Reset();
            return result;
        }

        public void RefreshPreview()
        {
            if (SelectedRecipe == null)
            {
                RecipeNodePreview = null;
                return;
            }

            var editor = new GraphEditorVM(this);

            RecipeNodePreview = new RecipeNodeVM(editor, SelectedRecipe);
        }

        public void RestoreState(object state)
        {
            if (state is PipelineModel model)
            {
                Recipes.Clear();
                foreach (var recipeModel in model.Recipes)
                {
                    var recipe = new RecipeVM();
                    recipe.RestoreState(recipeModel);
                    Recipes.Add(recipe);
                }

                Resources.Clear();

                foreach (var resourceModel in model.Resources)
                {
                    var resource = new ResourceVM();
                    resource.RestoreState(resourceModel);
                    Resources.Add(resource);
                }

                RefreshResources();

                Graph = model.Graph;
            }
        }

        public bool Save()
        {
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                try
                {
                    var model = SaveState() as PipelineModel;
                    if (model == null)
                        throw new NotSupportedException();
                    File.WriteAllText(FilePath, JsonSerializer.Serialize(model, SourceGenerationContext.Default.PipelineModel));
                    Saved = true;
                }
                catch
                {
                }
            }
            return Saved;
        }

        public object SaveState()
        {
            var model = new PipelineModel();
            foreach (var recipe in Recipes)
            {
                var recipeModel = recipe.SaveState() as RecipeModel;
                if (recipeModel != null)
                {
                    model.Recipes.Add(recipeModel);
                }
            }

            RefreshResources();

            foreach (var resource in Resources)
            {
                var resourceModel = resource.SaveState() as ResourceModel;

                if (resourceModel != null)
                {
                    model.Resources.Add(resourceModel);
                }
            }

            model.Graph = Graph;

            return model;
        }

        partial void OnFilePathChanged(string? value)
        {
            UpdateDisplayName();
        }

        partial void OnFilterForRecipesChanged(string? value)
        {
            RefreshResourcesFilters();
        }

        partial void OnFilterForResourcesChanged(string? value)
        {
            RefreshResourcesFilters();
        }

        partial void OnSavedChanged(bool value)
        {
            UpdateDisplayName();
        }

        partial void OnSelectedRecipeChanged(RecipeVM? value)
        {
            RefreshPreview();
        }

        private void Recipes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshRecipesFilters();
        }

        private void RefreshRecipesFilters()
        {
            if (string.IsNullOrEmpty(FilterForRecipes))
            {
                RecipesFiltered = Recipes.OrderBy(m => m.Name).ToArray();
            }
            else
            {
                RecipesFiltered = Recipes.Where(m => m.Name == null || m.Name.Contains(FilterForRecipes, StringComparison.OrdinalIgnoreCase)).OrderBy(m => m.Name).ToArray();
            }
        }

        private void RefreshResources()
        {
            List<string> strings = new List<string>();

            foreach (var recipe in Recipes)
            {
                foreach (var item in recipe.Input)
                    if (!String.IsNullOrWhiteSpace(item.Name))
                        strings.Add(item.Name);
                foreach (var item in recipe.Output)
                    if (!String.IsNullOrWhiteSpace(item.Name))
                        strings.Add(item.Name);
            }

            for (int i = 0; i < Resources.Count; i++)
            {
                var resource = Resources[i];

                resource.IsEnabled = strings.Contains(resource.Name!);
            }

            foreach (var str in strings)
            {
                if (!Resources.Any(m => m.Name == str))
                {
                    Resources.Add(new ResourceVM()
                    {
                        Name = str,
                        IsEnabled = true
                    });
                }
            }
        }

        private void RefreshResourcesFilters()
        {
            if (string.IsNullOrEmpty(FilterForResources))
            {
                ResourcesFiltered = Resources.OrderBy(m => m.Name).ToArray();
            }
            else
            {
                ResourcesFiltered = Resources.Where(m => m.Name == null || m.Name.Contains(FilterForResources, StringComparison.OrdinalIgnoreCase)).OrderBy(m => m.Name).ToArray();
            }
        }

        private void Resources_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshResourcesFilters();
        }

        private void UpdateDisplayName()
        {
            var fileName = FilePath;
            if (File.Exists(fileName))
            {
                fileName = Path.GetFileName(fileName);
            }
            else
            {
                fileName = "unknown.json";
            }

            if (!Saved)
            {
                fileName += "*";
            }

            DisplayName = fileName;
        }
    }
}