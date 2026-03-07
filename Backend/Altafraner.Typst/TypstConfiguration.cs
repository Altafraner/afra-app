namespace Altafraner.Typst;

/// Configuration for Document generation using Typst
public class TypstConfiguration
{
    /// The root path to build typst documents in
    public required string TypstResourcePath { get; set; }

    /// A List of paths to scan for fonts when building typst documents
    public string[] TypstFontPaths { get; set; } = [];

    /// The maximum number of concurrent typst compilations
    public int NumThreads { get; set; } = 1;

    /// <returns>True, iff the configuration looks valid</returns>
    public static bool Validate(TypstConfiguration config)
    {
        if (config.NumThreads < 1) return false;
        return true;
    }
}
