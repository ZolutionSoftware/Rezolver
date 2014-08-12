using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class LifetimeRezolverScopeTests
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

		private static Mock<IRezolver> CreateMock()
		{
			Mock<IRezolver> parentRezolverMock = new Mock<IRezolver>();
			parentRezolverMock.Setup(c => c.CreateLifetimeScope()).Returns(() => new LifetimeScopeRezolver(parentRezolverMock.Object));
			parentRezolverMock.Setup(c => c.Resolve(typeof(ITestDisposable), null, null)).Returns(() => new DisposableType());
			return parentRezolverMock;
		}

		[TestMethod]
		public void ShouldAllowInstanceToBeRegisteredAndDisposed()
		{
			var rezolverMock = Mock.Of<IRezolver>();

			ITestDisposable disposable = null;
			using (var scope = new LifetimeScopeRezolver(rezolverMock))
			{
				scope.Add(disposable = new DisposableType());
			}
		}

		[TestMethod]
		public void ShouldDisposeOnceOly()
		{
			Mock<IRezolver> parentRezolverMock = CreateMock();
			ITestDisposable instance = null;
			using (var lifetime = parentRezolverMock.Object.CreateLifetimeScope())
			{
				instance = (ITestDisposable) lifetime.Resolve(typeof (ITestDisposable));
			}

			Assert.IsTrue(instance.Disposed);
			Assert.AreEqual(1, instance.DisposeCount);
		}

		[TestMethod]
		public void ShouldDisposeTwoInstances()
		{
			var mockContainer = CreateMock();
			int totalInstanceCount = DisposableType.TotalDisposeCount;
			ITestDisposable instance = null;
			ITestDisposable instance2 = null;
			using(var lifetime = mockContainer.Object.CreateLifetimeScope())
			{
				instance = (ITestDisposable)lifetime.Resolve(typeof(ITestDisposable));
				instance2 = (ITestDisposable)lifetime.Resolve(typeof(ITestDisposable));
				Assert.AreNotSame(instance, instance2);
			}

			Assert.IsTrue(instance.Disposed);
			Assert.IsTrue(instance2.Disposed);
			Assert.AreEqual(1, instance.DisposeCount);
			Assert.AreEqual(1, instance2.DisposeCount);
			Assert.AreEqual(totalInstanceCount + 2, DisposableType.TotalDisposeCount);
		}

		[TestMethod]
		public void ShouldDisposeNestedScope()
		{
			var mockContainer = CreateMock();
			ITestDisposable instance = null;
			using(var lifetime = mockContainer.Object.CreateLifetimeScope())
			{
				var lifetime2 = lifetime.CreateLifetimeScope();
				instance = (ITestDisposable)lifetime2.Resolve(typeof(ITestDisposable));
			}

			Assert.IsTrue(instance.Disposed);
			Assert.AreEqual(1, instance.DisposeCount);
		}

		[TestMethod]
		public void ShouldDisposeNestedScopeFirst()
		{
			var mockContainer = CreateMock();
			int totalDisposeCount = DisposableType.TotalDisposeCount;
			ITestDisposable instance = null;
			ITestDisposable instance2 = null;
			using (var lifetime = mockContainer.Object.CreateLifetimeScope())
			{
				instance = (ITestDisposable)lifetime.Resolve(typeof (ITestDisposable));
				using (var lifetime2 = mockContainer.Object.CreateLifetimeScope())
				{
					instance2 = (ITestDisposable) lifetime2.Resolve(typeof (ITestDisposable));
				}
				Assert.IsTrue(instance2.Disposed);
				Assert.IsFalse(instance.Disposed);
			}
			Assert.IsTrue(instance.Disposed);
			Assert.AreEqual(1, instance.DisposeCount);
			Assert.AreEqual(1, instance2.DisposeCount);
			Assert.AreEqual(totalDisposeCount + 2, DisposableType.TotalDisposeCount);
		}

		[TestMethod]
		public void ShouldAutoDisposeNestedScope()
		{
			var mockContainer = CreateMock();
			int totalDisposeCount = DisposableType.TotalDisposeCount;
			ITestDisposable instance = null;
			ITestDisposable instance2 = null;
			using (var lifetime = mockContainer.Object.CreateLifetimeScope())
			{
				instance = (ITestDisposable) lifetime.Resolve(typeof (ITestDisposable));
				//create an inner scope but don't dispose it - it should be auto-disposed by the
				//parent scope when it is disposed.
				var lifetime2 = lifetime.CreateLifetimeScope();
				instance2 = (ITestDisposable)lifetime2.Resolve(typeof (ITestDisposable));
			}

			Assert.IsTrue(instance.Disposed);
			Assert.IsTrue(instance2.Disposed);
			Assert.AreEqual(1, instance.DisposeCount);
			Assert.AreEqual(1, instance2.DisposeCount);
			Assert.AreEqual(totalDisposeCount + 2, DisposableType.TotalDisposeCount);

		}
	}
}
