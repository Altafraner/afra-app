using System.Runtime.InteropServices;
using System.Text.Json;

namespace Altafraner.Typst;

internal class CompilerSafe
{
    private readonly unsafe CsBindgen.Compiler* _inner;
    internal unsafe CompilerSafe(CsBindgen.Compiler* x)
    {
        if (x == null)
        {
            throw new ArgumentNullException();
        }

        _inner = x;
    }

    internal CompileResultSafe Compile()
    {
        unsafe
        {
            return new CompileResultSafe(CsBindgen.NativeMethods.compile(_inner));

        }
    }

    internal unsafe bool SetSysInputs(Dictionary<string, string> inputs)
    {
        bool ok;

        var sysInputsJson = JsonSerializer.Serialize(inputs);

        var sysInputsPtr = Marshal.StringToHGlobalAnsi(sysInputsJson);
        try
        {
            ok = CsBindgen.NativeMethods.set_sys_inputs(_inner, (byte*)sysInputsPtr);
        }
        finally
        {
            Marshal.FreeHGlobal(sysInputsPtr);
        }
        return ok;
    }

    ~CompilerSafe()
    {
        unsafe
        {
            CsBindgen.NativeMethods.free_compiler(_inner);
        }
    }
}
