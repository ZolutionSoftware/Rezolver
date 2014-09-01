using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class ScopedSingletonTargetTests
	{
		/// <summary>
		/// class demonstrates that types don't have to disposable in order to be scoped singletons.
		/// </summary>
		public class ScopedSingletonTest
		{

		}

		[TestMethod]
		public void ShouldCreateASingleInstanceForAScopeAndChildScopes()
		{
			ScopedSingletonTarget target = new ScopedSingletonTarget(ConstructorTarget.Auto<ScopedSingletonTest>());

			var builder = new RezolverBuilder();
			builder.Register(target);
			var rezolver = new DefaultRezolver(builder, new RezolveTargetDelegateCompiler());
			ScopedSingletonTest obj = null;
			ScopedSingletonTest obj2 = null;
			using(var scope = rezolver.CreateLifetimeScope())
			{
				obj = (ScopedSingletonTest)scope.Resolve(typeof(ScopedSingletonTest));
				obj2 = (ScopedSingletonTest)scope.Resolve(typeof(ScopedSingletonTest));
				Assert.IsNotNull(obj);
				Assert.AreSame(obj, obj2);
				using(var scope2 = scope.CreateLifetimeScope())
				{
					obj2 = (ScopedSingletonTest)scope2.Resolve(typeof(ScopedSingletonTest));

					Assert.AreSame(obj, obj2);
				}

				//create another top-level scope under the rezolver - this should create a new instance
				using(var siblingScope = rezolver.CreateLifetimeScope())
				{
					obj2 = (ScopedSingletonTest)siblingScope.Resolve(typeof(ScopedSingletonTest));
					Assert.AreNotSame(obj, obj2);
				}

			}
		}
	}
}
