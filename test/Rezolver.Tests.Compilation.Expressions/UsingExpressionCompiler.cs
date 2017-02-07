using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Expressions
{
    public class UsingExpressionCompiler
    {
		private readonly ITestOutputHelper _output;

		public UsingExpressionCompiler(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void ContainerShouldResolveExpressionCompiler()
		{
			var container = new Container(ExpressionCompiler.ConfigProvider);
			var compiler = Assert.IsType<ExpressionCompiler>(container.Resolve<ITargetCompiler>());
		}

		public static IEnumerable<object[]> GetTargetsToResolveExpressionBuilders()
		{
			return new object[][] {
				new[] { new ConstructorTarget(typeof(Types.SimpleType)) },
				new[] { new ObjectTarget("Hello world") },
				new[] { new ConstructorTarget(typeof(Types.SingletonType)).Singleton() },
				new[] { new ConstructorTarget(typeof(Types.ScopedType)).Scoped() }
			};
		}

		[Theory]
		[MemberData(nameof(GetTargetsToResolveExpressionBuilders))]
		public void CompilerShouldResolveExpressionBuilder(ITarget target)
		{
			_output.WriteLine($"Target type is { target.GetType() }");

			var container = new Container(ExpressionCompiler.ConfigProvider);
			//okay, so yes: repeating the assertion from above.
			var compiler = Assert.IsType<ExpressionCompiler>(container.Resolve<ITargetCompiler>());
			var contextProvider = Assert.IsType<ExpressionCompiler>(container.Resolve<ICompileContextProvider>());
			//in normal operation, the Container object impersonates its own target container, 
			//hence the doubling up of the parameter here.
			var builder = compiler.ResolveBuilder(target, Assert.IsType<ExpressionCompileContext>(contextProvider.CreateContext(new ResolveContext(container, target.DeclaredType), container)));
			Assert.NotNull(builder);
			Assert.IsAssignableFrom(typeof(IExpressionBuilder<>).MakeGenericType(target.GetType()), builder);
		}
    }
}
