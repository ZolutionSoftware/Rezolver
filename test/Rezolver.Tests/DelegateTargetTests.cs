using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class DelegateTargetTests : TestsBase
	{
		private static int _counter = 0;
		private int _thisID = _counter++;

		[Fact]
		public void ShouldNotAllowNullDelegate()
		{
			//notice here that delegate targets MUST have a target type, because they are generic.
			//we use generic delegates because otherwise we can't guarantee what the underlying type
			//is that's being returned - which we need for IRezolveTarget.

			Assert.Throws<ArgumentNullException>(() => new DelegateTarget<object>((Func<object>)null));
		}

		[Fact]
		public void ShouldSupportAndReturnInt()
		{
			ITarget target = new DelegateTarget<int>(() => 1);
			Assert.True(target.SupportsType(typeof(int)));
			Assert.Equal(1, GetValueFromTarget(target));
		}

		[Fact]
		public void ShouldSupportAndReturnObject()
		{
			ITarget target = new DelegateTarget<object>(() => 1);
			Assert.True(target.SupportsType(typeof(object)));
			Assert.Equal(1, GetValueFromTarget(target));
		}

		[Fact]
		public void ShouldCreateNewInstanceEachCall()
		{
			using (var session = InstanceCountingType.NewSession())
			{
				ITarget target = new DelegateTarget<InstanceCountingType>(() => new InstanceCountingType());
				int currentInstances = InstanceCountingType.InstanceCount;
				var result = GetValueFromTarget(target);
				Assert.Equal(currentInstances + 1, InstanceCountingType.InstanceCount);
				var result2 = GetValueFromTarget(target);
				Assert.Equal(currentInstances + 2, InstanceCountingType.InstanceCount);
			}
		}

		[Fact]
		public void ShouldGetNonNullRezolveContext()
		{
			Func<RezolveContext, Type> f = (RezolveContext rc) => rc.RequestedType;

			var container = CreateContainer();
			container.Register(f.AsDelegateTarget());

			var result = container.Resolve<Type>();
			Assert.NotNull(result);
			Assert.Equal(typeof(Type).AssemblyQualifiedName, result.AssemblyQualifiedName);
		}



		[Fact]
		public void ShouldInvokeWithResolvedInteger()
		{
			Func<int, string> f = i => $"String #{i}";
			var container = CreateContainer();
			container.RegisterObject(5);
			container.Register(f.AsDelegateTarget());

			var result = container.Resolve<string>();
			Assert.Equal("String #5", result);
		}

		[Fact]
		public void ShouldInvokeWithResolvedIntAndContext()
		{
			var container = CreateContainer();
			//create the context here purely so we can check it gets passed through
			var context = new RezolveContext(container, typeof(string));

			Func<RezolveContext, int, string> f = (rc, i) =>
			{
				Assert.NotNull(rc);
				return $"String #{i}";
			};

			container.RegisterObject(10);
			container.Register(f.AsDelegateTarget());


			var result = container.Resolve<string>();
			Assert.Equal("String #10", result);
		}

		[Fact]
		public void ShouldInvokeWithResolvedIntAndContextReversed()
		{
			//The RezolveContext parameter should work regardless of where it is in the parameter list
			var container = CreateContainer();
			//create the context here purely so we can check it gets passed through
			var context = new RezolveContext(container, typeof(string));

			Func<int, RezolveContext, string> f = (i, rc) =>
			{
				Assert.NotNull(rc);
				return $"String #{i}";
			};

			container.RegisterObject(15);
			container.Register(f.AsDelegateTarget());


			var result = container.Resolve<string>();
			Assert.Equal("String #15", result);
		}

		string DelegateTargetMethod(int i)
		{
			return $"Instance {_thisID}, String #{i}";
		}

		string DelegateTargetMethodWithContext(int i, RezolveContext c)
		{
			return $"Instance {_thisID}, String #{i}, for context {c}";
		}

		[Fact]
		public void ShouldInvokeInstanceMethodWithResolvedInt()
		{
			//same as before, except this time, instead of a lambda, we're using an instance method.
			var container = CreateContainer();
			container.RegisterObject(20);
			container.Register(new DelegateTarget<int, string>(DelegateTargetMethod));
			var expected = DelegateTargetMethod(20);

			var result = container.Resolve<string>();
			Assert.Equal(expected, result);
		}

		delegate string NonGenericDelegateWithContext(int i, RezolveContext c);

		[Fact]
		public ShouldInvokeNonGenericDelegateWithResolvedIntAndContext()
		{
			var del = new NonGenericDelegateWithContext(DelegateTargetMethodWithContext);
			var container = CreateContainer();
			//container.RegisterDelegate
		}
	}
}
