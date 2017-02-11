using Rezolver.Targets;
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
		public void ObjectTarget_ShouldWrapNull()
		{
			var container = CreateContainerForSingleTarget(new ObjectTarget(null));
			Assert.Null(container.Resolve<object>());
		}

		[Fact]
		public void ObjectTarget_ShouldWrapNonNull()
		{
			var container = CreateContainerForSingleTarget(new ObjectTarget("Hello world"));
			Assert.Equal("Hello world", container.Resolve<string>());
		}

		[Fact]
		public void ObjectTarget_ShouldAllowNullableForValueType()
		{
			var container = CreateContainerForSingleTarget(new ObjectTarget(1, typeof(int?)));

			Assert.Equal((int?)1, container.Resolve<int?>());
		}

		[Fact]
		public void ObjectTarget_ShouldAllowBaseAsTargetType()
		{
			var container = CreateContainerForSingleTarget(new ObjectTarget("hello world", typeof(IEnumerable<char>)));
			Assert.Equal("hello world", container.Resolve<IEnumerable<char>>());
		}

		private class MyDisposable : IDisposable
		{
			#region IDisposable Support
			private bool disposedValue = false; // To detect redundant calls

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					disposedValue = true;
				}
				else
					throw new ObjectDisposedException("MyDisposable");
			}

			// This code added to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(true);
			}
			#endregion
		}

		[Fact]
		public void ObjectTarget_ShouldNotDisposeByDefault()
		{
			var myDisposable = new MyDisposable();
			var targets = CreateTargetContainer();
			targets.RegisterObject(myDisposable);
			var container = CreateContainer(targets);
			using (var scope = container.CreateScope())
			{
				var instance = scope.Resolve<MyDisposable>();
			}

			myDisposable.Dispose();
		}

		[Fact]
		public void ObjectTarget_ShouldNotDisposeMultipleTimes()
		{
			var myDisposable = new MyDisposable();
			var targets = CreateTargetContainer();
			targets.RegisterObject(myDisposable, scopeBehaviour: ScopeActivationBehaviour.Explicit);
			var container = CreateContainer(targets);
			using (var scope = container.CreateScope())
			{
				using (var childScope = scope.CreateScope())
				{
					var instance = childScope.Resolve<MyDisposable>();
					//should not dispose here when scope is disposed because 
					//we default to the root scope.
					//if it does, then when instance2 is disposed we'll get an exception we're not expecting
				}
				var instance2 = scope.Resolve<MyDisposable>();
				//should dispose here when root scope is disposed
			}

			//should already be disposed here.
			Assert.Throws<ObjectDisposedException>(() => myDisposable.Dispose());
		}
	}
}
