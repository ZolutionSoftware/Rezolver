using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Rezolver.Targets;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Rezolver.Tests.Targets
{
    public class OptionalParameterTargetTests : TargetTestsBase
    {
		public OptionalParameterTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		//this target is pretty basic - all it does is wrap the parameter's default value

		internal void TestMethod_NoOptional(string msg)
		{
		}

		static MethodInfo TestMethod_NoOptional_Info
			= typeof(OptionalParameterTargetTests).GetMethod(nameof(TestMethod_NoOptional), BindingFlags.NonPublic | BindingFlags.Instance);

		internal void TestMethod_OptionalStringWithDefault(string msg = "hello world")
		{
		}

		static MethodInfo TestMethod_OptionalStringWithDefault_Info 
			= typeof(OptionalParameterTargetTests).GetMethod(nameof(TestMethod_OptionalStringWithDefault), BindingFlags.NonPublic | BindingFlags.Instance);

		internal void TestMethod_OptionalIntWithNoDefault([Optional]int value)
		{

		}

		static MethodInfo TestMethod_OptionalIntWithNoDefault_Info
			 = typeof(OptionalParameterTargetTests).GetMethod(nameof(TestMethod_OptionalIntWithNoDefault), BindingFlags.NonPublic | BindingFlags.Instance);

		[Fact]
		public void ShouldNotAllowNullParameter()
		{
			Assert.Throws<ArgumentNullException>(() => new OptionalParameterTarget(null));
		}

		[Fact]
		public void ShouldNotAllowRequiredParameter()
		{
			Assert.Throws<ArgumentException>(() => new OptionalParameterTarget(TestMethod_NoOptional_Info.GetParameters()[0]));
		}

		[Fact]
		public void ShouldSetMethodParameterAndDeclaredType()
		{
			var target = new OptionalParameterTarget(TestMethod_OptionalStringWithDefault_Info.GetParameters()[0]);
			Assert.NotNull(target.MethodParameter);
			Assert.Equal(typeof(string), target.DeclaredType);
		}

		[Fact]
		public void ShouldCorrectlyUseStringArgumentWithDefault()
		{
			var target = new OptionalParameterTarget(TestMethod_OptionalStringWithDefault_Info.GetParameters()[0]);
			Assert.Equal("hello world", target.Value);
		}

		[Fact]
		public void ShouldCorrectlyUseDefaultIntForOptionalArgWithNoDefault()
		{
			var target = new OptionalParameterTarget(TestMethod_OptionalIntWithNoDefault_Info.GetParameters()[0]);
			Assert.Equal(0, target.Value);
		}
	}
}
