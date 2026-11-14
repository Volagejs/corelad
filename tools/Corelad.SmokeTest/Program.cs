using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Corel.Interop.VGCore;

namespace Corelad.SmokeTest
{
    internal static class Program
    {
        private const string CorelInteropAssembly = "Corel.Interop.VGCore";

        [STAThread]
        private static int Main(string[] args)
        {
            // Corel interop DLL'i çıktı klasörüne kopyalanmaz (Private=false);
            // çalışma zamanında kurulu CorelDRAW klasöründen yüklenir.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveCorelInterop;

            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                if (args != null && args.Length > 0 &&
                    string.Equals(args[0], "--probe", StringComparison.OrdinalIgnoreCase))
                {
                    return CurveProbe.Run();
                }

                return Runner.Run();
            }
            catch (Exception exception)
            {
                Console.WriteLine("[FAIL] Smoke harness beklenmeyen istisna ile durdu:");
                Console.WriteLine(exception);
                return 1;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Assembly ResolveCorelInterop(object sender, ResolveEventArgs args)
        {
            var requested = new AssemblyName(args.Name).Name;
            if (!string.Equals(requested, CorelInteropAssembly, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var programsDir = Environment.GetEnvironmentVariable("COREL_PROGRAMS64");
            if (string.IsNullOrWhiteSpace(programsDir))
            {
                programsDir = @"C:\Program Files\Corel\CorelDRAW Graphics Suite\27\Programs64";
            }

            var candidate = System.IO.Path.Combine(programsDir, "Assemblies", CorelInteropAssembly + ".dll");
            return System.IO.File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
        }
    }
}
