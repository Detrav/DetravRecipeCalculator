using Avalonia;
using Avalonia.Utilities;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DetravRecipeCalculator.Utils
{
    public class PointJsonConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                using StringTokenizer stringTokenizer = new StringTokenizer(str, CultureInfo.InvariantCulture, "Invalid Point.");

                if (stringTokenizer.TryReadDouble(out var px) && stringTokenizer.TryReadDouble(out var py))
                {
                    return new Point(px, py);
                }
            }
            return new Point();
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
