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
		public void ShouldSetExpressionOrFactory()
		{
			var expr = Expression.Constant("hello world");
			Assert.Same(expr, new ExpressionTarget(expr).Expression);
			Func<ICompileContext, Expression> factory = c => expr;
			var factoryTarget = new ExpressionTarget(factory, typeof(string));
			//we test DeclaredType for straight Expressions next
			Assert.Same(factory, factoryTarget.ExpressionFactory);
			Assert.Equal(typeof(string), factoryTarget.DeclaredType);
		}

		[Fact]
		public void DeclaredTypeShouldBeInheritedFromConstantExpression()
		{
			//use two expressions: a lambda and a standard expression
			var expr = Expression.Constant("hello world");
			
			var result1 = new ExpressionTarget(expr);

			Assert.Same(typeof(string), result1.DeclaredType);
		}

		[Fact]
		public void DeclaredTypeShouldBeInheritedFromLambdaExpression()
		{
			Expression<Func<ResolveContext, string>> expr = c => "hello world";

			var result2 = new ExpressionTarget(expr);

			Assert.Same(typeof(string), result2.DeclaredType);
		}
	}
}
