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
		public void DecoratorTarget_PostRegistered()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<Decorated, IDecorated>();
			targets.RegisterDecorator<Decorator, IDecorated>();
			var container = CreateContainer(targets);
			var result = container.Resolve<IDecorated>();
			Assert.IsType<Decorator>(result);
			//yes, we've checked the type - but we also need to check the 
			//argument is passed into the constructor.  Obviously, we could do this
			//by null-checking in the constructor, but this is more fun.
			Assert.Equal("Hello World", result.DoSomething());
		}

		[Fact]
		public void DecoratorTarget_PreRegistered()
		{
			var targets = CreateTargetContainer();
			targets.RegisterDecorator<Decorator, IDecorated>();
			targets.RegisterType<Decorated, IDecorated>();
			var container = CreateContainer(targets);
			var result = container.Resolve<IDecorated>();
			Assert.IsType<Decorator>(result);

			Assert.Equal("Hello World", result.DoSomething());
		}

		[Fact]
		public void DecoratorTarget_DecorateADecorator()
		{
			//see if stacking multiple decorators works.
			var targets = CreateTargetContainer();
			targets.RegisterType<Decorated, IDecorated>();
			targets.RegisterDecorator<Decorator, IDecorated>();
			targets.RegisterDecorator<OMGDecorator, IDecorated>();

			var container = CreateContainer(targets);
			var result = container.Resolve<IDecorated>();
			Assert.IsType<OMGDecorator>(result);

			Assert.Equal("OMG: Hello World", result.DoSomething());
		}

		[Fact]
		public void DecoratorTarget_Enumerable_PostRegistered()
		{
			//this test check that requesting an IEnumerable of T gives you
			//an enumerable of decorated T
			var targets = CreateTargetContainer();
			targets.RegisterType<Decorated, IDecorated>();
			targets.RegisterType<Decorated2, IDecorated>();
			targets.RegisterDecorator<Decorator, IDecorated>();
			var container = CreateContainer(targets);
			var result = container.Resolve<IEnumerable<IDecorated>>().ToArray();
			Assert.Equal(2, result.Length);
			Assert.Single(result, r => r.DoSomething() == "Hello World");
			Assert.Single(result, r => r.DoSomething() == "Goodbye World");
		}

		[Fact]
		public void DecoratorTarget_Enumerable_PreRegistered()
		{
			//this test is the same as above, except the decorator is registered first.
			var targets = CreateTargetContainer();
			targets.RegisterDecorator<Decorator, IDecorated>();
			targets.RegisterType<Decorated, IDecorated>();
			targets.RegisterType<Decorated2, IDecorated>();

			var container = CreateContainer(targets);
			var result = container.Resolve<IEnumerable<IDecorated>>().ToArray();
			Assert.Equal(2, result.Length);
			Assert.Single(result, r => r.DoSomething() == "Hello World");
			Assert.Single(result, r => r.DoSomething() == "Goodbye World");
		}

		[Fact]
		public void DecoratorTarget_Generic_PostRegistered()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<StringHandler, IHandler<string>>();
			targets.RegisterType<DoubleHandler, IHandler<double>>();
			targets.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			var container = CreateContainer(targets);
			var result = container.Resolve<IHandler<string>>();
			var result2 = container.Resolve<IHandler<double>>();
			Assert.IsType<GenericDecoratingHandler<string>>(result);
			Assert.IsType<GenericDecoratingHandler<double>>(result2);
		}

		[Fact]
		public void DecoratorTarget_Generic_PreRegistered()
		{
			//check that the decorator is registration order agnostic.
			var targets = CreateTargetContainer();
			targets.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			targets.RegisterType<StringHandler, IHandler<string>>();
			targets.RegisterType<DoubleHandler, IHandler<double>>();
			var container = CreateContainer(targets);
			var result = container.Resolve<IHandler<string>>();
			var result2 = container.Resolve<IHandler<double>>();
			Assert.IsType<GenericDecoratingHandler<string>>(result);
			Assert.IsType<GenericDecoratingHandler<double>>(result2);
		}

		[Fact]
		public void DecoratorTarget_Generic_DecorateADecorator()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<StringHandler, IHandler<string>>();
			targets.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			targets.RegisterDecorator(typeof(GenericDecoratingHandler2<>), typeof(IHandler<>));
			var container = CreateContainer(targets);
			var result = container.Resolve<IHandler<string>>();
			Assert.IsType<GenericDecoratingHandler2<string>>(result);
			Assert.Equal("((This is a string: Hello World) Decorated) Decorated again :)", result.Handle("Hello World"));
		}

		[Fact]
		public void DecoratorTarget_Generic_SpecificClosedGeneric()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<StringHandler, IHandler<string>>();
			targets.RegisterType<DoubleHandler, IHandler<double>>();
			targets.RegisterDecorator<GenericDecoratingHandler<string>, IHandler<string>>();
			var container = CreateContainer(targets);
			var result1 = container.Resolve<IHandler<string>>();
			var result2 = container.Resolve<IHandler<double>>();

			Assert.IsType<DoubleHandler>(result2);
			Assert.IsType<GenericDecoratingHandler<string>>(result1);
		}

		[Fact]
		public void DecoratorTarget_Generic_DecorateADecorator_SpecificClosedGeneric_PostRegistered()
		{
			//in this test we register an open generic decorator
			//and then register another decorator specialised for string.
			//when we get a handler for the double type, we should get only one decorator
			//when we get a handler for the string type, we should get the two decorators - the open generic
			//decorator wrapping the specialised decorator, wrapping the string handler.
			var targets = CreateTargetContainer();
			targets.RegisterType<StringHandler, IHandler<string>>();
			targets.RegisterType<DoubleHandler, IHandler<double>>();
			targets.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			targets.RegisterDecorator(typeof(GenericDecoratingHandler2<string>), typeof(IHandler<string>));
            
            var container = CreateContainer(targets);
			var result = container.Resolve<IHandler<double>>();
			var result2 = container.Resolve<IHandler<string>>();
			Assert.IsType<GenericDecoratingHandler<double>>(result);
			Assert.IsType<GenericDecoratingHandler<string>>(result2);
			var handled = result2.Handle("Hello World");
            //see BUG #27: https://github.com/ZolutionSoftware/Rezolver/issues/27
            Assert.Equal("((This is a string: Hello World) Decorated) Decorated again :)", handled);
		}

		[Fact]
		public void DecoratorTarget_Generic_DecorateADecorator_SpecificClosedGeneric_PreRegistered()
		{
			//as above, but checking that it works when the decorators are applied first
			var targets = CreateTargetContainer();
			targets.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			targets.RegisterDecorator(typeof(GenericDecoratingHandler2<string>), typeof(IHandler<string>));
			targets.RegisterType<StringHandler, IHandler<string>>();
			targets.RegisterType<DoubleHandler, IHandler<double>>();
            
            var container = CreateContainer(targets);
			var result = container.Resolve<IHandler<double>>();
			var result2 = container.Resolve<IHandler<string>>();
			Assert.IsType<GenericDecoratingHandler<double>>(result);
			Assert.IsType<GenericDecoratingHandler<string>>(result2);
			var handled = result2.Handle("Hello World");
            //see BUG #27: https://github.com/ZolutionSoftware/Rezolver/issues/27
            Assert.Equal("((This is a string: Hello World) Decorated) Decorated again :)", handled);
		}

		[Fact]
		public void DecoratorTarget_Generic_DecoratingImplementationInsteadOfInterface()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType(typeof(GenericHandler<>), typeof(IHandler<>));
			targets.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			var container = CreateContainer(targets);
			Assert.IsType<GenericDecoratingHandler<string>>(container.Resolve<IHandler<string>>());
		}

		#region SHOULD THE DECORATOR SUPPORT THIS WITH CHILD CONTAINERS/CHILD TARGET CONTAINERS?

		//[Fact]
		public void ChildContainerShouldDecorateParent()
		{
			//pretty sure this will fail - and there's a question as to whether it should
			var containertargets = CreateTargetContainer();
			var container = CreateContainer(containertargets);
			var childContainertargets = CreateTargetContainer();
			var childContainer = new OverridingContainer(container, childContainertargets);

			containertargets.RegisterType(typeof(GenericHandler<>), typeof(IHandler<>));
			//the test only passes if we also register the above target in the child container
			//childContainertargets.RegisterType(typeof(GenericHandler<>), typeof(IHandler<>));
			childContainertargets.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));

			var result = childContainer.Resolve<IHandler<string>>();
			Assert.IsType<GenericDecoratingHandler<string>>(result);
		}

		//[Fact]
		public void ChildTargetContainerShouldDecorateParent()
		{
			//let's try it this way instead.  I somehow think this will not work either.
			var targetContainer = CreateTargetContainer();
			var childTargetContainer = new ChildTargetContainer(targetContainer);
			var container = CreateContainer(childTargetContainer);
			targetContainer.RegisterType(typeof(GenericHandler<>), typeof(IHandler<>));
			childTargetContainer.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));

			var result = container.Resolve<IHandler<string>>();
			Assert.IsType<GenericDecoratingHandler<string>>(result);
		}

		#endregion
	}
}
