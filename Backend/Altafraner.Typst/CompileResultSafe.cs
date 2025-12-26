using System.Runtime.InteropServices;

internal class CompileResultSafe
{
    private CsBindgen.CompileResult inner;

    internal CompileResultSafe(CsBindgen.CompileResult x)
    {
        inner = x;
    }

    ///
    public string? error
    {
        get
        {
            unsafe
            {
                if (inner.error == null)
                {
                    return null;
                }
                return Marshal.PtrToStringAnsi((nint)inner.error);
            }
        }
    }

    ///
    public List<byte[]> buffers
    {
        get
        {
            unsafe
            {

                List<byte[]>? managedBuffers = new List<byte[]>((int)inner.buffers_len);
                if (inner.buffers != null)
                {
                    for (nuint i = 0; i < inner.buffers_len; i++)
                    {
                        CsBindgen.Buffer buffer = inner.buffers[i];
                        byte[]? managed = new byte[checked((int)buffer.len)];
                        if (buffer.len > 0 && buffer.ptr != null)
                        {
                            Marshal.Copy((IntPtr)buffer.ptr, managed, 0, managed.Length);
                        }
                        managedBuffers.Add(managed);
                    }
                }

                return managedBuffers;
            }


        }
    }

    ~CompileResultSafe()
    {
        CsBindgen.NativeMethods.free_compile_result(inner);
    }
}
