namespace Altafraner.Typst;

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;

///
public class Typst
{
    private readonly IOptions<TypstConfiguration> _typstConfiguration;
    private readonly ConcurrentDictionary<string, CompilerSafe> _cachedCompilers;

    ///
    public Typst(IOptions<TypstConfiguration> typstConfiguration)
    {
        _typstConfiguration = typstConfiguration;
        _cachedCompilers = new();
    }

    ///
    public byte[] GeneratePdf(string source, object inputData)
    {
        var compiler = _cachedCompilers.GetOrAdd(source, s =>
            new CompilerSafe(_typstConfiguration.Value.TypstResourcePath, s, _typstConfiguration.Value.TypstFontPaths ?? [])
        );
        return compiler.CompileWithInputs(JsonSerializer.Serialize(inputData));
    }
}
