using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
	public class DelegateTargetTests : TargetTestsBase
	{
		public DelegateTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullFactory()
		{
			Assert.Throws<ArgumentNullException>(() => new DelegateTarget(null));
		}

		[Fact]
		public void ShouldNotAllowVoidDelegate()
		{
			Action invalidFactory = () => Console.WriteLine();
			Assert.Throws<ArgumentException>(() => new DelegateTarget(invalidFactory));
		}

		[Fact]
		public void NonNullDeclaredTypeMustBeCompatibleWithDelegateReturnType()
		{
			Func<string> factory = () => "hello world";
			Assert.Throws<ArgumentException>(() => new DelegateTarget(factory, typeof(int)));
		}

		delegate string HasRefParam(ref object o);
		delegate string HasOutParam(out object o);

		[Fact]
		public void ShouldNotAllowDelegateWithRefOrOutParameters()
		{
			var invalidRefFactory = new HasRefParam((ref object o) => "hello world");
			var invalidOutFactory = new HasOutParam((out object o) =>
			{
				o = null;
				return "hello world";
			});

			Assert.Throws<ArgumentException>(() => new DelegateTarget(invalidRefFactory));
			Assert.Throws<ArgumentException>(() => new DelegateTarget(invalidOutFactory));
		}

		[Fact]
		public void DeclaredTypeShouldReturnTypeOfDelegate()
		{
			Func<string> factory = () => "hello world";
			var result = new DelegateTarget(factory);

			Assert.Equal(typeof(string), result.DeclaredType);
		}

		[Fact]
		public void ShouldAllowDifferentButCompatibleDeclaredType()
		{
			Func<EqualityComparer<string>> factory = () => EqualityComparer<string>.Default;
			var result = new DelegateTarget(factory, typeof(IEqualityComparer<string>));

			Assert.Equal(typeof(IEqualityComparer<string>), result.DeclaredType);
		}
    }
}
