using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Specification
{
    public abstract partial class CompilerTestsBase
    {
		/// <summary>
		/// Gets the output helper supplied by the XUnit test runner.
		/// </summary>
		/// <value>The output.</value>
		protected ITestOutputHelper Output { get; }
		protected CompilerTestsBase(ITestOutputHelper output)
		{
			Output = output;
		}

		/// <summary>
		/// Creates the target container for the test
		/// </summary>
		/// <param name="testName">Name of the test.</param>
		protected abstract ITargetContainer CreateTargetContainer([CallerMemberName]string testName = null);

		/// <summary>
		/// Creates the container for the test or theory
		/// </summary>
		/// <param name="targets">The targets to be used for the container.</param>
		/// <param name="testName">Name of the test.</param>
		protected virtual IContainer CreateContainer(ITargetContainer targets, [CallerMemberName]string testName = null)
		{
			return new Container(targets);
		}

	}
}
