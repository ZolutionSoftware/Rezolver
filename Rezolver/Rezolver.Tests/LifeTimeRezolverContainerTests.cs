using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class LifetimeRezolverContainerTests
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

		private static Mock<IRezolverContainer> CreateMock()
		{
			Mock<IRezolverContainer> parentContainerMock = new Mock<IRezolverContainer>();
			parentContainerMock.Setup(c => c.CreateLifetimeContainer()).Returns(() => new LifetimeRezolverContainer(parentContainerMock.Object));
			parentContainerMock.Setup(c => c.Resolve(typeof(ITestDisposable), null, null)).Returns(() => new DisposableType());
			return parentContainerMock;
		}

		[TestMethod]
		public void ShouldDisposeOnceOly()
		{
			Mock<IRezolverContainer> parentContainerMock = CreateMock();
			ITestDisposable instance = null;
			using (var lifetime = parentContainerMock.Object.CreateLifetimeContainer())
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
			using(var lifetime = mockContainer.Object.CreateLifetimeContainer())
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
			using(var lifetime = mockContainer.Object.CreateLifetimeContainer())
			{
				var lifetime2 = lifetime.CreateLifetimeContainer();
				instance = (ITestDisposable)lifetime2.Resolve(typeof(ITestDisposable));
			}

			Assert.IsTrue(instance.Disposed);
			Assert.AreEqual(1, instance.DisposeCount);
		}
	}
}
