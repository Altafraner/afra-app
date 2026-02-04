namespace Altafraner.AfraApp.Domain;

/// <summary>
/// An exception representing an NotFound return code
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <inheritdoc/>
    public NotFoundException(string message)
        : base(message) { }
}
