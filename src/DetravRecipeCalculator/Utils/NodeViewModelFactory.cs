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
            Register<CommentNodeViewModel>();
            Register<RecipeNodeVM>();
            Register<DebugNodeVM>();
        }

        public static Dictionary<string, Func<NodeViewModel>> Factory { get; } = new Dictionary<string, Func<NodeViewModel>>();

        public static void Register<T>()
            where T : NodeViewModel, new()
        {
            Factory[typeof(T).Name] = () => new T();
        }

        public static string GetName(NodeViewModel @object)
        {
            return @object.GetType().Name;
        }

        public static NodeViewModel? Create(string? name)
        {
            if (!String.IsNullOrWhiteSpace(name) && Factory.TryGetValue(name, out var activator))
            {
                return activator();
            }
            return null;
        }
    }
}
