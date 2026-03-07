using System.Runtime.InteropServices;
using System.Text;
using CsBindgen;

namespace Altafraner.Typst;

internal sealed class CompilerSafe : IDisposable
{
    private unsafe CsBindgen.Compiler* _inner;
    private bool _disposed;

    internal unsafe CompilerSafe(
                    string? root,
                    string inputSource,
                    IEnumerable<string> fontPaths
                    )
    {
        IntPtr inputSourcePtr = IntPtr.Zero;
        IntPtr rootPtr = IntPtr.Zero;
        var fontPathsList = fontPaths.ToList();
        var fontPathPtrs = new IntPtr[fontPathsList.Count];
        try
        {
            inputSourcePtr = StringToHGlobalUtf8(inputSource);
            if (!string.IsNullOrWhiteSpace(root))
                rootPtr = StringToHGlobalUtf8(root);

            for (int i = 0; i < fontPathsList.Count; i++)
                fontPathPtrs[i] = StringToHGlobalUtf8(fontPathsList[i]);
            fixed (IntPtr* fontPathsRawPtr = fontPathPtrs)
            {
                var fontPathsPtr = fontPathsList.Count == 0 ? null : fontPathsRawPtr;
                _inner = NativeMethods.create_compiler(
                    (byte*)rootPtr,
                    (byte*)inputSourcePtr,
                    (byte**)fontPathsPtr,
                    (nuint)fontPathsList.Count,
                    false);
                if (_inner == null)
                {
                    throw new InvalidOperationException("Failed to create Typst compiler (native create_compiler returned null).");
                }
            }
        }
        finally
        {
            if (inputSourcePtr != IntPtr.Zero) Marshal.FreeHGlobal(inputSourcePtr);
            if (rootPtr != IntPtr.Zero) Marshal.FreeHGlobal(rootPtr);
            foreach (var ptr in fontPathPtrs)
            {
                if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
            }
        }
    }

    internal unsafe CompileResultSafe CompileWithInputsOrNull(string inputs)
    {
        var inputsPtr = StringToHGlobalUtf8(inputs);
        try
        {
            return new CompileResultSafe(
                NativeMethods.compile_with_inputs(_inner, (byte*)inputsPtr));
        }
        finally
        {
            Marshal.FreeHGlobal(inputsPtr);
        }
    }

    /// Set inputs and compile atomically (thread-safe via Rust-side mutex)
    /// <returns> The binary pdf output </returns>
    public byte[] CompileWithInputs(string inputs)
    {
        var cres = CompileWithInputsOrNull(inputs);

        if (cres.Error != null)
            throw new InvalidOperationException(cres.Error);

        return cres.Buffers[0];
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (_inner != null)
        {
            NativeMethods.free_compiler(_inner);
            _inner = null;
        }
        _disposed = true;
    }

    ~CompilerSafe()
    {
        Dispose(false);
    }

    private static IntPtr StringToHGlobalUtf8(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);

        var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Marshal.WriteByte(ptr, bytes.Length, 0);

        return ptr;
    }
}
