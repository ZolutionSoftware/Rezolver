using Rezolver.Targets;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ExpressionExamples
    {
        [Fact]
        public void ShouldUseConstant()
        {
            //<example1>
            var container = new Container();
            container.Register(
                new ExpressionTarget(
                    Expression.Constant("Hello World")
                )
            );

            Assert.Equal("Hello World", container.Resolve<string>());
            //</example1>
        }

        //<example2a>
        private string _theMessage = "Hello World";
        public string GetMessage()
        {
            return _theMessage;
        }
        //</example2a>

        [Fact]
        public void ShouldCallMethod()
        {
            //<example2b>
            var container = new Container();

            container.Register(
                new ExpressionTarget(
                    Expression.Call(
                            Expression.Constant(this), //the instance
                            this.GetType().GetMethod("GetMessage")
                        )
                    )
                );
            Assert.Same(_theMessage, container.Resolve<string>());
            //now change _theMessage and re-resolve:
            _theMessage = "New Message!!!";
            Assert.Same(_theMessage, container.Resolve<string>());
            //</example2b>
        }

        [Fact]
        public void ShouldCallMethodOnInjectedObject()
        {
            //<example3>
            //this is equivalent to the previous example, just with a lambda
            //and explicitly injected argument
            var container = new Container();
            //note - the type of 'this' is ExpressionExamples
            container.RegisterObject(this);
            container.RegisterExpression((ExpressionExamples e) => e.GetMessage());

            Assert.Same(_theMessage, container.Resolve<string>());

            _theMessage = "Another New Message!!!";
            Assert.Same(_theMessage, container.Resolve<string>());
            //</example3>
        }

        //<example4>
        static IPrincipal CurrentPrincipal { get; set; }

        [Fact]
        public void ShouldGetDifferentImplementationFromResolveContextForUser()
        {
            //this test is functionally identical to the one in DelegateExamples.cs,
            //it's just done with an expression, and therefore the format of the code block
            //used for the IUserActionsService is different because the compiler can only 
            //translate expression lambdas.
            IIdentity identity = new AppIdentity();
            //three principals, one for each role
            var adminPrincipal = new AppPrincipal(identity, new[] { "Admin" });
            var salesPrincipal = new AppPrincipal(identity, new[] { "Sales" });
            var customerPrincipal = new AppPrincipal(identity, new[] { "Customer" });

            var container = new Container();
            container.RegisterType<AdminActionsService>();
            container.RegisterType<SalesActionsService>();
            container.RegisterType<CustomerActionsService>();
            container.RegisterType<UserControlPanel>();
            //register expression to read the CurrentPrincipal property, to make it dynamic
            container.RegisterExpression(() => CurrentPrincipal);
            //now register the expression for the IUserActionsService, which does the
            //role sniffing over the principal as one expression
            container.RegisterExpression((IPrincipal p, ResolveContext rc) =>
                p.IsInRole("Customer") ?
                rc.Resolve<CustomerActionsService>() :
                    p.IsInRole("Sales") ?
                        rc.Resolve<SalesActionsService>() :
                        p.IsInRole("Admin") ?
                            rc.Resolve<AdminActionsService>() :
                            (IUserActionsService)null);

            //set the principal, and resolve
            CurrentPrincipal = adminPrincipal;
            var result1 = container.Resolve<UserControlPanel>();
            //now swap principals
            CurrentPrincipal = salesPrincipal;
            var result2 = container.Resolve<UserControlPanel>();
            //and again
            CurrentPrincipal = customerPrincipal;
            var result3 = container.Resolve<UserControlPanel>();

            Assert.IsType<AdminActionsService>(result1.ActionsService);
            Assert.IsType<SalesActionsService>(result2.ActionsService);
            Assert.IsType<CustomerActionsService>(result3.ActionsService);
        }
        //</example4>

        [Fact]
        public void ShouldLocateViaHelperInExprFragment()
        {
            //<example5>
            var container = new Container();
            container.RegisterType<MyService, IMyService>();
            //get the ExpressionFunctions.Resolve<T> method
            var resolveMethod = typeof(ExpressionFunctions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SingleOrDefault(m => m.Name == "Resolve" && m.IsGenericMethodDefinition);

            //this expression is equivalent to the lambda expression:
            //rc => new RequiresMyService(rc.Resolve<IMyService>())
            container.Register(
                new ExpressionTarget(
                    Expression.New(
                        //get the (IMyService) constructor
                        typeof(RequiresMyService).GetConstructor(new[] {
                            typeof(IMyService)
                        }),
                        Expression.Call(
                            resolveMethod.MakeGenericMethod(typeof(IMyService))
                        )
                    )
                ));

            var result = container.Resolve<RequiresMyService>();

            Assert.NotNull(result.Service);
            //</example5>
        }

        [Fact]
        public void ShouldLocateViaHelperInParameterlessLambda()
        {
            //<example6>
            var container = new Container();
            container.RegisterType<MyService, IMyService>();
            container.RegisterExpression(() =>
                new RequiresMyService(ExpressionFunctions.Resolve<IMyService>())
                );
            var result = container.Resolve<RequiresMyService>();

            Assert.NotNull(result.Service);
            //</example6>
        }
    }
}
