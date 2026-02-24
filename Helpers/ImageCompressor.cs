using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace SmartWinners.Helpers;

public class ImageCompressor
{
    public static async Task<(string base64, string contentType)> CompressBase64ImageAsync(string base64Image, int maxWidth = 2000, int maxHeight = 2000)
    {
        var match = Regex.Match(base64Image, @"data:(image\/[a-zA-Z]+);base64,");
        var contentType = match.Success ? match.Groups[1].Value : "image/jpeg";

        var base64Data = Regex.Replace(base64Image, @"^data:image\/[a-zA-Z]+;base64,", "");

        var imageBytes = Convert.FromBase64String(base64Data);

        using var inputStream = new MemoryStream(imageBytes);
        using var image = await Image.LoadAsync(inputStream);

        if (image.Width > maxWidth || image.Height > maxHeight)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));
        }

        await using var outputStream = new MemoryStream();

        if (contentType == "image/png")
        {
            var encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.Level6 };
            await image.SaveAsync(outputStream, encoder);
        }
        else // default to JPEG
        {
            var encoder = new JpegEncoder { Quality = 70 };
            await image.SaveAsync(outputStream, encoder);
            contentType = "image/jpeg";
        }

        var compressedBase64 = Convert.ToBase64String(outputStream.ToArray());
        var result = $"data:{contentType};base64,{compressedBase64}";

        return (compressedBase64, contentType);
    }
}