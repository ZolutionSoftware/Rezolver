using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
		[Fact]
		public void ContainerScope_ShouldDispose_CtorTargetDisposable()
		{
			var container = CreateContainerForSingleTarget(ConstructorTarget.Auto<Disposable>());

			Disposable result;
			//will change the API to use a member
			using(var scope = new ContainerScope(container))
			{
				result = scope.Resolve<Disposable>();
			}

			Assert.True(result.Disposed);
		}
    }
}
