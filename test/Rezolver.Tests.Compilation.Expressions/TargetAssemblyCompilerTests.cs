using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Rezolver.Tests.Compilation.Expressions
{
#if DISABLED
  public class TargetAssemblyCompilerTests : TargetCompilerTestsBase
  {

    private static string _baseOutputDir;
    protected override ITargetCompiler CreateCompilerBase(string callingMethod)
    {
      if (string.IsNullOrWhiteSpace(_baseOutputDir))
      {
        _baseOutputDir = Path.Combine(Environment.CurrentDirectory, "_TestAssemblies", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss"));
        if (!Directory.Exists(_baseOutputDir)) Directory.CreateDirectory(_baseOutputDir);
      }
      return new TargetAssemblyCompiler(TargetAssemblyCompiler.CreateAssemblyBuilder(AssemblyBuilderAccess.RunAndSave, _baseOutputDir));
    }

    protected override void ReleaseCompiler(ITargetCompiler compiler)
    {
      TargetAssemblyCompiler compiler2 = compiler as TargetAssemblyCompiler;

      string assemblyFileName = compiler2.AssemblyBuilder.GetName().Name + ".dll";

      try
      {
        //TODO: Get this to create a folder.
        compiler2.AssemblyBuilder.Save(assemblyFileName);
        Debug.WriteLine("Saved {0}", (object)assemblyFileName);
      }
      catch (Exception)
      {
        Debug.WriteLine("Failed to save {0}", (object)assemblyFileName);
      }
    }

  }
#endif
}
