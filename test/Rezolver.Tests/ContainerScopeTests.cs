using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class ContainerScopeTests
	{
		//this is testing the container scope's low level behaviour, which
		//does not require a container to work - just a delegate that creates
		//an object, and 

		[Fact]
		public void ShouldDisposeImplicitIDisposable()
		{
			Disposable disposable;

			using (var scope = new ContainerScope(new Container()))
			{
				disposable = scope.Resolve<Disposable>(
					new ResolveContext(scope, typeof(Disposable)),
					c => new Disposable(),
					ScopeActivationBehaviour.Implicit);
			}

			Assert.True(disposable.Disposed);
			Assert.Equal(1, disposable.DisposedCount);
		}

		[Fact]
		public void ShouldDisposeExplicitIDisposable()
		{
			Disposable disposable;

			using (var scope = new ContainerScope(new Container()))
			{
				disposable = scope.Resolve<Disposable>(
					new ResolveContext(scope, typeof(Disposable)),
					c => new Disposable(),
					ScopeActivationBehaviour.Explicit);
			}

			Assert.True(disposable.Disposed);
			Assert.Equal(1, disposable.DisposedCount);
		}

		[Fact]
		public void ChildScopeShouldDisposeItsOwnImplicitDisposable()
		{
			using (var scope = new ContainerScope(new Container()))
			{
				Disposable inner1, inner2;
				Func<ResolveContext, object> factory = c => new Disposable();

				using (var childScope = scope.CreateScope())
				{
					inner1 = childScope.Resolve<Disposable>(
						new ResolveContext(childScope, typeof(Disposable)),
						factory,
						ScopeActivationBehaviour.Implicit);
				}

				Assert.True(inner1.Disposed);
				Assert.Equal(1, inner1.DisposedCount);

				using (var childScope = scope.CreateScope())
				{
					inner2 = childScope.Resolve<Disposable>(
						new ResolveContext(childScope, typeof(Disposable)),
						factory,
						ScopeActivationBehaviour.Implicit);
				}

				Assert.True(inner2.Disposed);
				Assert.Equal(1, inner2.DisposedCount);
			}
		}

		[Fact]
		public void ShouldCreateOnlyOneExplicitNonDisposable()
		{
			using (var scope = new ContainerScope(new Container()))
			{
				Func<ResolveContext, object> factory = c => new NoCtor();

				var result = scope.Resolve<NoCtor>(
					new ResolveContext(scope, typeof(NoCtor)),
					factory,
					ScopeActivationBehaviour.Explicit);

				Assert.NotNull(result);

				var result2 = scope.Resolve<NoCtor>(
					new ResolveContext(scope, typeof(NoCtor)),
					factory,
					ScopeActivationBehaviour.Explicit);

				Assert.Same(result, result2);
			}
		}

		[Fact]
		public void ChildScopeShouldGetItsOwnExplicitInstance()
		{
			using (var scope = new ContainerScope(new Container()))
			{
				Func<ResolveContext, object> factory = c => new NoCtor();

				var result = scope.Resolve<NoCtor>(
					new ResolveContext(scope, typeof(NoCtor)),
					factory,
					ScopeActivationBehaviour.Explicit);

				Assert.NotNull(result);

				using (var childScope = scope.CreateScope())
				{
					var result2 = childScope.Resolve<NoCtor>(
						new ResolveContext(childScope, typeof(NoCtor)),
						factory,
						ScopeActivationBehaviour.Explicit);

					Assert.NotSame(result, result2);
				}
			}
		}

		[Fact]
		public void ImplicitlyScopedObjectShouldNotBeSameAsExplicitlyScopedObject()
		{
			Disposable explicitlyScoped, implicitlyScoped;
			using(var scope = new ContainerScope(new Container()))
			{
				Func<ResolveContext, object> factory = c => new Disposable();
				explicitlyScoped = scope.Resolve<Disposable>(
					new ResolveContext(scope, typeof(Disposable)),
					factory,
					ScopeActivationBehaviour.Explicit);

				implicitlyScoped = scope.Resolve<Disposable>(
					new ResolveContext(scope, typeof(Disposable)),
					factory,
					ScopeActivationBehaviour.Implicit);

				Assert.NotSame(explicitlyScoped, implicitlyScoped);
			}
			Assert.True(explicitlyScoped.Disposed);
			Assert.True(implicitlyScoped.Disposed);
		}

		[Fact]
		public void ParentScopeShouldDisposeAllChildScopesAndObjects()
		{
			Func<ResolveContext, object> factory = c => new Disposable();
			Disposable parentObj, childObj, grandChildObj, siblingObj;

			using (var parent = new ContainerScope(new Container()))
			{
				var child = parent.CreateScope();
				var grandChild = child.CreateScope();
				var sibling = parent.CreateScope();

				parentObj = parent.Resolve<Disposable>(
					new ResolveContext(parent, typeof(Disposable)),
					factory,
					ScopeActivationBehaviour.Implicit);

				childObj = child.Resolve<Disposable>(
					new ResolveContext(child, typeof(Disposable)),
					factory,
					ScopeActivationBehaviour.Implicit);

				grandChildObj = grandChild.Resolve<Disposable>(
					new ResolveContext(grandChild, typeof(Disposable)),
					factory,
					ScopeActivationBehaviour.Implicit);

				siblingObj = sibling.Resolve<Disposable>(
					new ResolveContext(sibling, typeof(Disposable)),
					factory,
					ScopeActivationBehaviour.Implicit);
			}

			Assert.All(new[] { parentObj, childObj, grandChildObj, siblingObj },
				d => {
					Assert.True(d.Disposed);
					Assert.Equal(1, d.DisposedCount);
				});
		}
	}
}
