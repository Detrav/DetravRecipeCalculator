using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DetravRecipeCalculator.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public static class MessageBoxExtentions
    {
        public static async Task<T> ShowDialogAsync<T>(this IMsBox<T> box, Control? control = null)
        {
            if (control != null)
            {
                var topLevel = TopLevel.GetTopLevel(control);

                if (topLevel is Window window)
                {
                    return await box.ShowWindowDialogAsync(window);
                }
            }

            return await box.ShowAsync();
        }

        internal static async Task ShowErrorAsync(Exception ex, Control? control = null)
        {
            await MessageBoxManager.GetMessageBoxStandard(Xloc.Get("__Errors_Title"),
                ex.Message + "\n" +
                ex.StackTrace +
                (ex.InnerException != null ? ("\n" + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace) : "")
                , MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error).ShowDialogAsync(control);
        }
    }
}
