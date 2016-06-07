using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class ObjectTargetTests : TestsBase
	{
		[Fact]
		public void ShouldWrapNull()
		{
			ITarget target = new ObjectTarget(null);
			Assert.Null(GetValueFromTarget<object>(target));
		}

		[Fact]
		public void ShouldWrapNonNull()
		{
			ITarget target = new ObjectTarget("Hello world");
			Assert.Equal("Hello world", GetValueFromTarget<string>(target));
		}

		[Fact]
		public void ShouldWrapNullableWithNonNullable()
		{
			ITarget target = new ObjectTarget(1, typeof(int?));
			Assert.True(target.SupportsType(typeof(int?)));
			Assert.Equal((int?)1, GetValueFromTarget<int?>(target));
		}

		[Fact]
		public void ShouldAllowAnyBaseAsTargetType()
		{
			ITarget target = new ObjectTarget("hello world", typeof(IEnumerable<char>));
			ITarget target2 = new ObjectTarget("hello world", typeof(object));

			string expected = "hello world";
			Assert.Equal(expected, GetValueFromTarget<IEnumerable<char>>(target));
			Assert.Equal(expected, GetValueFromTarget<object>(target));
		}

		[Fact]
		public void ShouldNotAllowIncorrectDeclaredType()
		{
			Assert.Throws<ArgumentException>(() => new ObjectTarget("Hello world", typeof(int)));
		}

		[Fact]
		public void ShouldRequireTypeParamInSupportsType()
		{
			ITarget target = new ObjectTarget("Hello world");
			Assert.Throws<ArgumentNullException>(() => target.SupportsType(null));
		}

		[Fact]
		public void Extension_ShouldDeriveDeclaredType()
		{
			ITarget target = (1).AsObjectTarget();

			Assert.Equal(typeof(int), target.DeclaredType);
			Assert.True(target.SupportsType(typeof(int)));
		}

		[Fact]
		public void Extension_ShouldAllowBaseType()
		{
			ITarget target = (1).AsObjectTarget(typeof(object));
			Assert.Equal(typeof(object), target.DeclaredType);
			Assert.Equal((object)1, GetValueFromTarget(target));
		}

		[Fact]
		public void ShouldSupportClosedGeneric()
		{
			ObjectTarget target = new ObjectTarget(new int[0], typeof(IEnumerable<int>));
			Assert.True(target.SupportsType(typeof(IEnumerable<int>)));
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
		public void ShouldNotDisposeByDefault()
		{
			var myDisposable = new MyDisposable();
			using (var rezolver = new ScopedContainer())
			{
				rezolver.RegisterObject(myDisposable);
				var instance = rezolver.Resolve<MyDisposable>();
			}

			myDisposable.Dispose();
		}

		[Fact]
		public void ShouldNotDisposeMultipleTimes()
		{
			var myDisposable = new MyDisposable();
			//test targeted specifically at a piece of functionality I currently know not to work.
			//an objecttarget should behave like a SingletonTarget in terms of how it tracks in a scope.
			using (var rezolver = new ScopedContainer())
			{
				rezolver.RegisterObject(myDisposable, suppressScopeTracking: false);
				using (var childScope = rezolver.CreateLifetimeScope())
				{
					var instance = childScope.Resolve<MyDisposable>();
					//should not dispose here when scope is disposed
				}
				var instance2 = rezolver.Resolve<MyDisposable>();
				//should dispose here when root scope is disposed
			}

			//should already be disposed here.
			Assert.Throws<ObjectDisposedException>(() => myDisposable.Dispose());
		}
	}
}
