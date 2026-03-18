namespace Altafraner.Typst;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Options;

///
public static class Telemetry
{
    ///
    public static readonly ActivitySource ActivitySource = new("Altafraner.Typst");
}

///
public class Typst
{
    private readonly TypstConfiguration _config;
    private readonly int _threadCount;
    private int _nextThread;
    private readonly ConcurrentDictionary<string, CompilerSafe>[] _compilerCaches;

    ///
    public Typst(IOptions<TypstConfiguration> typstConfiguration)
    {
        _config = typstConfiguration.Value;

        _threadCount = _config.NumThreads;

        _compilerCaches = new ConcurrentDictionary<string, CompilerSafe>[_threadCount];

        for (int i = 0; i < _threadCount; i++)
        {
            _compilerCaches[i] = new ConcurrentDictionary<string, CompilerSafe>();
        }
    }

    ///
    public byte[] GeneratePdf(string source, object inputData)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("GeneratePdf", ActivityKind.Internal);
        var threadIndex = (int)((uint)Interlocked.Increment(ref _nextThread) % _threadCount);
        var cache = _compilerCaches[threadIndex];
        var compiler = cache.GetOrAdd(source, s =>
            new CompilerSafe(
                _config.TypstResourcePath,
                s,
                _config.TypstFontPaths ?? []
            )
        );
        var res = compiler.CompileWithInputs(JsonSerializer.Serialize(inputData));
        activity?.SetStatus(ActivityStatusCode.Ok);
        return res;
    }
}
