using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
	public class TargetTestsBase
	{
		public ITestOutputHelper Output { get; }

		public TargetTestsBase(ITestOutputHelper output)
		{
			this.Output = output;
		}

		protected virtual IContainer GetDefaultContainer(ITargetContainer targets = null)
		{
			return new Container(targets);
		}

		protected virtual ITargetContainer GetDefaultTargetContainer(IContainer existingContainer = null)
		{
            if (existingContainer is ITargetContainer targets)
                return targets;
			return new TargetContainer();
		}

		/// <summary>
		/// Gets the compile context for the specified target under test.
		/// 
		/// The <see cref="ICompileContext.TargetType"/> is automatically set to the <see cref="ITarget.DeclaredType"/>
		/// of the passed target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="container">The container to use for the <see cref="IResolveContext"/> from which the compile context
		/// will be created.  If null, then the <see cref="GetDefaultContainer"/> method is called.</param>
		/// <param name="targets">The target container to use for the compile context.  If null, then the <paramref name="container"/>
		/// will be passed to the <see cref="GetDefaultTargetContainer(IContainer)"/> method (including if one is automatically
		/// built) - with the target container that's returned being used instead.</param>
		protected virtual ICompileContext GetCompileContext(ITarget target, IContainer container = null, ITargetContainer targets = null, Type targetType = null)
		{
			targets = targets ?? GetDefaultTargetContainer(container);
            container = container ?? GetDefaultContainer(targets);
            // to create a context we resolve the ITargetCompiler from the container.
            // this is usually an internal operation within the container itself.
            return targets.GetOption<ITargetCompiler>()
                .CreateContext(new ResolveContext(container, targetType ?? target.DeclaredType), targets);
		}
	}
}
