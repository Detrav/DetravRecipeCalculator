using Avalonia.Input.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public static class ClipboardHelper
    {
        public static async Task<byte[]?> GetImageAsync(IClipboard? clipboard)
        {
            if (clipboard != null)
            {
                var formats = await clipboard.GetFormatsAsync();

                if (formats.Contains("PNG")) return await GetImagePNGAsync(clipboard, "PNG");
                if (formats.Contains("image/jpeg")) return await GetImageJpegAsync(clipboard, "image/jpeg");
                if (formats.Contains("Unknown_Format_8")) return await GetImageBitmapAsync(clipboard, "Unknown_Format_8");
            }

            return null;
        }

        private static async Task<byte[]?> GetImageBitmapAsync(IClipboard clipboard, string format)
        {
            try
            {
                var bitmapInfoData = await clipboard.GetDataAsync(format) as byte[];
                if (bitmapInfoData == null) return null;
                var header = ReadBitmapInfo(bitmapInfoData);
                var pixels = new SKColor[header.biWidth * header.biHeight];
                var pixelIndex = header.biWidth * header.biHeight - 1;

                var pos = 52; // length of bitmap header
                for (var y = header.biHeight - 1; y >= 0; y--)
                {
                    for (var x = header.biWidth - 1; x >= 0; x--)
                    {
                        if (pos >= bitmapInfoData.Length)
                            goto exit;

                        var b = bitmapInfoData[pos++];
                        var g = bitmapInfoData[pos++];
                        var r = bitmapInfoData[pos++];
                        if (header.biBitCount == 32)
                        {
                            var a = bitmapInfoData[pos++];
                            pixels[y * header.biWidth + (header.biWidth - 1 - x)] = new SKColor(r, g, b, a);
                        }
                        else
                        {
                            pixels[y * header.biWidth + (header.biWidth - 1 - x)] = new SKColor(r, g, b);
                        }
                    }
                }

                exit:

                using var bitmap = new SKBitmap(new SKImageInfo(header.biWidth, header.biHeight));
                bitmap.Pixels = pixels;

                using var data = bitmap.Encode(SKEncodedImageFormat.Png, 90);

                return data.ToArray();

                //var path = "xxx";
                //var fileName = "yyy";

                //using var writeStream = File.Open(Path.Combine(path, fileName), FileMode.OpenOrCreate, FileAccess.Write);
                //bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(writeStream);
            }
            catch
            {
                return null;
            }
        }

        private static Task<byte[]?> GetImageJpegAsync(IClipboard clipboard, string format)
        {
            return TryGetFormatAsync(clipboard, format);
        }

        private static Task<byte[]?> GetImagePNGAsync(IClipboard clipboard, string format)
        {
            return TryGetFormatAsync(clipboard, format);
        }

        // https://github.com/AvaloniaUI/Avalonia/discussions/14647
        private static BitmapHeaderInfo ReadBitmapInfo(byte[] data)
        {
            using var memStream = new MemoryStream(data);
            using var reader = new BinaryReader(memStream);
            return new BitmapHeaderInfo
            {
                biSize = reader.ReadUInt32(),
                biWidth = reader.ReadInt32(),
                biHeight = reader.ReadInt32(),
                biPlanes = reader.ReadUInt16(),
                biBitCount = reader.ReadUInt16(),
                biCompression = reader.ReadUInt32(),
                biSizeImage = reader.ReadUInt32(),
                biXPelsPerMeter = reader.ReadInt32(),
                biYPelsPerMeter = reader.ReadInt32(),
                biClrUsed = reader.ReadUInt32(),
                biClrImportant = reader.ReadUInt32()
            };
        }

        private static async Task<byte[]?> TryGetFormatAsync(IClipboard clipboard, string format)
        {
            try
            {
                var obj = await clipboard.GetDataAsync(format);
                var data = obj as byte[];

                if (data == null || data.Length < 4)
                    return null;

                SKBitmap.Decode(data).Dispose();
                return data;
            }
            catch
            {
            }

            return null;
        }

        private class BitmapHeaderInfo
        {
            public ushort biBitCount;
            public uint biClrImportant;
            public uint biClrUsed;
            public uint biCompression;
            public int biHeight;
            public ushort biPlanes;
            public uint biSize;
            public uint biSizeImage;
            public int biWidth;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
        }
    }
}