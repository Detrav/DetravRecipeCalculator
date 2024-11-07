using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public class ByteToBitmapConverter : IValueConverter
    {
        public static ByteToBitmapConverter Instance { get; } = new ByteToBitmapConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is byte[] arr)
                {
                    return new Bitmap(new MemoryStream(arr));
                }
            }
            catch
            {
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}