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
			int _disposeCount = 0;

			public int DisposeCount
			{
				get { return _disposeCount; }
			}

			bool _disposed = false;
			public bool Disposed { get { return _disposed; } }

			public void Dispose()
			{
				++_disposeCount;
				if (!_disposed)
				{
					_disposed = true;
				}
			}
		}

		[TestMethod]
		public void ShouldDisposeOnceOly()
		{
			Mock<IRezolverContainer> parentContainerMock = new Mock<IRezolverContainer>();
			parentContainerMock.Setup(c => c.CreateLifetimeContainer()).Returns(() => new LifetimeRezolverContainer(parentContainerMock.Object));
			parentContainerMock.Setup(c => c.Resolve(typeof (ITestDisposable), null, null)).Returns(() => new DisposableType());
			ITestDisposable instance = null;
			using (var lifetime = parentContainerMock.Object.CreateLifetimeContainer())
			{
				instance = (ITestDisposable) lifetime.Resolve(typeof (ITestDisposable));
			}

			Assert.IsTrue(instance.Disposed);
			Assert.AreEqual(1, instance.DisposeCount);



		}
	}
}
