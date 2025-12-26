using System.Runtime.InteropServices;

namespace Altafraner.Typst;

public record Fonts(
    bool IncludeSystemFonts = true,
    IEnumerable<string>? FontPaths = null
);

/// <summary>
///     Safe wrapper around the typyst compiler
/// </summary>
public unsafe class TypstCompilerWrapper
{
    private CompilerSafe _compiler;

    /// <summary>
    ///     Construct a new instance of the typst compiler
    /// </summary>
    public TypstCompilerWrapper(string inputSource, Fonts? fonts = null, string? root = null)
    {
        fonts ??= new Fonts();
        var fontPaths = fonts.FontPaths ?? Enumerable.Empty<string>();
        bool ignoreSystemFonts = !fonts.IncludeSystemFonts;

        nint inputSourcePtr = inputSource != null ? Marshal.StringToHGlobalAnsi(inputSource) : IntPtr.Zero;

        IntPtr rootPtr = IntPtr.Zero;
        if (!string.IsNullOrWhiteSpace(root))
        {
            rootPtr = Marshal.StringToHGlobalAnsi(root);
        }

        List<string>? fontPathsList = fontPaths.ToList();
        nint[]? fontPathPtrs = new IntPtr[fontPathsList.Count];
        for (int i = 0; i < fontPathsList.Count; i++)
        {
            fontPathPtrs[i] = Marshal.StringToHGlobalAnsi(fontPathsList[i]);
        }

        try
        {
            fixed (IntPtr* fontPathsRawPtr = fontPathPtrs)
            {
                IntPtr* fontPathsPtr = fontPathsList.Count == 0 ? null : fontPathsRawPtr;
                _compiler = new CompilerSafe(CsBindgen.NativeMethods.create_compiler(
                    (byte*)rootPtr,
                    (byte*)inputSourcePtr,
                    (byte**)fontPathsPtr,
                    (nuint)fontPathsList.Count,
                    ignoreSystemFonts));
            }
        }
        finally
        {
            if (rootPtr != IntPtr.Zero) Marshal.FreeHGlobal(rootPtr);
            if (inputSourcePtr != IntPtr.Zero) Marshal.FreeHGlobal(inputSourcePtr);
            foreach (var ptr in fontPathPtrs) Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>
    ///     Compile the document
    /// </summary>
    /// <returns> The binary pdf output </returns>
    public byte[] CompilePdf()
    {
        CompileResultSafe cres = _compiler.compile();
        if (cres.error is not null)
        {
            throw new InvalidOperationException(cres.error);
        }
        return cres.buffers[0];
    }


    /// <summary>
    ///     Replace the sysInputs Dictionary used by the typst compiler
    /// </summary>
    public void SetSysInputs(Dictionary<string, string> inputs)
    {
        bool ok = _compiler.setSysInputs(inputs);
        if (!ok)
        {
            throw new Exception("Failed to set system inputs");
        }
    }
}
