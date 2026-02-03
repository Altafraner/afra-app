using System.Runtime.InteropServices;
using CsBindgen;

internal class CompileResultSafe
{
    private readonly CompileResult _inner;

    internal CompileResultSafe(CompileResult x)
    {
        _inner = x;
    }

    public string? Error
    {
        get
        {
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
            unsafe
            {
                var managedBuffers = new List<byte[]>((int)_inner.buffers_len);
                if (_inner.buffers == null)
                    return managedBuffers;

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

    ~CompileResultSafe()
    {
        NativeMethods.free_compile_result(_inner);
    }
}
