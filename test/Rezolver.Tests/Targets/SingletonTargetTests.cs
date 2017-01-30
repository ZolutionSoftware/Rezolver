using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Rezolver.Targets;

namespace Rezolver.Tests.Targets
{
	public class SingletonTargetTests : TargetTestsBase
	{
		public SingletonTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullTarget()
		{
			Assert.Throws<ArgumentNullException>(() => new SingletonTarget(null));
		}

		[Fact]
		public void ShouldNotAllowInnerSingletonTarget()
		{
			//actually, this is a little bit silly because it can't check for singletons nested inside 
			//non-singleton targets.  That said, it steps silly mistakes.
			Assert.Throws<ArgumentException>(() => new SingletonTarget(new SingletonTarget(new TestTarget())));
		}

		[Fact]
		public void ShouldAssignTargetFromConstructor()
		{
			var inner = new TestTarget();
			Assert.Same(inner, new SingletonTarget(inner).InnerTarget);
		}

		[Fact]
		public void ShouldInheritInnerTargetsDeclaredType()
		{
			Assert.Equal(typeof(string),
				new SingletonTarget(new TestTarget(typeof(string), useFallBack: false, supportsType: true)).DeclaredType);
		}

		[Fact]
		public void GetOrAddInitialiserShouldNotAllowNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => new SingletonTarget(new TestTarget()).GetOrAddInitialiser(null, t => null));
			Assert.Throws<ArgumentNullException>(() => new SingletonTarget(new TestTarget()).GetOrAddInitialiser(typeof(string), null));
		}

		[Fact]
		public void ShouldExecuteInitialiserFactoryIfNewInitialiser()
		{
			var target = new SingletonTarget(new TestTarget());

			bool executed = false;
			var result = target.GetOrAddInitialiser(typeof(string), t => {
				executed = true;
				return new TestCompiledTarget();
			});

			Assert.True(executed);
		}

		[Fact]
		public void ShouldOnlyExecuteInitialiseFactoryOnce()
		{
			var target = new SingletonTarget(new TestTarget());

			int execCount = 0;
			Func<Type, ICompiledTarget> factory = t =>
			{
				++execCount;
				return new TestCompiledTarget();
			};
			var result1 = target.GetOrAddInitialiser(typeof(string), factory);
			var result2 = target.GetOrAddInitialiser(typeof(string), factory);

			Assert.Equal(1, execCount);
			Assert.Same(result1, result2);
		}
	}
}
