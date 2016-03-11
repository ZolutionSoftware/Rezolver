using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Rezolver.Tests.Compilation
{
#if !DOTNET
	public class AssemblyRezolveTargetCompilerTests : RezolveTargetCompilerTestsBase
	{
		private static string _baseOutputDir;
		protected override IRezolveTargetCompiler CreateCompilerBase(string callingMethod)
		{
			if (string.IsNullOrWhiteSpace(_baseOutputDir))
			{
				_baseOutputDir = Path.Combine(Environment.CurrentDirectory, "_TestAssemblies", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss"));
				if (!Directory.Exists(_baseOutputDir)) Directory.CreateDirectory(_baseOutputDir);
			}

			return new AssemblyRezolveTargetCompiler(AssemblyRezolveTargetCompiler.CreateAssemblyBuilder(AssemblyBuilderAccess.RunAndSave, _baseOutputDir));
		}

		protected override void ReleaseCompiler(IRezolveTargetCompiler compiler)
		{
			AssemblyRezolveTargetCompiler compiler2 = compiler as AssemblyRezolveTargetCompiler;

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
