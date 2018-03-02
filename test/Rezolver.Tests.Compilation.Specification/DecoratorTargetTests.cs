using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using SetupTargets = System.Action<Rezolver.IRootTargetContainer>;

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

        public static DecoratorTheoryData RedecoratedRegistrations =>
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

        public static DecoratorTheoryData Generic_RedecoratedRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator(typeof(DecoratorForAny2<>), typeof(IDecorated<>));
                }, "after" },
                { t => {
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator(typeof(DecoratorForAny2<>), typeof(IDecorated<>));
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                }, "before" },
            };

        public static DecoratorTheoryData Generic_EnumerableRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterType(typeof(DecoratedForAny2<>), typeof(IDecorated<>));
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                }, "after" },
                { t => {
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterType(typeof(DecoratedForAny2<>), typeof(IDecorated<>));
                }, "before" },
                { t => {
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterType(typeof(DecoratedForAny2<>), typeof(IDecorated<>));
                }, "mid" }
            };

        public static DecoratorTheoryData Generic_WithSpecialisedGenericTypesRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterType<DecoratedForString, IDecorated<string>>();
                    t.RegisterType<DecoratedForDouble, IDecorated<double>>();
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                }, "after" },
                { t => {
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterType<DecoratedForString, IDecorated<string>>();
                    t.RegisterType<DecoratedForDouble, IDecorated<double>>();
                }, "before" }
            };

        public static DecoratorTheoryData Generic_OnlyOneClosedGenericRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator<DecoratorForAny<string>, IDecorated<string>>();
                }, "after" },
                { t => {
                    t.RegisterDecorator<DecoratorForAny<string>, IDecorated<string>>();
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                }, "before" }
            };

        public static DecoratorTheoryData Generic_RedecorateOnlyOneClosedGenericRegistrations =>
            new DecoratorTheoryData()
            {
                { t => {
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator<DecoratorForAny2<string>, IDecorated<string>>();
                }, "after" },
                { t => {
                    t.RegisterDecorator(typeof(DecoratorForAny<>), typeof(IDecorated<>));
                    t.RegisterDecorator<DecoratorForAny2<string>, IDecorated<string>>();
                    t.RegisterType(typeof(DecoratedForAny<>), typeof(IDecorated<>));
                }, "before" }
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
        [MemberData(nameof(RedecoratedRegistrations))]
        public void Decorator_Redecorated(SetupTargets setup, string description)
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
        public void Decorator_Enumerable(SetupTargets setup, string description)
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

        [Theory]
        [MemberData(nameof(Generic_RedecoratedRegistrations))]
        public void Decorator_Generic_Redecorated(SetupTargets setup, string description)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result1 = Assert.IsType<DecoratorForAny2<decimal>>(container.Resolve<IDecorated<decimal>>());
            var inner = Assert.IsType<DecoratorForAny<decimal>>(result1.Decorated);
            Assert.IsType<DecoratedForAny<decimal>>(inner.Decorated);
        }

        [Theory]
        [MemberData(nameof(Generic_EnumerableRegistrations))]
        public void Decorator_Generic_Enumerable(SetupTargets setup, string description)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result = container.Resolve<IEnumerable<IDecorated<Disposable2>>>().ToArray();

            Assert.Equal(2, result.Length);
            Assert.IsType<DecoratedForAny<Disposable2>>(Assert.IsType<DecoratorForAny<Disposable2>>(result[0]).Decorated);
            Assert.IsType<DecoratedForAny2<Disposable2>>(Assert.IsType<DecoratorForAny<Disposable2>>(result[1]).Decorated);
        }

        [Theory]
        [MemberData(nameof(Generic_WithSpecialisedGenericTypesRegistrations))]
        public void Decorator_Generic_WithSpecialisedGenericTypes(SetupTargets setup, string description)
        {
            //testing that the decorator kicks in for all IDecorated<> regardless of whether the underlying
            //registrations are open generics or specific closed generics
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var result1 = Assert.IsType<DecoratorForAny<Disposable>>(container.Resolve<IDecorated<Disposable>>());
            var result2 = Assert.IsType<DecoratorForAny<string>>(container.Resolve<IDecorated<string>>());
            var result3 = Assert.IsType<DecoratorForAny<double>>(container.Resolve<IDecorated<double>>());

            Assert.IsType<DecoratedForAny<Disposable>>(result1.Decorated);
            Assert.IsType<DecoratedForString>(result2.Decorated);
            Assert.IsType<DecoratedForDouble>(result3.Decorated);
        }

        [Theory]
        [MemberData(nameof(Generic_OnlyOneClosedGenericRegistrations))]
        public void Decorator_Generic_OnlyOneClosedGeneric(SetupTargets setup, string description)
        {
            // decorator only kicks in for one closed generic type
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var undecorated = Assert.IsType<DecoratedForAny<double>>(container.Resolve<IDecorated<double>>());
            var decorator = Assert.IsType<DecoratorForAny<string>>(container.Resolve<IDecorated<string>>());
            Assert.IsType<DecoratedForAny<string>>(decorator.Decorated);
        }

        [Theory]
        [MemberData(nameof(Generic_RedecorateOnlyOneClosedGenericRegistrations))]
        public void Decorator_Generic_RedecorateOnlyOneClosedGeneric(SetupTargets setup, string description)
        {
            var targets = CreateTargetContainer();
            setup(targets);
            var container = CreateContainer(targets);

            var onceDecorated = Assert.IsType<DecoratorForAny<double>>(container.Resolve<IDecorated<double>>());
            Assert.IsType<DecoratedForAny<double>>(onceDecorated.Decorated);

            var twiceDecorated = Assert.IsType<DecoratorForAny2<string>>(container.Resolve<IDecorated<string>>());
            var innerDecorator = Assert.IsType<DecoratorForAny<string>>(twiceDecorated.Decorated);
            Assert.IsType<DecoratedForAny<string>>(innerDecorator.Decorated);
        }
      
        

        #region SHOULD THE DECORATOR SUPPORT THIS WITH CHILD CONTAINERS/CHILD TARGET CONTAINERS?

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

        #endregion
    }
}
