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
			using (var container = new ScopedContainer())
			{
				container.RegisterObject(myDisposable);
				var instance = container.Resolve<MyDisposable>();
			}

			myDisposable.Dispose();
		}

		[Fact]
		public void ObjectTarget_ShouldNotDisposeMultipleTimes()
		{
			var myDisposable = new MyDisposable();
			//test targeted specifically at a piece of functionality I currently know not to work.
			//an objecttarget should behave like a SingletonTarget in terms of how it tracks in a scope.
			using (var container = new ScopedContainer())
			{
				container.RegisterObject(myDisposable, suppressScopeTracking: false);
				using (var childScope = container.CreateLifetimeScope())
				{
					var instance = childScope.Resolve<MyDisposable>();
					//should not dispose here when scope is disposed
				}
				var instance2 = container.Resolve<MyDisposable>();
				//should dispose here when root scope is disposed
			}

			//should already be disposed here.
			Assert.Throws<ObjectDisposedException>(() => myDisposable.Dispose());
		}
	}
}
