#if ENABLE_IL_EMIT
using System.Reflection;
using System.Reflection.Emit;

namespace Rezolver
{
    internal static class DynamicAssemblyHelper
    {
        private static int _counter = 1;
        private const string NameFormat = "Rezolver.Dynamic.{0}.A{1:00000}, Version=0.0";

        internal static (AssemblyBuilder, ModuleBuilder) Create(string baseName)
        {
            var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(string.Format(NameFormat, baseName, _counter++)), AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = builder.DefineDynamicModule("module");
            return (builder, moduleBuilder);
        }
    }
}
#endif
