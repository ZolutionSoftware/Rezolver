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
            container.RegisterType<Has2InjectableMembers>(DefaultMemberBindingBehaviour.Instance);

            var result = container.Resolve<Has2InjectableMembers>();

            Assert.NotNull(result.Service1);
            Assert.NotNull(result.Service2);
            // </example1>
        }



        [Fact]
        public void ShouldInjectMembersWithAttribute()
        {
            // <example2>
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
            // </example2>
        }
    }
}
