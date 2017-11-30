﻿using Rezolver.Tests.Types;
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
    }
}
