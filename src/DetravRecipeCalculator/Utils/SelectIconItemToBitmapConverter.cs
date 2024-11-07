using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using DetravRecipeCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public class SelectIconItemToBitmapConverter : IValueConverter
    {
        public static SelectIconItemToBitmapConverter Instance { get; } = new SelectIconItemToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SelectIconItemVM vm)
            {
                try
                {
                    if (vm.Path != null && vm.Bitmap == null)
                    {
                        vm.Bitmap = new Bitmap(vm.Path);
                    }
                }
                catch
                {
                    vm.Bitmap = new object();
                }
                return vm.Bitmap;
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}