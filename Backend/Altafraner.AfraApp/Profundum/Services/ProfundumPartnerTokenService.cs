using System.Security.Cryptography;
using Altafraner.AfraApp.Profundum.Configuration;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services;

/// <summary>
///     Generates human-shareable Partnerwahl invite tokens (e.g. "apfel-baum-schnee") from the wordlist configured
///     via <see cref="ProfundumConfiguration.PartnerWahlWordlistPath" />. Scoped to the Partnerwahl feature only -
///     see <see cref="ProfundumPartnerService" />.
/// </summary>
internal class ProfundumPartnerTokenService
{
    private readonly string[] _woerter;

    public ProfundumPartnerTokenService(IOptions<ProfundumConfiguration> config)
    {
        _woerter = File.ReadAllLines(config.Value.PartnerWahlWordlistPath)
            .Select(w => w.Trim().ToLowerInvariant())
            .Where(w => w.Length > 0)
            .ToArray();
        if (_woerter.Length == 0)
            throw new InvalidOperationException(
                $"Die Partnerwahl-Wortliste unter '{config.Value.PartnerWahlWordlistPath}' ist leer.");
    }

    /// <summary>
    ///     Generates a new token from <paramref name="wordCount" /> randomly drawn words, joined with hyphens.
    /// </summary>
    public string GenerateToken(int wordCount)
    {
        var gewaehlt = new string[wordCount];
        for (var i = 0; i < wordCount; i++)
            gewaehlt[i] = _woerter[RandomNumberGenerator.GetInt32(_woerter.Length)];
        return string.Join('-', gewaehlt);
    }
}
