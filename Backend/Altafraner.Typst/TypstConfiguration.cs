namespace Altafraner.Typst;

///
public class TypstConfiguration
{
    ///
    public required string TypstResourcePath { get; set; }

    ///
    public static bool Validate(TypstConfiguration config)
    {
        return true;
    }
}
