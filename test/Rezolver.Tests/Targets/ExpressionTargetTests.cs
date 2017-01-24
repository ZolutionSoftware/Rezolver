using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Rezolver.Compilation;
using System.Linq.Expressions;

namespace Rezolver.Tests.Targets
{
    public class ExpressionTargetTests : TargetTestsBase
    {
		public ExpressionTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullExpression()
		{
			Assert.Throws<ArgumentNullException>(() => new ExpressionTarget(null));
		}

		[Fact]
		public void ShouldNotAllowNullFactory()
		{
			Assert.Throws<ArgumentNullException>(() => new ExpressionTarget((Func<ICompileContext, Expression>)null, typeof(string)));
		}

		[Fact]
		public void ShouldNotAllowNullTypeForFactory()
		{
			Assert.Throws<ArgumentNullException>(() => new ExpressionTarget((ICompileContext c) => Expression.Constant(null, typeof(string)), null));
		}

		[Fact]
		public void ShouldNotAllowIncompatibleDeclaredType()
		{
			Assert.Throws<ArgumentException>(() => new ExpressionTarget(Expression.Constant("hello world"), typeof(int)));
		}

		[Fact]
		public void DeclaredTypeShouldBeInheritedFromExpression()
		{
			//use two expressions: a lambda and a standard expression
			var expr1 = Expression.Constant("hello world");
			Expression<Func<ResolveContext, string>> expr2 = c => "hello world";

			var result1 = new ExpressionTarget(expr1);
			Assert.Same(expr1.Type, result1.DeclaredType);

			var result2 = new ExpressionTarget(expr2);
			Assert.Same(expr2.Type, result2.DeclaredType);
		}
	}
}
