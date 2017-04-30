using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
		//testing ListTarget directly and via the EnumerableTargetContainer, which uses it

		[Fact]
		public void ListTarget_ListOfStrings()
		{
			var targets = CreateTargetContainer();
			//enumerable of object targets
			var expectedStrings = Enumerable.Range(0, 3).Select(i => $"Item{i}");
			var target = new ListTarget(typeof(string), expectedStrings.Select(s => Target.ForObject(s)));
			targets.Register(target);
			var container = CreateContainer(targets);

			var result = container.Resolve<List<string>>();

			Assert.NotNull(result);
			Assert.Equal(expectedStrings, result);
		}

		[Fact]
		public void ListTarget_ArrayOfStrings()
		{
			var targets = CreateTargetContainer();
			var expectedStrings = Enumerable.Range(0, 3).Select(i => $"Item{i}");
			var target = new ListTarget(typeof(string), expectedStrings.Select(s => Target.ForObject(s)), asArray: true);
			targets.Register(target);
			var container = CreateContainer(targets);

			var result = container.Resolve<string[]>();

			Assert.NotNull(result);
			Assert.Equal(expectedStrings, result);
		}

		[Fact]
		public void ListTarget_ListOfStrings_AsEnumerable()
		{
			var targets = CreateTargetContainer();
			//enumerable of object targets
			var expectedStrings = Enumerable.Range(0, 3).Select(i => $"Item{i}");
			var target = new ListTarget(typeof(string), expectedStrings.Select(s => Target.ForObject(s)));
			targets.Register(target, typeof(IEnumerable<string>));
			var container = CreateContainer(targets);

			var result = container.Resolve<IEnumerable<string>>();

			Assert.NotNull(result);
			Assert.Equal(expectedStrings, result);
		}

		[Fact]
		public void ListTarget_ArrayOfStrings_AsEnumerable()
		{
			var targets = CreateTargetContainer();
			//enumerable of object targets
			var expectedStrings = Enumerable.Range(0, 3).Select(i => $"Item{i}");
			var target = new ListTarget(typeof(string), expectedStrings.Select(s => Target.ForObject(s)), asArray: true);
			targets.Register(target, typeof(IEnumerable<string>));
			var container = CreateContainer(targets);

			var result = container.Resolve<IEnumerable<string>>();

			Assert.NotNull(result);
			Assert.Equal(expectedStrings, result);
		}

		[Fact]
		public void ListTarget_ImplicitEnumerable_Unregistered()
		{
			var targets = CreateTargetContainer();

			var container = CreateContainer(targets);

			var result = container.Resolve<IEnumerable<string>>();

			Assert.Equal(Enumerable.Empty<string>(), result);
		}

		[Fact]
		public void ListTarget_ImplicitEnumerable_SingleTarget()
		{
			var targets = CreateTargetContainer();
			targets.RegisterObject("first");
			var container = CreateContainer(targets);

			var result = container.Resolve<IEnumerable<string>>();

			Assert.Equal(new[] { "first" }, result);
		}

		[Fact]
		public void ListTarget_ImplicitEnumerable_MultipleTargets_OrderMustBePreserved()
		{
			var targets = CreateTargetContainer();
			targets.RegisterObject("first");
			targets.RegisterObject("second");
			targets.RegisterObject("third");
			var container = CreateContainer(targets);

			var result = container.Resolve<IEnumerable<string>>();

			Assert.Equal(new[] { "first", "second", "third" }, result);
		}
	}
}
