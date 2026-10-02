using SkiaSharp;

namespace Altafraner.AfraApp.Files.Services;

/// <summary>
///     A service for handling images
/// </summary>
public class ImageService
{
    private const string FileScope = "img";
    private const int EncodingQuality = 75;
    private readonly FileService _fileService;

    ///
    public ImageService(FileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    ///     Saves an image to the filesystem
    /// </summary>
    /// <param name="scope">The scope the image belongs to</param>
    /// <param name="path">The images path</param>
    /// <param name="stream">The image to save</param>
    public async Task SaveImageAsync(string scope, IEnumerable<string> path, Stream stream)
    {
        if (!stream.CanSeek) throw new ArgumentException("Stream must support Seeking", nameof(stream));
        using var codec = SKCodec.Create(stream, out var result);
        if (codec is null || result != SKCodecResult.Success)
            throw new InvalidOperationException("Failed to create image codec.");
        var pathArray = path as string[] ?? path.ToArray();
        DeleteResizedImages(scope, pathArray);

        await using var file = _fileService.OpenFile(FileScope,
            GetOriginalPath(scope, pathArray),
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None);
        if (file is null) throw new ArgumentNullException(nameof(file));
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(file);
    }

    /// <summary>
    ///     Gets the originally saved image
    /// </summary>
    public Stream? GetOriginalImage(string scope, IEnumerable<string> path)
    {
        var pathArray = path as string[] ?? path.ToArray();
        return _fileService.OpenFile(FileScope, GetOriginalPath(scope, pathArray));
    }

    /// <summary>
    ///     Gets a resized version of an image
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Do not expose this directly via any api surface. It could be used for DOS-Attacks by requesting many
    ///             different sizes of the same image. Validate requested image size externally.
    ///         </item>
    ///         <item>
    ///             The returned stream is not guaranteed to be an image of the requested size. It is only guaranteed to be
    ///             close to the requested aspect ratio and never any bigger than the original
    ///         </item>
    ///     </list>
    /// </remarks>
    public Stream? GetResizedImage(string scope, IEnumerable<string> path, int width, int height)
    {
        var pathArray = path as string[] ?? path.ToArray();
        if (_fileService.CheckExists(FileScope, GetResizedPath(scope, pathArray, width, height)))
            return _fileService.OpenFile(FileScope, GetResizedPath(scope, pathArray, width, height));

        var newPath = ResizeImage(scope, pathArray, width, height);
        if (newPath is null) return null;
        return _fileService.OpenFile(FileScope, newPath);
    }

    /// <summary>
    ///     Deletes an image from the filesystem
    /// </summary>
    public void DeleteImage(string scope, IEnumerable<string> path)
    {
        var pathArray = path as string[] ?? path.ToArray();
        _fileService.TryDeletePath(FileScope, GetOriginalPath(scope, pathArray));
        DeleteResizedImages(scope, pathArray);
    }

    private void DeleteResizedImages(string scope, string[] path)
    {
        var resizeDirectory = GetResizedPath(scope, path, 0, 0).ToList();
        resizeDirectory.RemoveAt(resizeDirectory.Count - 1);
        _fileService.TryDeletePath(FileScope, resizeDirectory);
    }

    private string[]? ResizeImage(string scope, IEnumerable<string> path, int rWidth, int rHeight)
    {
        var pathArray = path as string[] ?? path.ToArray();
        using var originalFile = _fileService.OpenFile(FileScope, GetOriginalPath(scope, pathArray));
        if (originalFile is null) return null;
        using var image = SKBitmap.Decode(originalFile);

        /*
         * - Never upscale an image
         * - Keep the requested aspect ratio but accept some rounding error
         */
        var scalingFactor = MathF.Min(1f, MathF.Min(image.Width / (float)rWidth, image.Height / (float)rHeight));
        var nWidth = (int)MathF.Round(rWidth * scalingFactor);
        var nHeight = (int)MathF.Round(rHeight * scalingFactor);

        var newPath = GetResizedPath(scope, pathArray, nWidth, nHeight).ToArray();

        if (_fileService.CheckExists(FileScope, newPath)) return newPath;

        using var cropped = CenterCropAndResize(image, nWidth, nHeight);
        using var newImage = _fileService.OpenFile("img",
            GetResizedPath(scope, pathArray, nWidth, nHeight),
            FileMode.Create,
            FileAccess.Write);
        if (!cropped.Encode(newImage, SKEncodedImageFormat.Webp, EncodingQuality))
            throw new Exception("Could not encode");

        return newPath;
    }

    private static SKBitmap CenterCropAndResize(SKBitmap original, int nWidth, int nHeight)
    {
        var croppedBitmap = new SKBitmap(nWidth, nHeight, original.ColorType, original.AlphaType);

        var scaleX = (float)nWidth / original.Width;
        var scaleY = (float)nHeight / original.Height;
        var scale = Math.Max(scaleX, scaleY);

        var cropWidth = nWidth / scale;
        var cropHeight = nHeight / scale;
        var cropX = (original.Width - cropWidth) / 2f;
        var cropY = (original.Height - cropHeight) / 2f;

        var sourceRect = SKRect.Create(cropX, cropY, cropWidth, cropHeight);
        var destRect = SKRect.Create(0, 0, nWidth, nHeight);

        var samplingOptions = new SKSamplingOptions(SKCubicResampler.Mitchell);

        using var canvas = new SKCanvas(croppedBitmap);
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        canvas.DrawBitmap(original, sourceRect, destRect, samplingOptions, paint);

        return croppedBitmap;
    }

    private static IEnumerable<string> GetOriginalPath(string scope, IEnumerable<string> path)
    {
        return path.Prepend(scope).Prepend("original");
    }

    private static IEnumerable<string> GetResizedPath(string scope, IEnumerable<string> path, int width, int height)
    {
        return path.Prepend(scope).Prepend("resized").Append($"{width}x{height}");
    }
}
