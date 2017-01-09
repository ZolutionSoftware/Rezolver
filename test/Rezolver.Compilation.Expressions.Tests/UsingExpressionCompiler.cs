using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Compilation.Expressions.Tests
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
			var container = new Container();
			container.UseExpressionCompiler();
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

			var container = new Container();
			container.UseExpressionCompiler();
			//okay, so yes: repeating the assertion from above.
			var compiler = Assert.IsType<ExpressionCompiler>(container.Resolve<ITargetCompiler>());

			//in normal operation, the Container object impersonates its own target container, 
			//hence the doubling up of the parameter here.
			var builder = compiler.ResolveBuilder(target, new CompileContext(container, container, target.DeclaredType));
			Assert.NotNull(builder);
			Assert.IsAssignableFrom(typeof(IExpressionBuilder<>).MakeGenericType(target.GetType()), builder);
		}
    }
}
