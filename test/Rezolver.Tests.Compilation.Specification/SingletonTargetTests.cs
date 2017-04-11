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
        public void ShouldCreateDependantWithSingletonDependenciesInScope()
        {
            //this test arrived from a bug that occurred early on.

            var targets = CreateTargetContainer();
            targets.RegisterSingleton<Disposable>();
            targets.RegisterSingleton<Disposable2>();
            targets.RegisterSingleton<Disposable3>();
            targets.RegisterType<RequiresThreeDisposables>();
            
            var container = CreateContainer(targets);
            using (var scope = container.CreateScope())
            {
                var created = scope.Resolve<RequiresThreeDisposables>();
                Assert.NotNull(created);
            }
        }

        [Fact]
		public void SingletonTarget_ShouldOnlyCreateOneInstance_ViaConstructorTarget()
		{
			var targets = CreateTargetContainer();
			//RegisterSingleton simply creates ConstructorTarget wrapped inside a singleton
			targets.RegisterSingleton<InstanceCountingType>();
			var container = CreateContainer(targets);

			using(var session = InstanceCountingType.NewSession())
			{
				var result1 = container.Resolve<InstanceCountingType>();
				var result2 = container.Resolve<InstanceCountingType>();

				Assert.Same(result1, result2);
			}
		}

		[Fact]
		public void SingletonTarget_ShouldOnlyResolveOneInstance_ViaResolvedTarget()
		{
			//little bit wacky - register the instance counting type as before,
			//but register a ResolvedTarget (wrapped in a singleton) for a base of
			//the same type.  This is basically how aliasing works (just without 
			//the singleton)

			//when resolving directly, we should always get a new instance
			//when resolving via the base, we should only ever get one.

			var targets = CreateTargetContainer();
			targets.RegisterType<InstanceCountingType>();
			//now register the ResolvedTarget singleton against IInstanceCountingType
			targets.Register(new ResolvedTarget(typeof(InstanceCountingType)).Singleton(), typeof(IInstanceCountingType));

			var container = CreateContainer(targets);

			using (var session = InstanceCountingType.NewSession())
			{
				//although we're not testing 'normal' transient objects - we need some
				//assertions on this part of the test to ensure reliable results.
				var direct1 = container.Resolve<InstanceCountingType>();
				var direct2 = container.Resolve<InstanceCountingType>();
				Assert.NotSame(direct1, direct2);
				var currentInstanceCount = session.InitialInstanceCount + 2;
				Assert.Equal(currentInstanceCount, InstanceCountingType.InstanceCount);

				//Right - on to the main course.
				//This should create a new instance, because the singleton is wrapped around
				//the inner ResolveTarget.
				var indirect1 = container.Resolve<IInstanceCountingType>();
				Assert.NotEqual(currentInstanceCount, indirect1.ThisInstanceID);
				var indirect2 = container.Resolve<IInstanceCountingType>();
				Assert.Same(indirect1, indirect2);
			}
		}

		[Fact]
		public void SingletonTarget_ShouldCreateUniqueInstanceForEachClosedGeneric_ViaGenericCtorTarget()
		{
			//one generic target=multiple singletons=per closed generic

			var targets = CreateTargetContainer();
			//can use RegisterSingleton for Open Generics too
			targets.RegisterSingleton(typeof(Generic<>));
			var container = CreateContainer(targets);

			var r1 = container.Resolve<Generic<int>>();
			var r2 = container.Resolve<Generic<int>>();
			Assert.Same(r1, r2);

			//I mean, obviously a Generic<int> cannot be equal to Generic<string>, but,
			//the test is more likely to crash as opposed to return identical instance
			var r3 = container.Resolve<Generic<string>>();
			var r4 = container.Resolve<Generic<string>>();
			Assert.NotSame(r1, r3);
			Assert.Same(r3, r4);

			//and then re fetch the first type to make sure that it's not getting confused
			Assert.Same(r1, container.Resolve<Generic<int>>());
		}
	}
}
