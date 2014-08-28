using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

		private static EndlessProducer<T> MakeEndlessProducer<T>(Func<T> producerFunc)
		{
			return new EndlessProducer<T>(producerFunc);
		}

		public class EndlessProducer<T>
		{
			private readonly IEnumerator<T> _enumerator;
			public EndlessProducer(Func<T> producerFunc)
			{
				_enumerator = MakeEnumerable(producerFunc).GetEnumerator();
			}

			public T Next()
			{
				_enumerator.MoveNext();
				return _enumerator.Current;
			}

			private IEnumerable<T> MakeEnumerable(Func<T> producerFunc)
			{
				while (true)
				{
					yield return producerFunc();
				}
			}
		}

		private static Mock<IRezolver> CreateMock()
		{
			Mock<IRezolver> parentRezolverMock = new Mock<IRezolver>();
			var producer1 = MakeEndlessProducer(() => new DisposableType());
			Expression<Func<DisposableType>> e = () => producer1.Next();

			parentRezolverMock.Setup(c => c.CreateLifetimeScope()).Returns(() => new LifetimeScopeRezolver(parentRezolverMock.Object));
			parentRezolverMock.Setup(c => c.Fetch(typeof(ITestDisposable), null)).Returns(new ExpressionTarget(e.Body));
			parentRezolverMock.Setup(c => c.Resolve(It.Is((RezolveContext r) => r.RequestedType == typeof(ITestDisposable)))).Returns(producer1.Next);
			parentRezolverMock.Setup(c => c.Compiler).Returns(new RezolveTargetDelegateCompiler());
			return parentRezolverMock;
		}

		[TestMethod]
		public void ShouldAllowInstanceToBeRegisteredWithoutContextAndDisposed()
		{
			var rezolverMock = Mock.Of<IRezolver>();

			ITestDisposable disposable = null;
			using (var scope = new LifetimeScopeRezolver(rezolverMock))
			{
				scope.AddToScope(disposable = new DisposableType());
			}
			Assert.IsTrue(disposable.Disposed);
		}

		[TestMethod]
		public void ShouldAllowInstanceToBeRegisteredWithContextAndDisposed()
		{
			Assert.Fail("Not yet written");
		}

		[TestMethod]
		public void ShouldConfirmNoInstanceAlreadyRegistered()
		{
			var rezolverMock = Mock.Of<IRezolver>();

			using (var scope = new LifetimeScopeRezolver(rezolverMock))
			{
				Assert.Fail("Not yet written");
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
