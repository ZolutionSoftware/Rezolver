using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Rezolver.TypeExtensions;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        [Fact]
        public void Members_ShouldBindAll()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(10);             //single int property
            targets.RegisterObject("hello world");  //single string property
            targets.RegisterObject(30m);            //List decimal property
            targets.RegisterObject(40m);
            DateTime now = DateTime.UtcNow;         //Bind to collection of datetimes
            targets.RegisterObject(now);
            targets.RegisterObject(now.AddDays(1));
            targets.RegisterType<HasMembers>(MemberBindingBehaviour.BindAll);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasMembers>();

            // Assert
            Assert.Equal(10, result.BindableInt);
            Assert.Equal("hello world", result.BindableString);
            // never bound because it's read only enumerable
            Assert.Equal(new[] { 1.0, 2.0, 3.0 }, result.ReadOnlyDoubles);
            Assert.Equal(new[] { 30m, 40m }, result.BindableListOfDecimals);
            Assert.Equal(new[] { now, now.AddDays(1) }, result.CollectionBindableListOfDateTimes);
        }

        [Fact]
        public void Members_ExplicitBinding_ShouldBindNoneByDefault()
        {
            // Arrange
            var targets = CreateTargetContainer();
            // note - by forcibly calling this overload, we'll trigger a BindSpecificMembersBehaviour to be
            // created with no member bindings
            targets.RegisterType((Action<IMemberBindingBehaviourBuilder<HasMembers>>)null);

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasMembers>();

            // Assert
            Assert.Equal(0, result.BindableInt);
            Assert.Null(result.BindableString);
            // never bound because it's read only enumerable
            Assert.Equal(new[] { 1.0, 2.0, 3.0 }, result.ReadOnlyDoubles);
            Assert.Empty(result.BindableListOfDecimals);
            Assert.Equal(new DateTime[0], result.CollectionBindableListOfDateTimes);
        }

        [Fact]
        public void Members_ShouldBindAll_OfBase()
        {
            // Same as Members_ShouldBindAll but this does it on a derived type

            //Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(10);             //single int property
            targets.RegisterObject("hello world");  //single string property
            targets.RegisterObject(30m);            //List decimal property
            targets.RegisterObject(40m);
            DateTime now = DateTime.UtcNow;         //Bind to collection of datetimes
            targets.RegisterObject(now);
            targets.RegisterObject(now.AddDays(1));
            targets.RegisterType<HasMembersChild>(MemberBindingBehaviour.BindAll);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasMembersChild>();

            // Assert
            Assert.Equal(10, result.BindableInt);
            Assert.Equal("hello world", result.BindableString);
            // never bound because it's read only enumerable
            Assert.Equal(new[] { 1.0, 2.0, 3.0 }, result.ReadOnlyDoubles);
            Assert.Equal(new[] { 30m, 40m }, result.BindableListOfDecimals);
            Assert.Equal(new[] { now, now.AddDays(1) }, result.CollectionBindableListOfDateTimes);
        }


        [Fact]
        public void Members_ShouldExplicitlyBindCollectionMember()
        {
            // Arrange
            // This time, going to disable List injection and force the decimals into 
            // being resolved as enumerables so the collection binder can do its thang.
            var config = TargetContainer.DefaultConfig.Clone();
            config.ConfigureOption<Options.EnableCollectionInjection>(false);
            var targets = CreateTargetContainer(configOverride: config);

            targets.RegisterObject(10m);
            targets.RegisterObject(20m);

            targets.RegisterType<HasMembers>(b => b.Bind(hm => hm.BindableListOfDecimals).AsCollection());

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasMembers>();

            // Assert
            Assert.Equal(new[] { 10m, 20m }, result.BindableListOfDecimals);
        }

        [Fact]
        public void Members_ShouldAutoBindIListMember()
        {
            // issue #69

            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(1);
            targets.RegisterObject(2);
            targets.RegisterObject(3);
            targets.RegisterType<HasIListMember<int>>(MemberBindingBehaviour.BindAll);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasIListMember<int>>();

            // Assert
            Assert.Equal(Enumerable.Range(1, 3), result.Children);
        }

        [Fact]
        public void Members_ShouldExplicitlyBindIListMember()
        {
            // issue #69

            // Arrange
            var targets = CreateTargetContainer();

            targets.RegisterType<HasIListMember<int>>(m => m.Bind(o => o.Children).AsCollection(Target.ForObject(1), Target.ForObject(2), Target.ForObject(3)));
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasIListMember<int>>();

            // Assert
            Assert.Equal(Enumerable.Range(1, 3), result.Children);
        }

        [Fact]
        public void Members_ShouldExplicitlyBindIListMember_ResolvingDerivedTypes()
        {
            // issue #69 still, but might affect more than just IList

            // Arrange
            var targets = CreateTargetContainer();

            targets.RegisterType<BaseClassChild>();
            targets.RegisterType<BaseClassGrandchild>();
            targets.RegisterType<HasIListMember<BaseClass>>(m => m.Bind(o => o.Children).AsCollection(typeof(BaseClassChild), typeof(BaseClassGrandchild)));
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasIListMember<BaseClass>>();

            // Assert
            Assert.Equal(2, result.Children.Count);
            Assert.IsType<BaseClassChild>(result.Children[0]);
            Assert.IsType<BaseClassGrandchild>(result.Children[1]);
        }

        [Fact]
        public void Members_ShouldAutoBindCompliantCustomCollectionMember()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(1);
            targets.RegisterObject(2);
            targets.RegisterObject(3);
            targets.RegisterType<HasCustomCollection>(MemberBindingBehaviour.BindAll);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCustomCollection>();

            // Assert
            Assert.Equal(Enumerable.Range(1, 3), result.Integers);
        }

        [Fact]
        public void Members_ShouldExplicitlyBindCustomCollection()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(1);
            targets.RegisterObject(2);
            targets.RegisterObject(3);
            // can do .AsCollection or not, is doesn't matter
            targets.RegisterType<HasCustomCollection>(b => b.Bind(hcc => hcc.Integers));
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCustomCollection>();

            // Assert
            Assert.Equal(Enumerable.Range(1, 3), result.Integers);
        }

        [Fact]
        public void Members_ShouldBindToCollectionWithExplicitItems()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<HasCollectionMember>(b => 
                b.Bind(c => c.Numbers).AsCollection(
                    Target.ForObject(1),
                    Target.ForObject(2),
                    Target.ForObject(3)));

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCollectionMember>();

            // Assert
            Assert.Equal(Enumerable.Range(1, 3), result.Numbers);
        }

        [Fact]
        public void Members_ShouldExplicitlyBindCollection_Covariantly()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<BaseClass>();
            targets.RegisterType<BaseClassChild>();
            targets.RegisterType<BaseClassGrandchild>();
            targets.RegisterType<HasCollectionOfBaseClass>(b =>
                b.Bind(o => o.Collection));
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCollectionOfBaseClass>();

            // Assert
            Assert.Equal(3, result.Collection.Count);
            Assert.IsType<BaseClass>(result.Collection[0]);
            Assert.IsType<BaseClassChild>(result.Collection[1]);
            Assert.IsType<BaseClassGrandchild>(result.Collection[2]);
        }

        [Fact]
        public void Members_ShouldExplicitlyBindCollection_WithExplicitTypes()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<BaseClass>();
            targets.RegisterType<BaseClassChild>();
            targets.RegisterType<BaseClassGrandchild>();
            targets.RegisterType<HasCollectionOfBaseClass>(b =>
                b.Bind(o => o.Collection).AsCollection(typeof(BaseClassChild), typeof(BaseClassGrandchild)));
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCollectionOfBaseClass>();

            // Assert
            Assert.Equal(2, result.Collection.Count);
            Assert.IsType<BaseClassChild>(result.Collection[0]);
            Assert.IsType<BaseClassGrandchild>(result.Collection[1]);
        }
    }
}
