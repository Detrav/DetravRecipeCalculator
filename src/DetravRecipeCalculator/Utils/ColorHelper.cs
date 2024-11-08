using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DetravRecipeCalculator.Utils
{
    public static class DetravColorHelper
    {
        public static Color GetColorFormString(string? value, Color defaultColor)
        {
            if (value == null)
                return defaultColor;

            if (value.Length < 4 || value[0] != '#')
                return defaultColor;

            byte a = 255;
            byte r = 0;
            byte g = 0;
            byte b = 0;

            switch (value.Length)
            {
                case 4:
                    if (!TryGetValue(value.AsSpan(1, 1), 0xf, out r)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(2, 1), 0xf, out g)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(3, 1), 0xf, out b)) return defaultColor;
                    break;

                case 5:
                    if (!TryGetValue(value.AsSpan(1, 1), 0xf, out a)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(2, 1), 0xf, out r)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(3, 1), 0xf, out g)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(4, 1), 0xf, out b)) return defaultColor;
                    break;

                case 7:
                    if (!TryGetValue(value.AsSpan(1, 2), 0xff, out r)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(3, 2), 0xff, out g)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(5, 2), 0xff, out b)) return defaultColor;
                    break;

                case 9:
                    if (!TryGetValue(value.AsSpan(1, 2), 0xff, out a)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(3, 2), 0xff, out r)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(5, 2), 0xff, out g)) return defaultColor;
                    if (!TryGetValue(value.AsSpan(7, 2), 0xff, out b)) return defaultColor;
                    break;

                default:
                    return defaultColor;
            }

            return new Color(a, r, g, b);
        }

        public static Color GetRandomColor(string? seedName)
        {
            int i = 1;
            var seed = (seedName ?? "None").Sum(m => (int)(m * i++));
            var r = new Random(seed);

            var color = new Color(255, (byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256));

            return color;
        }

        private static bool TryGetValue(ReadOnlySpan<char> readOnlySpan, double size, out byte color)
        {
            if (int.TryParse(readOnlySpan, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
            {
                color = (byte)(255 * (result / size));
                return true;
            }
            color = 0;
            return false;
        }
    }
}