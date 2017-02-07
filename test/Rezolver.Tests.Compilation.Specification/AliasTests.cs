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
		//An alias is a synthetic target created by wrapping a ChangeTypeTarget 
		//around a ResolvedTarget

		//These tests check that the compiler can support this combination

		[Fact]
		public void Alias_ShouldWorkForBaseTypes()
		{
			ITargetContainer targets = CreateTargetContainer();
			targets.RegisterObject(1);
			targets.RegisterAlias<object, int>();

			var container = CreateContainer(targets);

			object o = container.Resolve<object>();

			Assert.Equal(1, o);
		}

		[Fact]
		public void Alias_ShouldWorkForDerivedTypes()
		{
			ITargetContainer targets = CreateTargetContainer();
			targets.RegisterObject(1, typeof(object));
			targets.RegisterAlias<int, object>();

			var container = CreateContainer(targets);
			int i = container.Resolve<int>();

			Assert.Equal(1, i);
		}

		[Fact]
		public void Alias_ShouldYieldSameSingleton()
		{
			//this test can fail if the compiler is not working properly for singletons, too

			ITargetContainer targets = CreateTargetContainer();
			targets.RegisterSingleton<InstanceCountingType>();
			targets.RegisterAlias<IInstanceCountingType, InstanceCountingType>();

			using (var session = InstanceCountingType.NewSession())
			{
				var container = CreateContainer(targets);
				var first = container.Resolve<InstanceCountingType>();
				var second = container.Resolve<IInstanceCountingType>();

				Assert.Equal(session.InitialInstanceCount + 1, first.ThisInstanceID);
				Assert.Equal(first.ThisInstanceID, second.ThisInstanceID);
				Assert.Same(first, second);
			}
		}
	}
}
