using System.Runtime.InteropServices;
using CsBindgen;

internal sealed class CompileResultSafe : IDisposable
{
    private readonly CompileResult _inner;
    private bool _disposed;

    internal CompileResultSafe(CompileResult x)
    {
        _inner = x;
    }

    public string? Error
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            unsafe
            {
                return _inner.error == null ? null : Marshal.PtrToStringUTF8((nint)_inner.error);
            }
        }
    }

    public List<byte[]> Buffers
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            unsafe
            {
                var managedBuffers = new List<byte[]>((int)_inner.buffers_len);
                if (_inner.buffers == null) return managedBuffers;

                for (nuint i = 0; i < _inner.buffers_len; i++)
                {
                    var buffer = _inner.buffers[i];
                    var managed = new byte[checked((int)buffer.len)];
                    if (buffer.len > 0 && buffer.ptr != null)
                        Marshal.Copy((IntPtr)buffer.ptr, managed, 0, managed.Length);
                    managedBuffers.Add(managed);
                }

                return managedBuffers;
            }
        }
    }

    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    ~CompileResultSafe()
    {
        Free();
    }

    private void Free()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMethods.free_compile_result(_inner);
    }
}
