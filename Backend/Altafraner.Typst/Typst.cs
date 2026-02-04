namespace Altafraner.Typst;

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;

///
public class Typst
{
    private readonly IOptions<TypstConfiguration> _typstConfiguration;
    private readonly ConcurrentDictionary<string, TypstCompilerWrapper> _cachedCompilers;

    ///
    public Typst(IOptions<TypstConfiguration> typstConfiguration)
    {
        _typstConfiguration = typstConfiguration;
        _cachedCompilers = new();
    }

    ///
    public byte[] GeneratePdf(string source, object inputData)
    {
        var compiler = _cachedCompilers.GetOrAdd(
            source,
            s => new TypstCompilerWrapper(
                s,
                _typstConfiguration.Value.TypstFontPaths ?? [],
                _typstConfiguration.Value.TypstResourcePath
            )
        );

        lock (compiler)
        {
            compiler.SetSysInputs(
                new Dictionary<string, string> { { "data", JsonSerializer.Serialize(inputData) } }
            );
            var res = compiler.CompilePdf();
            compiler.SetSysInputs(new Dictionary<string, string>());
            return res;
        }
    }
}
