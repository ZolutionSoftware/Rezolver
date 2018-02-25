// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Targets;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rezolver.Compilation;
using System.Reflection;

namespace Rezolver.Tests.Examples
{
    public class MemberBindingExamples
    {
        [Fact]
        public void ShouldInject2MembersWithAllMembersBehaviour()
        {
            // <example1>
            var container = new Container();
            container.RegisterAll(
                Target.ForType<MyService1>(),
                Target.ForType<MyService2>()
            );
            container.RegisterType<Has2InjectableMembers>(MemberBindingBehaviour.BindAll);

            var result = container.Resolve<Has2InjectableMembers>();

            Assert.NotNull(result.Service1);
            Assert.NotNull(result.Service2);
            // </example1>
        }

        [Fact]
        public void ShouldInject2MembersWithAllMembersBehaviour_PassedWhenCreatingTarget()
        {
            // <example1b>
            var container = new Container();
            container.RegisterAll(
                Target.ForType<MyService1>(),
                Target.ForType<MyService2>()
            );

            var target = Target.ForType<Has2InjectableMembers>(MemberBindingBehaviour.BindAll);
            container.Register(target);

            var result = container.Resolve<Has2InjectableMembers>();

            Assert.NotNull(result.Service1);
            Assert.NotNull(result.Service2);
            // </example1b>
        }

        [Fact]
        public void ShouldInject2MembersWithAllMembersBehaviour_FromGlobalOption()
        {
            // <example2>
            var container = new Container();
            container.SetOption(MemberBindingBehaviour.BindAll);
            container.RegisterAll(
                Target.ForType<MyService1>(),
                Target.ForType<MyService2>()
            );
            container.RegisterType<Has2InjectableMembers>();

            var result = container.Resolve<Has2InjectableMembers>();

            Assert.NotNull(result.Service1);
            Assert.NotNull(result.Service2);
            // </example2>
        }

        [Fact]
        public void ShouldInject2MembersWithAllMembersBehaviour_FromTypeSpecificOption()
        {
            // <example3>
            var container = new Container();
            // Here - the behaviour will *only* kick in for the Has2InjectableMembers type
            container.SetOption(MemberBindingBehaviour.BindAll, typeof(Has2InjectableMembers));
            container.RegisterAll(
                Target.ForType<MyService1>(),
                Target.ForType<MyService2>()
            );
            container.RegisterType<Has2InjectableMembers>();
            container.RegisterType<AlsoHas2InjectableMembers>();

            var result1 = container.Resolve<Has2InjectableMembers>();
            var result2 = container.Resolve<AlsoHas2InjectableMembers>();

            Assert.NotNull(result1.Service1);
            Assert.NotNull(result1.Service2);
            // but this instance shouldn't have had any members injected
            Assert.Null(result2.Service1);
            Assert.Null(result2.Service2);
            // </example3>
        }

        [Fact]
        public void ShouldInjectMembersOfDerivedType_ViaOption()
        {
            // <example4>
            var container = new Container();
            container.SetOption(MemberBindingBehaviour.BindAll, typeof(Has2InjectableMembers));
            container.RegisterAll(
                Target.ForType<MyService1>(),
                Target.ForType<MyService2>()
            );
            // note - registering the Inherits2InjectableMembers type,
            // which derives from Has2InjectableMembers
            container.RegisterType<Inherits2InjectableMembers>();

            var result = container.Resolve<Inherits2InjectableMembers>();

            Assert.NotNull(result.Service1);
            Assert.NotNull(result.Service2);
            // </example4>
        }

        [Fact]
        public void ShouldInjectMembersWithAttribute()
        {
            // <example5>
            var container = new Container();

            // register the type which uses the attributes, passing our custom binding behaviour
            container.RegisterType<HasAttributeInjectedMembers>(new AttributeBindingBehaviour());

            // The first injected field will request an IMyService
            container.RegisterType<MyService1, IMyService>();
            // The second requests an MyService6 by way of the type override on the attribute
            container.RegisterType<MyService6>();

            var result = container.Resolve<HasAttributeInjectedMembers>();

            Assert.IsType<MyService1>(result.InjectedServiceField);
            Assert.IsType<MyService6>(result.InjectedServiceProp);
            Assert.Null(result.ServiceField);
            Assert.Null(result.ServiceProp);
            // </example5>
        }

        [Fact]
        public void Fluent_ShouldRegisterATypeWithEmbeddedBehaviour()
        {
            // <example6>
            var container = new Container();

            container.RegisterType<MyService2>();
            container.RegisterType<Has2InjectableMembers>(b => 
                b.Bind(o => o.Service2));

            var result = container.Resolve<Has2InjectableMembers>();

            Assert.Null(result.Service1);
            Assert.NotNull(result.Service2);
            // </example6>
        }

        [Fact]
        public void Fluent_ShouldCreateATargetWithEmbeddedBehaviour()
        {
            // <example7>
            var container = new Container();

            container.RegisterType<MyService1>();
            var has2IMTarget = Target.ForType<Has2InjectableMembers>(b => 
                b.Bind(o => o.Service1));
            container.Register(has2IMTarget);

            var result = container.Resolve<Has2InjectableMembers>();

            Assert.NotNull(result.Service1);
            Assert.Null(result.Service2);
            // </example7>
        }

        [Fact]
        public void Fluent_ShouldBindIdenticalMembersWithDifferentServices()
        {
            // <example8>
            var container = new Container();

            container.RegisterType<MyService1>();
            MyService2 constantService2 = new MyService2();

            container.RegisterType<Has2IdenticalMembers>(b => 
                b.Bind(s => s.Service1).ToType<MyService1>()
                 .Bind(s => s.Service2).ToObject(constantService2));

            var result = container.Resolve<Has2IdenticalMembers>();

            Assert.NotNull(result.Service1);
            Assert.Same(constantService2, result.Service2);
            // </example8>
        }

        [Fact]
        public void Collections_ShouldAddSomeServicesToExisting()
        {
            // <example9>
            var container = new Container();

            // this example also shows enumerable covariance in action
            container.RegisterType<MyService2>();
            container.RegisterType<MyService3>();

            container.RegisterType<HasInjectableCollection>(MemberBindingBehaviour.BindAll);

            var result = container.Resolve<HasInjectableCollection>();

            Assert.Equal(3, result.Services.Count);
            Assert.IsType<MyService1>(result.Services[0]);
            Assert.IsType<MyService2>(result.Services[1]);
            Assert.IsType<MyService3>(result.Services[2]);
            // </example9>
        }

        [Fact]
        public void Collections_ShouldAddToGenericCustomCollections()
        {
            // <example10>
            var container = new Container();

            container.RegisterObject(1);
            container.RegisterObject(2);
            container.RegisterObject(3);

            container.RegisterObject("oh, ");
            container.RegisterObject("hello");
            container.RegisterObject("world!");

            container.RegisterType(typeof(HasCustomCollection<>), 
                memberBinding: MemberBindingBehaviour.BindAll);

            var hasInts = container.Resolve<HasCustomCollection<int>>();
            var hasStrings = container.Resolve<HasCustomCollection<string>>();

            // remember - a default(T) instance is added on construction
            Assert.Equal(new[] { 0, 1, 2, 3 }, hasInts.List);
            Assert.Equal(new[] { null, "oh, ", "hello", "world!" }, hasStrings.List);
            // </example10>
        }

        [Fact]
        public void Fluent_Collections_ShouldUseSuppliedInts()
        {
            // <example11>
            var container = new Container();

            // these will be ignored
            container.RegisterObject(1);
            container.RegisterObject(2);
            container.RegisterObject(3);

            // can't use the fluent API on open generics
            container.RegisterType<HasCustomCollection<int>>(mb =>
                mb.Bind(hcc => hcc.List).AsCollection(
                    Target.ForObject(10),
                    Target.ForObject(11),
                    Target.ForObject(12)));

            var result = container.Resolve<HasCustomCollection<int>>();

            Assert.Equal(new[] { 0, 10, 11, 12 }, result.List);
            // </example11>
        }

        [Fact]
        public void Collections_ShouldFailToBindWritableCollectionByDefault()
        {
            // <example12>
            var container = new Container();

            // will make no difference
            container.RegisterObject(1);

            container.RegisterType<HasWritableCustomCollection<int>>(MemberBindingBehaviour.BindAll);

            // if instead we repeated this with HasCustomCollection, we'd just get an instance
            // whose list only contained the default item that was added in the constructor.
            Assert.ThrowsAny<InvalidOperationException>(
                () => container.Resolve<HasWritableCustomCollection<int>>());
            // </example12>
        }

        [Fact]
        public void Fluent_Collections_ShouldTreatWritableProperty_AsCollection()
        {
            // <example13>
            var container = new Container();

            container.RegisterObject(1);

            container.RegisterType<HasWritableCustomCollection<int>>(mb =>
                mb.Bind(hwcc => hwcc.List).AsCollection());

            var result = container.Resolve<HasWritableCustomCollection<int>>();

            Assert.Equal(new[] { 0, 1 }, result.List);
            // </example13>
        }
    }
}
