//using CommunityToolkit.Mvvm.ComponentModel;
//using DetravRecipeCalculator.Models;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DetravRecipeCalculator.ViewModels
//{
//    public abstract partial class IntermediateNode : NodeVM
//    {
//        [ObservableProperty]
//        private IEnumerable<string>? resourceNames;

//        protected IntermediateNode(GraphEditorVM parent, IEnumerable<string> resourceNames) : base(parent)
//        {
//            ResourceNames = resourceNames.ToList();
//            FixPins();
//        }

//        public override void FixPins()
//        {
//            base.FixPins();

//            if (ResourceNames == null || !ResourceNames.Any())
//            {
//                // no serialization any, just clear and add one
//                Input.Clear();
//                Output.Clear();

//                Input.Add(new ConnectorInVM(this));
//                Output.Add(new ConnectorOutVM(this));
//            }
//            else
//            {
//                for (int i = 0; i < Input.Count; i++)
//                {
//                    string? name = Input[i].Name;
//                    if (string.IsNullOrEmpty(name) || !ResourceNames.Contains(name))
//                    {
//                        Input.RemoveAt(i);
//                        i--;
//                    }
//                }

//                for (int i = 0; i < Output.Count; i++)
//                {
//                    string? name = Output[i].Name;
//                    if (string.IsNullOrEmpty(name) || !ResourceNames.Contains(name))
//                    {
//                        Output.RemoveAt(i);
//                        i--;
//                    }
//                }
//            }
//        }

//        public override void RestoreState(NodeModel model)
//        {
//            ResourceNames = model.ResourceNames.ToList();
//            base.RestoreState(model);
//        }

//        public override NodeModel SaveState()
//        {
//            var model = base.SaveState();
//            if (ResourceNames != null)
//                model.ResourceNames.AddRange(ResourceNames);
//            return model;
//        }
//    }
//}

