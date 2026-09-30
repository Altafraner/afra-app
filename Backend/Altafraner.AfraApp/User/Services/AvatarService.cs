using System.Net.Mime;
using Altafraner.AfraApp.Files.Services;

namespace Altafraner.AfraApp.User.Services;

/// <summary>
///     A service to handle user avatars
/// </summary>
public class AvatarService
{
    private readonly ImageService _imageService;
    private readonly AfraAppContext _dbContext;
    private const string ImageScope = "user_avatar";

    /// <summary>
    ///     The mime type of scaled images
    /// </summary>
    public const string ScaledMimeType = MediaTypeNames.Image.Webp;

    ///
    public AvatarService(ImageService imageService, AfraAppContext dbContext)
    {
        _imageService = imageService;
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Gets the originally uploaded avatar
    /// </summary>
    public Stream? GetOriginalAvatar(Guid userId)
    {
        return _imageService.GetOriginalImage(ImageScope, [userId.ToString()]);
    }

    /// <summary>
    ///     Gets a scaled version of a user avatar
    /// </summary>
    /// <param name="userId">The id of the user to get the avatar for</param>
    /// <param name="dimension">
    ///     The requested dimension. Scaled Avatars are returned as squares, where the x-length is a
    ///     multiple of 16 px and within the range of [16, 1024].
    /// </param>
    /// <returns>Null, if no avatar for the given user was found</returns>
    public Stream? GetScaledImage(Guid userId, int dimension)
    {
        var roundedDimension = dimension >= 1024 ? 1024 : dimension <= 16 ? 16 : RoundToMultiple(dimension, 16);
        return _imageService.GetResizedImage(ImageScope, [userId.ToString()], roundedDimension, roundedDimension);
    }

    /// <summary>
    ///     Saves a new image for a user
    /// </summary>
    public async Task SaveImageAsync(Guid userId, Stream image)
    {
        var user = await _dbContext.Personen.FindAsync(userId);
        if (user is null) throw new ArgumentException("User not found", nameof(user));
        await _imageService.SaveImageAsync(ImageScope, [userId.ToString()], image);
        user.HasAvatar = true;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Deletes a users avatar
    /// </summary>
    public async Task DeleteImageAsync(Guid userId)
    {
        var user = await _dbContext.Personen.FindAsync(userId);
        if (user is null) throw new ArgumentException("User not found", nameof(user));
        _imageService.DeleteImage(ImageScope, [userId.ToString()]);
        user.HasAvatar = false;
        await _dbContext.SaveChangesAsync();
    }

    private static int RoundToMultiple(int number, int factor)
    {
        var t = factor + number - 1;
        return t - t % factor;
    }
}
