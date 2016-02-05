using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Rezolver.Tests.vNext.Compilation
{
	public class AssemblyRezolveTargetCompilerTests : RezolveTargetCompilerTestsBase
	{
		protected override IRezolveTargetCompiler CreateCompilerBase(string callingMethod)
		{
			return new AssemblyRezolveTargetCompiler(AssemblyRezolveTargetCompiler.CreateAssemblyBuilder(AssemblyBuilderAccess.RunAndSave));
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
}
