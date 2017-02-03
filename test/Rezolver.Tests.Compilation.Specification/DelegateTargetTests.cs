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
			ITarget target = new DelegateTarget<int>(() => 1);
			targets.Register(target);
			var container = CreateContainer(targets);
			
			Assert.Equal(1, container.Resolve<int>());
		}

		[Fact]
		public void DelegateTarget_ReturningConstantIntAsObject()
		{
			var targets = CreateTargetContainer();
			ITarget target = new DelegateTarget<object>(() => 1);
			targets.Register(target);
			var container = CreateContainer(targets);

			Assert.Equal(1, container.Resolve<object>());
		}

		[Fact]
		public void DelegateTarget_ShouldExecuteEachResolveCall()
		{
			var targets = CreateTargetContainer();
			ITarget target = new DelegateTarget<InstanceCountingType>(() => new InstanceCountingType());
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
			targets.Register(f.AsDelegateTarget());
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
			targets.Register(f.AsDelegateTarget());
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
			targets.Register(f.AsDelegateTarget());
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
			targets.Register(f.AsDelegateTarget());
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
			targets.Register(new DelegateTarget<int, string>(DelegateTargetMethod));
			var container = CreateContainer(targets);

			var result = container.Resolve<string>();

			Assert.Equal(DelegateTargetMethod(20), result);
		}
	}
}
