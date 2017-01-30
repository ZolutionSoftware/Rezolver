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
		public void AliasShouldWorkForBaseTypes()
		{
			ITargetContainer targets = CreateTargetContainer();
			targets.RegisterObject(1);
			targets.RegisterAlias<object, int>();

			var container = CreateContainer(targets);

			object o = container.Resolve<object>();

			Assert.Equal(1, o);
		}

		[Fact]
		public void AliasShouldWorkForDerivedTypes()
		{
			ITargetContainer targets = CreateTargetContainer();
			targets.RegisterObject(1, typeof(object));
			targets.RegisterAlias<int, object>();

			var container = new Container(targets);
			int i = container.Resolve<int>();

			Assert.Equal(1, i);
		}

		[Fact]
		public void AliasShouldYieldSameSingleton()
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
