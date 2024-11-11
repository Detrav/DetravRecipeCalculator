using DetravRecipeCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public static class NodeViewModelFactory
    {
        static NodeViewModelFactory()
        {
            Register<CommentNodeVM>(parent => new CommentNodeVM(parent));
            Register<RecipeNodeVM>(parent => new RecipeNodeVM(parent));
            Register<DebugNodeVM>(parent => new DebugNodeVM(parent));
            Register<ResultTableNodeVM>(parent => new ResultTableNodeVM(parent));
            Register<SplitConnectorNodeVM>(parent => new SplitConnectorNodeVM(parent, null));
            Register<SubGraphNodeVM>(parent => new SubGraphNodeVM(parent));
        }

        public delegate NodeVM NodeVMActivator(GraphEditorVM parent);

        public static Dictionary<string, NodeVMActivator> Factory { get; } = new Dictionary<string, NodeVMActivator>();

        public static NodeVM? Create(string? name, GraphEditorVM parent)
        {
            if (!String.IsNullOrWhiteSpace(name) && Factory.TryGetValue(name, out var activator))
            {
                return activator(parent);
            }
            return null;
        }

        public static string GetName(NodeVM @object)
        {
            return @object.GetType().Name;
        }

        public static void Register<T>(NodeVMActivator activator)
                            where T : NodeVM
        {
            Factory[typeof(T).Name] = activator;
        }

        public static IEnumerable<PinDiscriminator> GetPinDiscrimantors<T>(RecipeVM? therecipe = null)
        {
            List<PinDiscriminator> result = new List<PinDiscriminator>();
            switch (typeof(T).Name)
            {
                case nameof(CommentNodeVM):
                    break;

                case nameof(ResultTableNodeVM):
                    result.Add(new PinDiscriminator() { IsAny = true, IsInput = true });
                    break;

                case nameof(SplitConnectorNodeVM):
                    result.Add(new PinDiscriminator() { IsAny = true, IsInput = true });
                    result.Add(new PinDiscriminator() { IsAny = true, IsInput = false });
                    break;
                case nameof(RecipeNodeVM):

                    if (therecipe != null)
                    {
                        foreach (var item in therecipe.Input)
                            result.Add(new PinDiscriminator() { Name = item.Name, IsInput = true });

                        foreach (var item in therecipe.Output)
                            result.Add(new PinDiscriminator() { Name = item.Name, IsInput = false });
                    }

                    break;
            }
            return result;
        }

        public class PinDiscriminator
        {
            public bool IsInput { get; set; }
            public string? Name { get; set; }
            public bool IsAny { get; set; }
        }
    }
}

