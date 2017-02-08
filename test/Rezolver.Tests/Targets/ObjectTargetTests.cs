using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Rezolver.Targets;
using Rezolver.Tests.Types;

namespace Rezolver.Tests.Targets
{
    public class ObjectTargetTests : TargetTestsBase
    {
		public ObjectTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullObjectForValueType()
		{
			Assert.Throws<ArgumentException>(() => new ObjectTarget(null, declaredType: typeof(int)));
		}

		[Fact]
		public void ObjectMustBeCompatibleWithExplicitDeclaredType()
		{
			Assert.Throws<ArgumentException>(() => new ObjectTarget("hello world", typeof(int)));
		}

		public static IEnumerable<object[]> ValueData()
		{
			return new object[][]
			{
				new object[] { (int)1 },
				new [] { "Hello world" },
				new [] { new NoCtor() }
			};
		}

		[Theory]
		[MemberData(nameof(ValueData))]
		public void ShouldSetTargetTypeAndValue(object value)
		{
			var target = new ObjectTarget(value);
			Assert.Equal(value.GetType(), target.DeclaredType);
			Assert.Same(value, target.Value);
		}

		[Theory]
		[MemberData(nameof(ValueData))]
		public void CompiledTargetImplementationShouldReturnValue(object value)
		{
			ICompiledTarget target = new ObjectTarget(value);
			Assert.Same(value, target.GetObject(new ResolveContext((IContainer)null, value.GetType())));
			//also check that it works for a base (going for typeof(object))
			Assert.Same(value, target.GetObject(new ResolveContext((IContainer)null, typeof(object))));
		}
	}
}
