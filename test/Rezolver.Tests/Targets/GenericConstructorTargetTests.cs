using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
	public class GenericConstructorTargetTests : TargetTestsBase
	{
		public GenericConstructorTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullType()
		{
			Assert.Throws<ArgumentNullException>(() => new GenericConstructorTarget(null));
		}

		[Fact]
		public void ShouldNotAllowNonGenericType()
		{
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(string)));
		}

		[Fact]
		public void ShouldNotAllowGenericInterfaceOrAbstractClass()
		{
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(IEqualityComparer<>)));
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(AbstractGeneric<>)));
		}

		[Fact]
		public void DeclaredTypeShouldBeEqualToGenericType()
		{
			Assert.Same(typeof(EqualityComparer<>), new GenericConstructorTarget(typeof(EqualityComparer<>)).DeclaredType);
		}

		//class has custom SupportsType implementation theories
#error carry on here.
	}
}
