namespace Altafraner.Typst;

using System.Text.Json;
using Microsoft.Extensions.Options;

///
public class Typst
{
    private readonly IOptions<TypstConfiguration> _typstConfiguration;
    private readonly Dictionary<string, TypstCompilerWrapper> _cachedCompilers;

    ///
    public Typst(IOptions<TypstConfiguration> typstConfiguration)
    {
        _typstConfiguration = typstConfiguration;
        _cachedCompilers = new();
    }

    ///
    public byte[] generatePdf(string source, object inputdata)
    {
        var typstCompilerWrapper = _cachedCompilers.GetValueOrDefault(source);
        if (typstCompilerWrapper is null)
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
