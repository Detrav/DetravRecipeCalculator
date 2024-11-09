//using Avalonia.Controls;
//using Avalonia.Controls.Templates;
//using Avalonia.Markup.Xaml.Templates;
//using Avalonia.Metadata;
//using DetravRecipeCalculator.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace DetravRecipeCalculator.Utils
//{
//    internal class MyTemplateSelector : IRecyclingDataTemplate
//    {
//        [Content]
//        public Dictionary<string, IDataTemplate> Templates { get; } = new Dictionary<string, IDataTemplate>();
//        public Func<ResultTableRow, object?>? Getter { get; private set; }

//        public Control? Build(object? data, Control? existing)
//        {
//            if (data is ResultTableRow row)
//            {
//                if (Getter?.Invoke(row) is ResultTableItemParameter parameter && parameter.Value != null)
//                {
//                    var value = parameter.Value;
//                    var key = value.GetType().Name;

//                    if (Templates.TryGetValue(key, out var template))
//                    {
//                        if (template is IRecyclingDataTemplate recyclingDataTemplate)
//                            existing = recyclingDataTemplate.Build(value, existing);
//                        else
//                            existing = template.Build(value);

//                        if (existing != null)
//                            existing.DataContext = value;
//                    }
//                }

//            }
//            return null;
//        }

//        public Control? Build(object? param)
//        {
//            return Build(param, null);
//        }

//        public bool Match(object? data)
//        {
//            return data != null && data is ResultTableRow;
//        }

//        public MyTemplateSelector WithGetter(Func<ResultTableRow, object?> getter)
//        {
//            var result = new MyTemplateSelector();
//            result.Getter = getter;

//            foreach (var kv in Templates)
//            {
//                result.Templates[kv.Key] = kv.Value;
//            }

//            return result;
//        }

//        internal int CompareColumnAscending(ResultTableRow? x, ResultTableRow? y)
//        {
           
//        }

//        internal int CompareColumnDescending(ResultTableRow? x, ResultTableRow? y)
//        {
//            return -CompareColumnAscending(x, y);
//        }
//    }
//}
