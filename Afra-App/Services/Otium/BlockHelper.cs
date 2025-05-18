using System.Diagnostics.Contracts;
using Afra_App.Data.Configuration;
using Afra_App.Data.Schuljahr;
using Microsoft.Extensions.Options;

namespace Afra_App.Services.Otium;

/// <summary>
/// A class that provides helper methods for working with block schemas.
/// </summary>
public class BlockHelper
{
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    /// <summary>
    /// Constructor for the BlockHelper class.
    /// </summary>
    /// <param name="otiumConfiguration"></param>
    public BlockHelper(IOptions<OtiumConfiguration> otiumConfiguration)
    {
        _otiumConfiguration = otiumConfiguration;
    }

    /// <summary>
    /// Gets the block metadata for a given schema ID.
    /// </summary>
    /// <param name="schemaId">The schemaId of the Block</param>
    /// <returns>The metadata for the block configuration with the given schema id if exists; Otherwise, null. </returns>
    [Pure]
    public BlockMetadata? Get(char schemaId)
    {
        return _otiumConfiguration.Value.Blocks.FirstOrDefault(b => b.Id == schemaId);
    }
}
