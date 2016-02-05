using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Tests.vNext.TestTypes
{
	public interface ITestSession : IDisposable
	{
		int InitialInstanceCount { get; }
	}

	/// <summary>
	/// Base type for types in the tests where we use instance counting as a means to simplify
	/// checking whether an object has actually been constructed or whether it's been served from
	/// a cached object (as with singletons).
	/// 
	/// Deriving type should always be sealed.
	/// </summary>
	/// <typeparam name="TDerived"></typeparam>
	public class InstanceCountingTypeBase<TDerived>
	{
		private class TestSession : ITestSession
		{
			private readonly object _locker;
			/// <summary>
			/// The <see cref="InitialInstanceCount"/> as it was then this session was constructed
			/// </summary>
			public int InitialInstanceCount { get; private set; }
			public TestSession(object locker)
			{
				_locker = locker;
				Monitor.Enter(locker);
				InitialInstanceCount = _instanceCount;
			}

			public void Dispose()
			{
				Monitor.Exit(_locker);
			}
		}


		private static readonly object _locker = new object();

		private static int _instanceCount = 0;

		public static ITestSession NewSession()
		{
			return new TestSession(_locker);
		}

		public static int InstanceCount
		{
			get
			{
				//in theory we allow reading of this property without the use of a session
				//in practise no test should ever need to do that.
				lock (_locker)
				{
					return _instanceCount;
				}
			}
		}
		public InstanceCountingTypeBase()
		{
			if (!Monitor.IsEntered(_locker))
				throw new InvalidOperationException("You must start a new disposable session with a call to NewSession");

			_instanceCount++;
		}

	}

	/// <summary>
	/// a type that simply counts the number of times an instance has been created.
	/// 
	/// Used primarily to test singleton - related functionality in the framework.
	/// 
	/// Typically, the initial instance count is captured at the start of the test, and compared
	/// to an expected value at the end to determine if the correct number of instances have
	/// been constructed.
	/// 
	/// In order to ensure consistency in the InstanceCount property for the current test
	/// and type reuse across multiple tests, you must take an exclusive lock on the underlying
	/// counter by creating a new session by calling NewSession, and then disposing of it. 
	/// </summary>
	public sealed class InstanceCountingType : InstanceCountingTypeBase<InstanceCountingType>
	{
	
	}
}
