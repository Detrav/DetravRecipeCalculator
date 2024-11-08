using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DetravRecipeCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Views
{
    public class TextBoxFocusable : TextBox
    {
        protected override Type StyleKeyOverride => typeof(TextBox);

        public TextBoxFocusable()
        {
            
            
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == Visual.IsVisibleProperty && change.NewValue is bool bv && bv)
            {
                Focus();
                SelectAll();
            }
        }


        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                if (DataContext is VariableVM vm)
                {
                    vm.EditCommand.Execute(true);
                }
            }
            base.OnKeyUp(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (DataContext is VariableVM vm)
            {
                vm.EditCommand.Execute(true);
            }
            base.OnLostFocus(e);
        }
    }
}
