using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class ScopedContainerTests
	{
		public class DisposableBase : IDisposable
		{
			public bool Disposed { get; private set; }

			public virtual void Dispose()
			{
				//
				if (Disposed)
					throw new ObjectDisposedException(GetType().Name);

				Disposed = true;
			}
		}

		public class SingletonDependency : DisposableBase
		{

		}

		public class Disposable1 : DisposableBase
		{

		}

		public class Disposable2 : DisposableBase
		{

		}

		public class RequiresDisposables : DisposableBase
		{

			public RequiresDisposables(Disposable1 disposable1, Disposable2 disposable2)
			{
				this.Disposable1 = disposable1;
				this.Disposable2 = disposable2;
			}

			public Disposable1 Disposable1 { get; private set; }
			public Disposable2 Disposable2 { get; private set; }
		}

		[Fact]
		public void ShouldDisposeTransientsResolvedAsDependencies()
		{
			Disposable1 disp1; Disposable2 disp2; RequiresDisposables reqDisp;

			using (var container = new ScopedContainer())
			{
				container.RegisterType<Disposable1>();
				container.RegisterType<Disposable2>();
				container.RegisterType<RequiresDisposables>();

				reqDisp = container.Resolve<RequiresDisposables>();
				disp1 = reqDisp.Disposable1;
				disp2 = reqDisp.Disposable2;
			}

			Assert.True(reqDisp.Disposed);
			Assert.True(disp1.Disposed);
			Assert.True(disp2.Disposed);
		}

		[Fact]
		public void ShouldDisposeTransientCreatedInChildScope()
		{
			Disposable1 disp1, disp1a;

			using (var container = new ScopedContainer())
			{
				container.RegisterType<Disposable1>();
				disp1 = container.Resolve<Disposable1>();
				using (var childContainer = container.CreateLifetimeScope())
				{
					disp1a = childContainer.Resolve<Disposable1>();
				}
				Assert.True(disp1a.Disposed);
				Assert.False(disp1.Disposed);
			}

			Assert.True(disp1.Disposed);
		}

		/// <summary>
		/// this is a test scenario taken from the DNX AspNet.DependencyInjection library.
		/// </summary>
		[Fact]
		public void SingletonShouldComeFromRootContainer()
		{
			SingletonDependency root, dep1, dep2;
			//the key element of this test is that we have a singleton object that is not explicitly registered
			//as scoped.  This means that, naturally, the first scope to resolve the instance will want to track
			//it.  In fact, however, it should be tracked in the root-level scoping container because it has 
			//effectively global lifetime.

			//this has interesting side effects for object targets, which are also effectively singletons - except
			//code outside the container will have created it first.  Who should dispose of those?
			using (var container = new ScopedContainer())
			{
				container.Builder.Register(new SingletonTarget(ConstructorTarget.Auto<SingletonDependency>()));

				using (var childScope = container.CreateLifetimeScope())
				{
					dep1 = childScope.Resolve<SingletonDependency>();
				}
				Assert.False(dep1.Disposed);

				using (var childScope = container.CreateLifetimeScope())
				{
					dep2 = childScope.Resolve<SingletonDependency>();
				}
				Assert.False(dep2.Disposed);

				root = container.Resolve<SingletonDependency>();

				Assert.Same(root, dep1);
				Assert.Same(dep1, dep2);
			}

			Assert.True(root.Disposed);
		}
	}
}
