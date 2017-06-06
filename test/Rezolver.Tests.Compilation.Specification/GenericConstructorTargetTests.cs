using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
		[Fact]
		public void GenericCtorTarget_NoCtor()
		{
			ITarget t = Target.ForType(typeof(Generic<>));
			var targets = CreateTargetContainer();
			targets.Register(t);
			var container = CreateContainer(targets);
			//try and build an instance 
			var instance = container.Resolve<Generic<int>>();
			Assert.NotNull(instance);
			Assert.Equal(default(int), instance.Value);
		}

		[Fact]
		public void GenericCtorTarget_OneCtor()
		{
			//typical pattern already 
			ITarget t = Target.ForType(typeof(GenericOneCtor<>));
			var targets = CreateTargetContainer();
			targets.Register(t);
			targets.RegisterObject(1);
			var container = CreateContainer(targets);

			var instance = container.Resolve<GenericOneCtor<int>>();
			Assert.NotNull(instance);
			Assert.Equal(1, instance.Value);
		}

		//now test that the target should work when used as the target of a dependency look up
		//just going to use the DefaultRezolver for this as it's far easier to setup the test.

		[Fact]
		public void GenericCtorTarget_ClosedGenericDependency()
		{
			var targets = CreateTargetContainer();

            targets.RegisterType(typeof(GenericOneCtor<>), typeof(IGeneric<>));
			targets.RegisterObject(2);
			targets.RegisterType(typeof(RequiresIGenericInt));
			var container = CreateContainer(targets);

			var result = container.Resolve<RequiresIGenericInt>();
			Assert.NotNull(result);
			Assert.NotNull(result.GenericValue);
			Assert.Equal(2, result.GenericValue.Value);
		}

		[Fact]
		public void GenericCtorTarget_NestedGenericDependency()
		{
			var targets = CreateTargetContainer();
			targets.Register(Target.ForType(typeof(GenericOneCtor<>)));
			targets.Register(Target.ForType(typeof(Generic<>)));
			var container = CreateContainer(targets);

			var result = container.Resolve<GenericOneCtor<Generic<int>>>();
			Assert.NotNull(result);
			Assert.NotNull(result.Value);
			//no int registered an the inner target type of Generic<> has no constructor through which we can set it anyway.
			Assert.Equal(0, result.Value.Value);
		}


		//this one is the open generic nested dependency check

		[Fact]
		public void GenericCtorTarget_NestedOpenGenericDependency()
		{
			var targets = CreateTargetContainer();
			targets.RegisterObject(10);
			targets.RegisterType(typeof(GenericOneCtor<>), typeof(IGeneric<>));
			targets.RegisterType(typeof(RequiresIGenericT<>));
			var container = CreateContainer(targets);

			var result = container.Resolve<RequiresIGenericT<int>>();
			Assert.NotNull(result);
			Assert.NotNull(result.Value);
			Assert.Equal(10, result.Value.Value);
		}

		[Fact]
		public void GenericCtorTarget_SpecialisedNestedGenericByInterface()
		{
			var targets = CreateTargetContainer();

			targets.RegisterObject(25);
			//register a standard handler for all IGeneric<T>
			targets.RegisterType(typeof(GenericOneCtor<>), typeof(IGeneric<>));
			//register a specific handler for IGeneric<IGeneric<T>>
			//only way to get hold of the type for this is to use MakeGenericType
			targets.RegisterType(typeof(GenericGenericOneCtor<>), typeof(IGeneric<>).MakeGenericType(typeof(IGeneric<>)));
			var container = CreateContainer(targets);

			var result = container.Resolve<IGeneric<IGeneric<int>>>();

			Assert.IsType<GenericGenericOneCtor<int>>(result);
			Assert.NotNull(result.Value);
			Assert.IsType<GenericOneCtor<int>>(result.Value);
			Assert.Equal(25, result.Value.Value);
		}

		[Fact]
		public void GenericCtorTarget_ShouldAllowRecursionForDifferentGenericTypes()
		{
			//this test verifies that generic targets can be compiled recursively so long
			//as the target types for those targets is different.

			var targets = CreateTargetContainer();

			targets.RegisterType(typeof(GenericWrapper<>));
			targets.RegisterType<NeedsWrappedRoot>();
			targets.RegisterType<Root>();

			var container = CreateContainer(targets);

			var result = container.Resolve<GenericWrapper<NeedsWrappedRoot>>();

			Assert.NotNull(result);
			Assert.NotNull(result.Wrapped);
			Assert.NotNull(result.Wrapped.WrappedRoot);
			Assert.NotNull(result.Wrapped.WrappedRoot.Wrapped);
		}

		//some of the more exotic scenarios (multiple type parameters, resolving by bases/interfaces
		//potentially with reversed/munged type parameter mappings etc) are ignored now that we've 
		//tested most single-parameter scenarios and everything has worked.
		//These exotica are effectively Bind(context) test cases, which are handled by the main library's
		//test suite.

        [Fact]
        public void GenericCtorTarget_ShouldMatchConstrainedGeneric()
        {
            var targets = CreateTargetContainer();

            targets.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
            targets.RegisterType(typeof(ConstrainedGeneric<>), typeof(IGeneric<>));

            var container = CreateContainer(targets);

            var result = Assert.IsType<ConstrainedGeneric<BaseClassChild>>(container.Resolve<IGeneric<BaseClassChild>>());
        }

        [Fact]
        public void GenericCtorTarget_ShouldFavourOpenGenericWhenConstraintsNotMatched()
        {
            var targets = CreateTargetContainer();

            targets.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
            targets.RegisterType(typeof(ConstrainedGeneric<>), typeof(IGeneric<>));

            var container = CreateContainer(targets);

            var result = Assert.IsType<Generic<string>>(container.Resolve<IGeneric<string>>());
        }
	}
}
