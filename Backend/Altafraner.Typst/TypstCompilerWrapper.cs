using System.Runtime.InteropServices;

namespace Altafraner.Typst;

/// <summary>
///     Safe wrapper around the typyst compiler
/// </summary>
internal unsafe class TypstCompilerWrapper
{
    private readonly CompilerSafe _compiler;

    /// <summary>
    ///     Construct a new instance of the typst compiler
    /// </summary>
    internal TypstCompilerWrapper(string inputSource, IEnumerable<string> fontPaths, string? root = null)
    {
        var ignoreSystemFonts = false;

        var inputSourcePtr = Marshal.StringToHGlobalAnsi(inputSource);

        var rootPtr = IntPtr.Zero;
        if (!string.IsNullOrWhiteSpace(root))
        {
            rootPtr = Marshal.StringToHGlobalAnsi(root);
        }

        var fontPathsList = fontPaths.ToList();
        var fontPathPtrs = new IntPtr[fontPathsList.Count];
        for (var i = 0; i < fontPathsList.Count; i++)
        {
            fontPathPtrs[i] = Marshal.StringToHGlobalAnsi(fontPathsList[i]);
        }

        try
        {
            fixed (IntPtr* fontPathsRawPtr = fontPathPtrs)
            {
                var fontPathsPtr = fontPathsList.Count == 0 ? null : fontPathsRawPtr;
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
        var cres = _compiler.Compile();
        return cres.Error is null ? cres.Buffers[0] : throw new InvalidOperationException(cres.Error);
    }


    /// <summary>
    ///     Replace the sysInputs Dictionary used by the typst compiler
    /// </summary>
    public void SetSysInputs(Dictionary<string, string> inputs)
    {
        var ok = _compiler.SetSysInputs(inputs);
        if (!ok)
        {
            throw new Exception("Failed to set system inputs");
        }
    }
}
