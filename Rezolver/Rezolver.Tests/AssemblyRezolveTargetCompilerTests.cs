using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class AssemblyRezolveTargetCompilerTests : RezolveTargetCompilerTestsBase
	{
		protected override IRezolveTargetCompiler CreateCompilerBase()
		{
			return new AssemblyRezolveTargetCompiler();
		}

		protected override void ReleaseCompiler(IRezolveTargetCompiler compiler)
		{
			AssemblyRezolveTargetCompiler compiler2 = compiler as AssemblyRezolveTargetCompiler;

			compiler2.AssemblyBuilder.Save(compiler2.AssemblyBuilder.GetName().Name + ".dll");
		}

		public class ToRezolve
		{
			public static int InstanceCount = 0;
			public ToRezolve()
			{
				InstanceCount++;
			}
		}

		[TestMethod]
		public void NaiveBenchmark()
		{
			IRezolveTargetCompiler compiler = CreateCompiler();

			ConstructorTarget constructorTarget = ConstructorTarget.Auto<ToRezolve>();
			var rezolverContainer = Mock.Of<IRezolverContainer>();
			ICompiledRezolveTarget target = compiler.CompileTarget(constructorTarget,  rezolverContainer, ExpressionHelper.DynamicContainerParam, null);

			ToRezolve.InstanceCount = 0; 
			var toRezolve = (ToRezolve)target.GetObject();
			Assert.AreEqual(1, ToRezolve.InstanceCount);
			Assert.IsNotNull(toRezolve);

			var toRezolve2 = (ToRezolve) target.GetObjectDynamic(null);
			Assert.AreEqual(2, ToRezolve.InstanceCount);
			Assert.IsNotNull(toRezolve2);

			Assert.AreNotSame(toRezolve, toRezolve2);
			var delegateCompiler = new RezolveTargetDelegateCompiler();
			var del
			 = delegateCompiler.CompileTarget(constructorTarget, rezolverContainer, ExpressionHelper.DynamicContainerParam, null);

			object benchResult = null;
			ToRezolve benchResult2 = null;
			const int counterStart = 1000*10000;
			Stopwatch s = new Stopwatch();
			int counter = counterStart;
			s.Start();
			while (counter-- != 0)
			{
				benchResult = new ToRezolve();
			}
			s.Stop();
			Console.WriteLine("Direct create (object) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = counterStart;
			s.Start();
			while (counter-- != 0)
			{
				benchResult2 = new ToRezolve();
			}
			s.Stop();
			Console.WriteLine("Direct create (no cast) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			//--------------

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = target.GetObject();
			}
			s.Stop();
			Console.WriteLine("Create via interface (object) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = target.GetObjectDynamic(null);
			}
			s.Stop();
			Console.WriteLine("Create via interface impl (object/with container) took {0}ms", s.Elapsed.TotalMilliseconds);
			ToRezolve.InstanceCount = 0;
			GC.Collect(2);
			GC.WaitForFullGCComplete();

			//--------------

			counter = counterStart;
			s.Restart();
			while (counter-- != 0)
			{
				benchResult = del.GetObject();
			}
			s.Stop();
			Console.WriteLine("Create via delegate (object) took {0}ms", s.Elapsed.TotalMilliseconds);
			GC.Collect(2);
			GC.WaitForFullGCComplete();
		}
	}
}
