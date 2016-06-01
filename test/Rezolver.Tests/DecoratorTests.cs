using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class DecoratorTests : TestsBase
	{
		[Fact]
		public void ShouldDecorateDecoratedType()
		{
			RezolverBuilder builder = new RezolverBuilder();
			builder.RegisterType<DecoratedType, IDecorated>();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
		}
	}
}
