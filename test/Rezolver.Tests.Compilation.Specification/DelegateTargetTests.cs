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
		private static int _nextID = 0;

		private int _thisID = ++_nextID;

		[Fact]
		public void DelegateTarget_ReturningConstantInt()
		{
			var targets = CreateTargetContainer();
			ITarget target = Target.ForDelegate(() => 1);
			targets.Register(target);
			var container = CreateContainer(targets);
			
			Assert.Equal(1, container.Resolve<int>());
		}

		[Fact]
		public void DelegateTarget_ReturningConstantIntAsObject()
		{
			var targets = CreateTargetContainer();
			ITarget target = Target.ForDelegate<object>(() => 1);
			targets.Register(target);
			var container = CreateContainer(targets);

			Assert.Equal(1, container.Resolve<object>());
		}

		[Fact]
		public void DelegateTarget_ShouldExecuteEachResolveCall()
		{
			var targets = CreateTargetContainer();
			ITarget target = Target.ForDelegate(() => new InstanceCountingType());
			targets.Register(target);
			var container = CreateContainer(targets);
			using (var session = InstanceCountingType.NewSession())
			{
				var result = container.Resolve<InstanceCountingType>();
				Assert.Equal(session.InitialInstanceCount + 1, result.ThisInstanceID);
				var result2 = container.Resolve<InstanceCountingType>();
				Assert.Equal(session.InitialInstanceCount + 2, result2.ThisInstanceID);
			}
		}

		[Fact]
		public void DelegateTarget_ShouldInject_ResolveContext()
		{
			Func<ResolveContext, RequiresResolveContext> f = (ResolveContext rc) => new RequiresResolveContext(rc);
			var targets = CreateTargetContainer();
			targets.RegisterDelegate(f);
			var container = CreateContainer(targets);

			var result = container.Resolve<RequiresResolveContext>();
			Assert.NotNull(result);
			Assert.NotNull(result.Context);
			Assert.Equal(typeof(RequiresResolveContext), result.Context.RequestedType);
			Assert.Same(container, result.Context.Container);
		}



		[Fact]
		public void DelegateTarget_ShouldInject_Int()
		{
			Func<int, string> f = i => $"String #{i}";
			var targets = CreateTargetContainer();
			targets.RegisterObject(5);
			targets.RegisterDelegate(f);
			var container = CreateContainer(targets);

			var result = container.Resolve<string>();
			Assert.Equal("String #5", result);
		}

		[Fact]
		public void DelegateTarget_ShouldInject_ResolveContext_Int()
		{
			Func<ResolveContext, int, string> f = (rc, i) =>
			{
				Assert.NotNull(rc);
				return $"String #{i}";
			};

			var targets = CreateTargetContainer();
			targets.RegisterObject(10);
			targets.RegisterDelegate(f);
			var container = CreateContainer(targets);
			
			//looks the same as above, but note there's an assertion in the delegate
			var result = container.Resolve<string>();

			Assert.Equal("String #10", result);
		}

		[Fact]
		public void DelegateTarget_ShouldInject_Int_ResolveContext()
		{
			//same as above, just the parameters are reversed - checking that there's no cheating going on
			Func<int, ResolveContext, string> f = (i, rc) =>
			{
				Assert.NotNull(rc);
				return $"String #{i}";
			};

			var targets = CreateTargetContainer();
			targets.RegisterObject(10);
			targets.RegisterDelegate(f);
			var container = CreateContainer(targets);

			//looks the same as above, but note there's an assertion in the delegate
			var result = container.Resolve<string>();

			Assert.Equal("String #10", result);
		}

		string DelegateTargetMethod(int i)
		{
			return $"Instance {_thisID}, String #{i}";
		}

		string DelegateTargetMethodWithContext(int i, ResolveContext c)
		{
			return $"Instance {_thisID}, String #{i}, for context {c}";
		}

		[Fact]
		public void DelegateTarget_InstanceMethod_ShouldInject_Int()
		{
			//same as before, except this time, instead of a lambda, we're using an instance method.
			var targets = CreateTargetContainer();
			targets.RegisterObject(20);
			targets.RegisterDelegate<int, string>(DelegateTargetMethod);
			var container = CreateContainer(targets);

			var result = container.Resolve<string>();

			Assert.Equal(DelegateTargetMethod(20), result);
		}

		[Fact]
		public void DelegateTarget_ShouldResolve_ThroughInjectedResolveContext()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<TaxService>();
			targets.RegisterDelegate(rc => new RequiresTaxService(rc.Resolve<TaxService>(), 0.175M));

			var container = CreateContainer(targets);
			var result = container.Resolve<RequiresTaxService>();
			Assert.Equal(117.5M, result.CalculatePrice(100M));
		}

        [Fact]
        public void DelegateTarget_ShouldUseScopeForExplicitlyResolveObject()
        {
            var targets = CreateTargetContainer();
            targets.RegisterType<Disposable>();
            targets.RegisterType<Disposable2>();
            targets.RegisterType<Disposable3>();
            //note that the delegate is using the resolve-live API on ResolveContext
            //and therefore the scope will be honoured automatically.
            targets.RegisterDelegate(rc => new RequiresThreeDisposables(
                rc.Resolve<Disposable>(),
                rc.Resolve<Disposable2>(),
                rc.Resolve<Disposable3>()));

            RequiresThreeDisposables result;
            using (var container = CreateScopedContainer(targets))
            {
                result = container.Resolve<RequiresThreeDisposables>();
            }

            Assert.True(result.First.Disposed);
            Assert.True(result.Second.Disposed);
            Assert.True(result.Third.Disposed);
        }

        //TODO: do the requiresscopeanddisposable 

	}
}
