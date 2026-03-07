using System.Runtime.InteropServices;
using System.Text;
using CsBindgen;

namespace Altafraner.Typst;

internal class CompilerSafe
{
    private readonly unsafe CsBindgen.Compiler* _inner;
    internal unsafe CompilerSafe(
                    string? root,
                    string inputSource,
                    IEnumerable<string> fontPaths
                    )
    {
        var inputSourcePtr = StringToHGlobalUtf8(inputSource);
        var rootPtr = IntPtr.Zero;
        if (!string.IsNullOrWhiteSpace(root))
        {
            rootPtr = StringToHGlobalUtf8(root);
        }
        var fontPathsList = fontPaths.ToList();
        var fontPathPtrs = new IntPtr[fontPathsList.Count];
        for (var i = 0; i < fontPathsList.Count; i++)
        {
            fontPathPtrs[i] = StringToHGlobalUtf8(fontPathsList[i]);
        }

        fixed (IntPtr* fontPathsRawPtr = fontPathPtrs)
        {
            var fontPathsPtr = fontPathsList.Count() == 0 ? null : fontPathsRawPtr;
            _inner = NativeMethods.create_compiler(
                (byte*)rootPtr,
                (byte*)inputSourcePtr,
                (byte**)fontPathsPtr,
                (nuint)fontPathsList.Count(),
                false);
            if (_inner == null)
            {
                throw new InvalidOperationException("Failed to create Typst compiler (native create_compiler returned null).");
            }
        }
    }

    internal unsafe CompileResultSafe CompileWithInputsOrNull(string inputs)
    {
        var sysInputsPtr = StringToHGlobalUtf8(inputs);
        try
        {
            return new CompileResultSafe(
                NativeMethods.compile_with_inputs(_inner, (byte*)sysInputsPtr));
        }
        finally
        {
            Marshal.FreeHGlobal(sysInputsPtr);
        }
    }

    ~CompilerSafe()
    {
        unsafe
        {
            NativeMethods.free_compiler(_inner);
        }
    }

    private static IntPtr StringToHGlobalUtf8(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);

        var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Marshal.WriteByte(ptr, bytes.Length, 0);

        return ptr;
    }

    /// Set inputs and compile atomically (thread-safe via Rust-side mutex)
    /// <returns> The binary pdf output </returns>
    public byte[] CompileWithInputs(string inputs)
    {
        var cres = CompileWithInputsOrNull(inputs);
        return cres.Error is null ? cres.Buffers[0] : throw new InvalidOperationException(cres.Error);
    }
}
