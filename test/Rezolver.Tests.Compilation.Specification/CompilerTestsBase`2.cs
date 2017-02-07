﻿using Rezolver.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Specification
{
	public abstract partial class CompilerTestsBase<TCompiler, TCompileContextProvider> : CompilerTestsBase
		where TCompiler : ITargetCompiler
		where TCompileContextProvider : ICompileContextProvider
	{
		protected CompilerTestsBase(ITestOutputHelper output)
			: base(output)
		{
			
		}

		[Fact]
		public void Container_ShouldResolveContextProvider()
		{
			var targets = CreateTargetContainer();
			var container = CreateContainer(targets);
			Output.WriteLine("If this test fails, then all other tests in this class will likely fail");
			Assert.IsType<TCompileContextProvider>(container.Resolve<ICompileContextProvider>());
		}

		[Fact]
		public void Container_ShouldResolveCompiler()
		{
			var targets = CreateTargetContainer();
			var container = CreateContainer(targets);
			Output.WriteLine("If this test fails, then all other tests in this class will likely fail");
			Assert.IsType<TCompiler>(container.Resolve<ITargetCompiler>());
		}

		[Fact]
		public void OverridingContainer_ShouldResolveSameContextProvider()
		{
			Output.WriteLine("Testing that the container returned from CreateOverridingContainer can resolve the same context provider as the base container.  If this fails, then any tests to do with overriding containers will fail.");
			var container = CreateContainer(CreateTargetContainer());
			var overrideContainer = CreateOverridingContainer(container);

			Assert.Same(container.Resolve<ICompileContextProvider>(), overrideContainer.Resolve<ICompileContextProvider>());
		}

		[Fact]
		public void OverridingContainer_ShouldResolveSameCompiler()
		{
			Output.WriteLine("Testing that the container returned from CreateOverridingContainer can resolve the same compiler as the base container.  If this fails, then any tests to do with overriding containers will fail.");
			var container = CreateContainer(CreateTargetContainer());
			var overrideContainer = CreateOverridingContainer(container);

			Assert.Same(container.Resolve<ITargetCompiler>(), overrideContainer.Resolve<ITargetCompiler>());
		}
	}
}