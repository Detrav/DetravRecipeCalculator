using Avalonia.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;

namespace DetravRecipeCalculator.Utils
{
    public class SizeJsonConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                using StringTokenizer stringTokenizer = new StringTokenizer(str, CultureInfo.InvariantCulture, "Invalid Point.");

                if (stringTokenizer.TryReadDouble(out var px) && stringTokenizer.TryReadDouble(out var py))
                {
                    return new Size(px, py);
                }
            }
            return new Size();
        }

        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
