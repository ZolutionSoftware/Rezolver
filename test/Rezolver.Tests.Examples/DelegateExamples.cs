// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class DelegateExamples
    {

        [Fact]
        public void ShouldGetMyService()
        {
            // <example1>
            var container = new Container();
            container.RegisterDelegate(() => new MyService());

            var result = container.Resolve<MyService>();
            Assert.NotNull(result);
            // </example1>
        }

        [Fact]
        public void ShouldGetMyServiceAsASingleton()
        {
            // <example2>
            var container = new Container();
            container.Register(
                // RegisterSingleton specialises for types only, so
                // we create the target manually and apply this .Singleton 
                // extension method to to it before registering
                Target.ForDelegate(() => new MyService()).Singleton()
            );

            var result = container.Resolve<MyService>();
            var result2 = container.Resolve<MyService>();

            Assert.Same(result, result2);
            // </example2>
        }

        [Fact]
        public void ScopeShouldDisposeDelegateResult()
        {
            // <example3>
            var container = new Container();
            container.RegisterDelegate(() => new DisposableType());

            DisposableType result, result2;

            using (var scope = container.CreateScope())
            {
                result = scope.Resolve<DisposableType>();
                using (var childScope = scope.CreateScope())
                {
                    result2 = childScope.Resolve<DisposableType>();
                }
                Assert.True(result2.Disposed);
            }
            Assert.True(result.Disposed);
            // </example3>
        }

        [Fact]
        public void ShouldInjectIMyService()
        {
            // <example10>
            var container = new Container();
            container.RegisterType<MyService, IMyService>();
            // delegate requires an IMyService to feed as a dependency, along with
            // another value which is not injection-friendly (DateTime)
            DateTime expectedStartDate = new DateTime(2014, 5, 5);
            container.RegisterDelegate((IMyService service) => 
                new RequiresIMyServiceAndDateTime(service, new DateTime(2014, 5, 5)));

            var result = container.Resolve<RequiresIMyServiceAndDateTime>();
            Assert.NotNull(result.Service);
            Assert.Equal(expectedStartDate, result.StartDate);
            // </example10>
        }

        [Fact]
        public void ShouldInjectResolveContext()
        {
            // <example11>
            var container = new Container();
            // RegisterDelegate has a specialisation for a delegate which takes ResolveContext
            container.RegisterDelegate(rc => new RequiresResolveContext(rc));

            var result = container.Resolve<RequiresResolveContext>();
            // the context was injected
            Assert.NotNull(result.Context);
            // and the container on the context will be the one on which we called Resolve<>
            Assert.Same(container, result.Context.Container);
            // </example11>
        }

        // <example12>
        static IPrincipal CurrentPrincipal { get; set; }

        [Fact]
        public void ShouldGetDifferentImplementationFromResolveContextForUser()
        {
            
            IIdentity identity = new AppIdentity();
            // three principals, one for each role
            var adminPrincipal = new AppPrincipal(identity, new[] { "Admin" });
            var salesPrincipal = new AppPrincipal(identity, new[] { "Sales" });
            var customerPrincipal = new AppPrincipal(identity, new[] { "Customer" });

            var container = new Container();
            container.RegisterType<AdminActionsService>();
            container.RegisterType<SalesActionsService>();
            container.RegisterType<CustomerActionsService>();
            container.RegisterType<UserControlPanel>();
            // register delegate to read the CurrentPrincipal property, to make it dynamic
            container.RegisterDelegate(() => CurrentPrincipal);
            // now register the delegate handler for the IUserActionsService, which does the
            // role sniffing over the principal
            container.RegisterDelegate((IPrincipal p, IResolveContext rc) => {
                IUserActionsService toReturn = null;
                if (p != null)
                {
                    if (p.IsInRole("Customer"))
                        toReturn = rc.Resolve<CustomerActionsService>();
                    else if (p.IsInRole("Sales"))
                        toReturn = rc.Resolve<SalesActionsService>();
                    else if (p.IsInRole("Admin"))
                        toReturn = rc.Resolve<AdminActionsService>();
                }
                return toReturn;
            });

            // set the principal, and resolve
            CurrentPrincipal = adminPrincipal;
            var result1 = container.Resolve<UserControlPanel>();
            // now swap principals
            CurrentPrincipal = salesPrincipal;
            var result2 = container.Resolve<UserControlPanel>();
            // and again
            CurrentPrincipal = customerPrincipal;
            var result3 = container.Resolve<UserControlPanel>();

            Assert.IsType<AdminActionsService>(result1.ActionsService);
            Assert.IsType<SalesActionsService>(result2.ActionsService);
            Assert.IsType<CustomerActionsService>(result3.ActionsService);
        }
        // </example12>
    }
}
