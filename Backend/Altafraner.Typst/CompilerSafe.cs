using System.Runtime.InteropServices;

using System.Text.Json;

internal class CompilerSafe
{
    private unsafe CsBindgen.Compiler* inner;
    internal unsafe CompilerSafe(CsBindgen.Compiler* x)
    {
        if (x == null)
        {
            throw new ArgumentNullException();
        }
        inner = x;
    }

    internal CompileResultSafe compile()
    {
        unsafe
        {
            return new(CsBindgen.NativeMethods.compile(inner));

        }
    }

    internal unsafe bool setSysInputs(Dictionary<string, string> inputs)
    {
        bool ok;

        System.String? sysInputsJson = JsonSerializer.Serialize<Dictionary<string, string>>(inputs);

        nint sysInputsPtr = Marshal.StringToHGlobalAnsi(sysInputsJson);
        try
        {
            ok = CsBindgen.NativeMethods.set_sys_inputs(inner, (byte*)sysInputsPtr);
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
            CsBindgen.NativeMethods.free_compiler(inner);
        }
    }
}
