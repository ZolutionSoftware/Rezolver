using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using SetupTargets = System.Action<Rezolver.ITargetContainer>;

namespace Rezolver.Tests.Compilation.Specification
{
    using DecoratorTheoryData = TheoryData<SetupTargets, string>;
    public partial class CompilerTestsBase
    {
        public static DecoratorTheoryData SimpleRegistrations =>
            new DecoratorTheoryData()
            {
                {
                    t => {
                    t.RegisterType<Decorated, IDecorated>();
                    t.RegisterDecorator<Decorator, IDecorated>();
                }, "after" },
                {
                    t => {
                    t.RegisterDecorator<Decorator, IDecorated>();
                    t.RegisterType<Decorated, IDecorated>();
                }, "before" }
            };

        public static DecoratorTheoryData DecoratedDecoratorRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType<Decorated, IDecorated>();
                    t.RegisterDecorator<Decorator, IDecorated>();
                    t.RegisterDecorator<Decorator2, IDecorated>();
                }, "after" },
                { t => {
                    t.RegisterDecorator<Decorator, IDecorated>();
                    t.RegisterDecorator<Decorator2, IDecorated>();
                    t.RegisterType<Decorated, IDecorated>();
                }, "before" },
                { t => {
                    t.RegisterDecorator<Decorator, IDecorated>();
                    t.RegisterType<Decorated, IDecorated>();
                    t.RegisterDecorator<Decorator2, IDecorated>();
                }, "beforeAndAfter" }
            };

        public static DecoratorTheoryData EnumerableRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType<Decorated, IDecorated>();
                    t.RegisterType<Decorated2, IDecorated>();
                    t.RegisterDecorator<Decorator, IDecorated>();
                }, "after" },
                { t => {
                    t.RegisterType<Decorated, IDecorated>();
                    t.RegisterType<Decorated2, IDecorated>();
                    t.RegisterDecorator<Decorator, IDecorated>();
                }, "before" },
                { t => {
                    t.RegisterType<Decorated, IDecorated>();
                    t.RegisterDecorator<Decorator, IDecorated>();
                    t.RegisterType<Decorated2, IDecorated>();
                }, "mid" }
            };

        public static DecoratorTheoryData Generic_SimpleRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                }, "after" },
                { t => {
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                }, "before" },
            };

        [Theory]
        [MemberData(nameof(SimpleRegistrations))]
        public void Decorator_Simple(SetupTargets setup, string description)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result = Assert.IsType<Decorator>(container.Resolve<IDecorated>());
            Assert.IsType<Decorated>(result.Decorated);
        }

        [Theory]
        [MemberData(nameof(DecoratedDecoratorRegistrations))]
        public void Decorator_DecoratedDecorator(SetupTargets setup, string description)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result = Assert.IsType<Decorator2>(container.Resolve<IDecorated>());
            var inner = Assert.IsType<Decorator>(result.Decorated);
            Assert.IsType<Decorated>(inner.Decorated);
        }


        [Theory]
        [MemberData(nameof(EnumerableRegistrations))]
        public void VerifyEnumerable(SetupTargets setup, string description)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result = container.Resolve<IEnumerable<IDecorated>>().ToArray();
            Assert.Equal(2, result.Length);
            Assert.IsType<Decorated>(Assert.IsType<Decorator>(result[0]).Decorated);
            Assert.IsType<Decorated2>(Assert.IsType<Decorator>(result[1]).Decorated);
        }

        [Theory]
        [MemberData(nameof(Generic_SimpleRegistrations))]
        public void Decorator_Generic_Simple(SetupTargets setup, string description)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result1 = Assert.IsType<DecoratorForAny<string>>(container.Resolve<IDecorated<string>>());
            Assert.IsType<DecoratedForAny<string>>(result1.Decorated);

            //verify it works for other types too
            var result2 = Assert.IsType<DecoratorForAny<Disposable>>(container.Resolve<IDecorated<Disposable>>());
            Assert.IsType<DecoratedForAny<Disposable>>(result2.Decorated);
        }

        //[Fact]
        //public void DecoratorTarget_Generic_PostRegistered()
        //{
        //    var targets = CreateTargetContainer();
        //    targets.RegisterType<DecoratedForString, IDecorated<string>>();
        //    targets.RegisterType<DecoratedForDouble, IDecorated<double>>();
        //    targets.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
        //    var container = CreateContainer(targets);
        //    var result = container.Resolve<IDecorated<string>>();
        //    var result2 = container.Resolve<IDecorated<double>>();
        //    Assert.IsType<DecoratorForAny<string>>(result);
        //    Assert.IsType<DecoratorForAny<double>>(result2);
        //}

        //[Fact]
        //public void DecoratorTarget_Generic_PreRegistered()
        //{
        //    //check that the decorator is registration order agnostic.
        //    var targets = CreateTargetContainer();
        //    targets.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
        //    targets.RegisterType<DecoratedForString, IDecorated<string>>();
        //    targets.RegisterType<DecoratedForDouble, IDecorated<double>>();
        //    var container = CreateContainer(targets);
        //    var result = container.Resolve<IDecorated<string>>();
        //    var result2 = container.Resolve<IDecorated<double>>();
        //    Assert.IsType<DecoratorForAny<string>>(result);
        //    Assert.IsType<DecoratorForAny<double>>(result2);
        //}

        //[Fact]
        //public void DecoratorTarget_Generic_DecorateADecorator()
        //{
        //    var targets = CreateTargetContainer();
        //    targets.RegisterType<DecoratedForString, IDecorated<string>>();
        //    targets.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
        //    targets.RegisterDecorator(typeof(GenericDecoratingHandler2<>), typeof(IDecorated<>));
        //    var container = CreateContainer(targets);
        //    var result = container.Resolve<IDecorated<string>>();
        //    Assert.IsType<GenericDecoratingHandler2<string>>(result);
        //    Assert.Equal("((This is a string: Hello World) Decorated) Decorated again :)", result.Handle("Hello World"));
        //}

        //[Fact]
        //public void DecoratorTarget_Generic_SpecificClosedGeneric()
        //{
        //    var targets = CreateTargetContainer();
        //    targets.RegisterType<DecoratedForString, IDecorated<string>>();
        //    targets.RegisterType<DecoratedForDouble, IDecorated<double>>();
        //    targets.RegisterDecorator<DecoratorForAny<string>, IDecorated<string>>();
        //    var container = CreateContainer(targets);
        //    var result1 = container.Resolve<IDecorated<string>>();
        //    var result2 = container.Resolve<IDecorated<double>>();

        //    Assert.IsType<DecoratedForDouble>(result2);
        //    Assert.IsType<DecoratorForAny<string>>(result1);
        //}

        //[Fact]
        //public void DecoratorTarget_Generic_DecorateADecorator_SpecificClosedGeneric_PostRegistered()
        //{
        //    //in this test we register an open generic decorator
        //    //and then register another decorator specialised for string.
        //    //when we get a handler for the double type, we should get only one decorator
        //    //when we get a handler for the string type, we should get the two decorators - the open generic
        //    //decorator wrapping the specialised decorator, wrapping the string handler.
        //    var targets = CreateTargetContainer();
        //    targets.RegisterType<DecoratedForString, IDecorated<string>>();
        //    targets.RegisterType<DecoratedForDouble, IDecorated<double>>();
        //    targets.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
        //    targets.RegisterDecorator(typeof(GenericDecoratingHandler2<string>), typeof(IDecorated<string>));

        //    var container = CreateContainer(targets);
        //    var result = container.Resolve<IDecorated<double>>();
        //    var result2 = container.Resolve<IDecorated<string>>();
        //    Assert.IsType<DecoratorForAny<double>>(result);
        //    Assert.IsType<DecoratorForAny<string>>(result2);
        //    var handled = result2.Handle("Hello World");
        //    //see BUG #27: https://github.com/ZolutionSoftware/Rezolver/issues/27
        //    Assert.Equal("((This is a string: Hello World) Decorated) Decorated again :)", handled);
        //}

        //[Fact]
        //public void DecoratorTarget_Generic_DecorateADecorator_SpecificClosedGeneric_PreRegistered()
        //{
        //    //as above, but checking that it works when the decorators are applied first
        //    var targets = CreateTargetContainer();
        //    targets.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
        //    targets.RegisterDecorator(typeof(GenericDecoratingHandler2<string>), typeof(IDecorated<string>));
        //    targets.RegisterType<DecoratedForString, IDecorated<string>>();
        //    targets.RegisterType<DecoratedForDouble, IDecorated<double>>();

        //    var container = CreateContainer(targets);
        //    var result = container.Resolve<IDecorated<double>>();
        //    var result2 = container.Resolve<IDecorated<string>>();
        //    Assert.IsType<DecoratorForAny<double>>(result);
        //    Assert.IsType<DecoratorForAny<string>>(result2);
        //    var handled = result2.Handle("Hello World");
        //    //see BUG #27: https://github.com/ZolutionSoftware/Rezolver/issues/27
        //    Assert.Equal("((This is a string: Hello World) Decorated) Decorated again :)", handled);
        //}

        //[Fact]
        //public void DecoratorTarget_Generic_DecoratingImplementationInsteadOfInterface()
        //{
        //    var targets = CreateTargetContainer();
        //    targets.RegisterType(typeof(GenericHandler<>), typeof(IDecorated<>));
        //    targets.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
        //    var container = CreateContainer(targets);
        //    Assert.IsType<DecoratorForAny<string>>(container.Resolve<IDecorated<string>>());
        //}

        //#region SHOULD THE DECORATOR SUPPORT THIS WITH CHILD CONTAINERS/CHILD TARGET CONTAINERS?

        ////[Fact]
        //public void ChildContainerShouldDecorateParent()
        //{
        //    //pretty sure this will fail - and there's a question as to whether it should
        //    var containertargets = CreateTargetContainer();
        //    var container = CreateContainer(containertargets);
        //    var childContainertargets = CreateTargetContainer();
        //    var childContainer = new OverridingContainer(container, childContainertargets);

        //    containertargets.RegisterType(typeof(GenericHandler<>), typeof(IDecorated<>));
        //    //the test only passes if we also register the above target in the child container
        //    //childContainertargets.RegisterType(typeof(GenericHandler<>), typeof(IHandler<>));
        //    childContainertargets.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));

        //    var result = childContainer.Resolve<IDecorated<string>>();
        //    Assert.IsType<DecoratorForAny<string>>(result);
        //}

        ////[Fact]
        //public void ChildTargetContainerShouldDecorateParent()
        //{
        //    //let's try it this way instead.  I somehow think this will not work either.
        //    var targetContainer = CreateTargetContainer();
        //    var childTargetContainer = new ChildTargetContainer(targetContainer);
        //    var container = CreateContainer(childTargetContainer);
        //    targetContainer.RegisterType(typeof(GenericHandler<>), typeof(IDecorated<>));
        //    childTargetContainer.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));

        //    var result = container.Resolve<IDecorated<string>>();
        //    Assert.IsType<DecoratorForAny<string>>(result);
        //}

        //#endregion
    }
}
