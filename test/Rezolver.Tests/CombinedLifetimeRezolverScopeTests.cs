﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class CombinedLifetimeRezolverScopeTests
    {
		public interface ITestDisposable : IDisposable
		{
			int DisposeCount { get; }
			bool Disposed { get; }
		}

		public class DisposableType : ITestDisposable
		{
			public static int TotalDisposeCount = 0;
			int _disposeCount = 0;

			public int DisposeCount
			{
				get { return _disposeCount; }
			}

			bool _disposed = false;
			public bool Disposed { get { return _disposed; } }

			public void Dispose()
			{
				++TotalDisposeCount;
				++_disposeCount;
				if (!_disposed)
				{
					_disposed = true;
				}
			}
		}

		[Fact]
		public void ShouldAllowInstanceToBeRegisteredWithoutContextAndDisposed()
		{
			var rezolver = new DefaultRezolver();

			ITestDisposable disposable = null;
			using (var scope = new CombinedLifetimeScopeRezolver(null, inner: rezolver))
			{
				scope.AddToScope(disposable = new DisposableType());
			}
			Assert.True(disposable.Disposed);
		}

		[Fact]
		public void ShouldAllowInstanceToBeRegisteredWithContextAndDisposed()
		{
			var rezolver = new DefaultRezolver();
			ITestDisposable disposable = null;
			using (var scope = new CombinedLifetimeScopeRezolver(null, inner: rezolver))
			{
				scope.AddToScope(disposable = new DisposableType(), new RezolveContext(rezolver, typeof(ITestDisposable)));
			}
			Assert.True(disposable.Disposed);
		}

		[Fact]
		public void ShouldConfirmNoInstanceAlreadyRegistered()
		{
			var rezolver = new DefaultRezolver();
			ITestDisposable disposable = null;
			var context = new RezolveContext(rezolver, typeof(DisposableType));
			using (var scope = new CombinedLifetimeScopeRezolver(null, inner: rezolver))
			{
				scope.AddToScope(disposable = new DisposableType(), context);

				var returned = scope.GetSingleFromScope(context);
				Assert.Same(disposable, returned);
			}

			Assert.True(disposable.Disposed);
		}

		[Fact]
		public void ShouldDisposeOnceOnly()
		{
			DefaultRezolver parentRezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			parentRezolver.Register(ConstructorTarget.Auto<DisposableType>(), typeof(ITestDisposable));
			ITestDisposable instance;
			using (var lifetime = parentRezolver.CreateLifetimeScope())
			{
				instance = (ITestDisposable)lifetime.Resolve(typeof(ITestDisposable));
			}

			Assert.True(instance.Disposed);
			Assert.Equal(1, instance.DisposeCount);
		}

		[Fact]
		public void ShouldDisposeTwoInstances()
		{
			DefaultRezolver parentRezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			parentRezolver.Register(ConstructorTarget.Auto<DisposableType>(), typeof(ITestDisposable));
			int totalInstanceCount = DisposableType.TotalDisposeCount;
			ITestDisposable instance = null;
			ITestDisposable instance2 = null;
			using (var lifetime = parentRezolver.CreateLifetimeScope())
			{
				instance = (ITestDisposable)lifetime.Resolve(typeof(ITestDisposable));
				instance2 = (ITestDisposable)lifetime.Resolve(typeof(ITestDisposable));
				Assert.NotSame(instance, instance2);
			}

			Assert.True(instance.Disposed);
			Assert.True(instance2.Disposed);
			Assert.Equal(1, instance.DisposeCount);
			Assert.Equal(1, instance2.DisposeCount);
			Assert.Equal(totalInstanceCount + 2, DisposableType.TotalDisposeCount);
		}

		[Fact]
		public void ShouldDisposeNestedScope()
		{
			DefaultRezolver parentRezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			parentRezolver.Register(ConstructorTarget.Auto<DisposableType>(), typeof(ITestDisposable));
			ITestDisposable instance = null;
			using (var lifetime = parentRezolver.CreateLifetimeScope())
			{
				var lifetime2 = lifetime.CreateLifetimeScope();
				instance = (ITestDisposable)lifetime2.Resolve(typeof(ITestDisposable));
			}

			Assert.True(instance.Disposed);
			Assert.Equal(1, instance.DisposeCount);
		}

		[Fact]
		public void ShouldDisposeNestedScopeFirst()
		{
			DefaultRezolver parentRezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			parentRezolver.Register(ConstructorTarget.Auto<DisposableType>(), typeof(ITestDisposable));
			int totalDisposeCount = DisposableType.TotalDisposeCount;
			ITestDisposable instance = null;
			ITestDisposable instance2 = null;
			using (var lifetime = parentRezolver.CreateLifetimeScope())
			{
				instance = (ITestDisposable)lifetime.Resolve(typeof(ITestDisposable));
				using (var lifetime2 = lifetime.CreateLifetimeScope())
				{
					instance2 = (ITestDisposable)lifetime2.Resolve(typeof(ITestDisposable));
				}
				Assert.True(instance2.Disposed);
				Assert.False(instance.Disposed);
			}
			Assert.True(instance.Disposed);
			Assert.Equal(1, instance.DisposeCount);
			Assert.Equal(1, instance2.DisposeCount);
			Assert.Equal(totalDisposeCount + 2, DisposableType.TotalDisposeCount);
		}

		[Fact]
		public void ShouldAutoDisposeNestedScope()
		{
			DefaultRezolver parentRezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			parentRezolver.Register(ConstructorTarget.Auto<DisposableType>(), typeof(ITestDisposable));
			int totalDisposeCount = DisposableType.TotalDisposeCount;
			ITestDisposable instance = null;
			ITestDisposable instance2 = null;
			using (var lifetime = parentRezolver.CreateLifetimeScope())
			{
				instance = (ITestDisposable)lifetime.Resolve(typeof(ITestDisposable));
				//create an inner scope but don't dispose it - it should be auto-disposed by the
				//parent scope when it is disposed.
				var lifetime2 = lifetime.CreateLifetimeScope();
				instance2 = (ITestDisposable)lifetime2.Resolve(typeof(ITestDisposable));
			}

			Assert.True(instance.Disposed);
			Assert.True(instance2.Disposed);
			Assert.Equal(1, instance.DisposeCount);
			Assert.Equal(1, instance2.DisposeCount);
			Assert.Equal(totalDisposeCount + 2, DisposableType.TotalDisposeCount);

		}
	}
}