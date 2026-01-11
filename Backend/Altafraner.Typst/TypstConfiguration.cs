namespace Altafraner.Typst;

/// Configuration for Document generation using Typst
public class TypstConfiguration
{
    /// The root path to build typst documents in
    public required string TypstResourcePath { get; set; }

    /// A List of paths to scan for fonts when building typst documents
    public string[] TypstFontPaths { get; set; } = [];
}
