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
    }
}