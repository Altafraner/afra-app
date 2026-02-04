using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Altafraner.Typst.Generator;

/// <summary>
///   A Source generator for converting '.typ' files to static classes
/// </summary>
[Generator]
public class TypstTemplateGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var textFiles = context.AdditionalTextsProvider.Where(static file =>
            file.Path.EndsWith(".typ")
        );
        var namesAndContents = textFiles.Select(
            (text, cancellationToken) =>
                (
                    name: Path.GetFileNameWithoutExtension(text.Path),
                    Directory: Path.GetFileName(Path.GetDirectoryName(text.Path)),
                    content: text.GetText(cancellationToken)!.ToString()
                )
        );
        context.RegisterSourceOutput(
            namesAndContents,
            (spc, source) =>
            {
                var (name, directory, content) = source;
                var safeContent = SymbolDisplay.FormatLiteral(content, true);
                spc.AddSource(
                    directory + "__" + name,
                    $$"""
                    #pragma warning disable CS1591
                    namespace Altafraner.Typst.Templates;

                    public static partial class {{directory}}
                    {
                        public const string {{name}} = {{safeContent}};
                    }
                    #pragma warning restore CS1591
                    """
                );
            }
        );
    }
}
