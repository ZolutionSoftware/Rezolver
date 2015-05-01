using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Rezolver.Tests.vNext
{
    public class RezolverBuilderTests_Multiple
    {
		class FakeTarget : IRezolveTarget
		{
			public Type DeclaredType
			{
				get
				{
					return typeof(RezolverBuilderTests_Multiple);
				}
			}

			public Expression CreateExpression(CompileContext context)
			{
				return null;
			}

			public bool SupportsType(Type type)
			{
				return true;
			}
		}

		[Fact]
        public void ShouldFetchDefault()
        {
			RezolverBuilder builder = new RezolverBuilder();
			var targets = Enumerable.Range(0, 3).Select(i => new FakeTarget()).ToArray();

			foreach(var target in targets)
			{
				builder.Register(target, typeof(RezolverBuilderTests_Multiple));
			}

			var entry = builder.Fetch(typeof(RezolverBuilderTests_Multiple), null);

			Assert.Same(targets[0], entry.DefaultTarget);
        }

		[Fact]
		public void ShouldFetchIEnumerableForSingleRegistration()
		{
			RezolverBuilder builder = new RezolverBuilder();
			var target = new FakeTarget();
			builder.Register(target, typeof(RezolverBuilderTests_Multiple));

			var entry = builder.Fetch(typeof(IEnumerable<RezolverBuilderTests_Multiple>), null);

			Assert.Same(target, entry.DefaultTarget);
		}
    }
}
