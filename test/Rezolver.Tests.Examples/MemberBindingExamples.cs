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
    }
}
