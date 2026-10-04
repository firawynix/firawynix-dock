using System.Collections.Concurrent;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using SkiaSharp;
using Svg.Skia;

namespace FirawynixDock;

internal static class ImageStore
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FirawynixDock", "images");
    private static readonly ConcurrentDictionary<string, Task<Image?>> Images = new();
    private static readonly SemaphoreSlim Downloads = new(4);
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static Task<Image?> GetAsync(string? url)
    {
        if (url is null) return Task.FromResult<Image?>(null);
        return Images.GetOrAdd(url, LoadAsync);
    }

    private static async Task<Image?> LoadAsync(string url)
    {
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)));
        var path = Path.Combine(Folder, key + ".img");
        try
        {
            if (File.Exists(path)) return Decode(await File.ReadAllBytesAsync(path), url);
        }
        catch (Exception) { /* Tenta novamente pela rede. */ }

        await Downloads.WaitAsync();
        try
        {
            using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is > 3_000_000) return null;
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var buffer = new MemoryStream();
            var chunk = new byte[32_768];
            int read;
            while ((read = await stream.ReadAsync(chunk)) > 0)
            {
                if (buffer.Length + read > 3_000_000) return null;
                buffer.Write(chunk, 0, read);
            }
            var bytes = buffer.ToArray();
            var image = Decode(bytes, url);
            if (image is null) return null;
            Directory.CreateDirectory(Folder);
            await File.WriteAllBytesAsync(path, bytes);
            return image;
        }
        catch (Exception) { return null; }
        finally { Downloads.Release(); }
    }

    private static Image? Decode(byte[] bytes, string url)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            using var original = Image.FromStream(stream);
            return new Bitmap(original);
        }
        catch (Exception) { /* Skia decodifica WebP e SVG. */ }

        try
        {
            if (url.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                using var svg = new SKSvg();
                using var stream = new MemoryStream(bytes);
                var picture = svg.Load(stream);
                if (picture is null || picture.CullRect.Width <= 0 || picture.CullRect.Height <= 0)
                    return null;
                using var bitmap = new SKBitmap(256, 256);
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.Transparent);
                var bounds = picture.CullRect;
                var scale = Math.Min(240f / bounds.Width, 240f / bounds.Height);
                canvas.Translate(128f - bounds.MidX * scale, 128f - bounds.MidY * scale);
                canvas.Scale(scale);
                canvas.DrawPicture(picture);
                using var png = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);
                using var imageStream = new MemoryStream(png.ToArray());
                using var original = Image.FromStream(imageStream);
                return new Bitmap(original);
            }
            using var decoded = SKBitmap.Decode(bytes);
            if (decoded is null) return null;
            using var encoded = SKImage.FromBitmap(decoded).Encode(SKEncodedImageFormat.Png, 100);
            using var pngStream = new MemoryStream(encoded.ToArray());
            using var raster = Image.FromStream(pngStream);
            return new Bitmap(raster);
        }
        catch (Exception) { return null; }
    }
}
