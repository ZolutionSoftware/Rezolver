using Rezolver.Tests.Examples.Types;
using Xunit;

namespace Rezolver.Tests.Examples
{
	public class ObjectExamples
	{
		[Fact]
		public void ShouldRegisterAndRetrieveByItsType()
		{
			//<example1>
			var container = new Container();

			var service = new MyService();
			container.RegisterObject(service);

			Assert.Same(service, container.Resolve<MyService>());
			//</example1>
		}

		[Fact]
		public void ShouldRegisterAndRetrieveByInterface()
		{
			//<example2>
			var container = new Container();

			var service = new MyService();
			container.RegisterObject<IMyService>(service);

			// NOTE: Could also use:
			// container.RegisterObject(service, typeof(IMyService)

			Assert.Same(service, container.Resolve<IMyService>());
			//</example2>
		}

		[Fact]
		public void ShouldNotDisposeByDefault()
		{
			//<example10>
			var disposableObj = new DisposableType();

			using (var container = new ScopedContainer())
			{
				container.RegisterObject(disposableObj);
				var result = container.Resolve<DisposableType>();
				Assert.Same(disposableObj, result);
			}
			// Should NOT be disposed
			Assert.False(disposableObj.Disposed);
			//</example10>
		}

		[Fact]
		public void ShouldDispose()
		{
			//<example11>
			var disposableObj = new DisposableType();

			using (var container = new ScopedContainer())
			{
				container.RegisterObject(disposableObj, scopeBehaviour: ScopeBehaviour.Explicit);
				var result = container.Resolve<DisposableType>();
				Assert.Same(disposableObj, result);
			}
			// Should be disposed
			Assert.True(disposableObj.Disposed);
			//</example11>
		}

		[Fact]
		public void OnlyRootScopeShouldDispose()
		{
			//<example12>
			var disposableObj = new DisposableType();

			using (var container = new ScopedContainer())
			{
				container.RegisterObject(disposableObj, scopeBehaviour: ScopeBehaviour.Explicit);

				using (var scope = container.CreateScope())
				{
					var result = container.Resolve<DisposableType>();
					Assert.Same(disposableObj, result);
				}
				// Should not be disposed here...
				Assert.False(disposableObj.Disposed);
			}
			// ... but should be disposed here
			Assert.True(disposableObj.Disposed);
			//</example12>
		}
	}
}
