namespace Altafraner.Typst;

using System.Text.Json;
using Microsoft.Extensions.Options;

///
public class Typst
{
    private readonly IOptions<TypstConfiguration> _typstConfiguration;
    private static readonly Dictionary<string, TypstCompilerWrapper> _cachedCompilers = new();

    ///
    public Typst(IOptions<TypstConfiguration> typstConfiguration)
    {
        _typstConfiguration = typstConfiguration;
    }

    ///
    public byte[] generatePdf(string source, object inputdata)
    {
        TypstCompilerWrapper typstCompilerWrapper;
        if (_cachedCompilers.ContainsKey(source))
        {
            typstCompilerWrapper = _cachedCompilers[source];
        }
        else
        {
            typstCompilerWrapper = new TypstCompilerWrapper(source,
                    _typstConfiguration.Value.TypstFontPaths ?? [],
                    _typstConfiguration.Value.TypstResourcePath);
            _cachedCompilers[source] = typstCompilerWrapper;
        }

        typstCompilerWrapper.SetSysInputs(new Dictionary<string, string> { { "data", JsonSerializer.Serialize(inputdata) } });
        var res = typstCompilerWrapper.CompilePdf();
        typstCompilerWrapper.SetSysInputs(new Dictionary<string, string>());
        return res;
    }
}
