using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace Rezolver.Tests
{
    [TestClass]
    public class ScopeTests : TestsBase
    {
        public class DisposableBase : IDisposable
        {
            public bool Disposed { get; private set; }

            public virtual void Dispose()
            {
                //
                if (Disposed)
                    throw new ObjectDisposedException(GetType().Name);

                Disposed = true;
            }
        }

        public class SingletonDependency : DisposableBase
        {
            
        }

        public class Disposable1 : DisposableBase
        {

        }

        public class Disposable2 : DisposableBase
        {

        }

        public class RequiresDisposables : DisposableBase
        {

            public RequiresDisposables(Disposable1 disposable1, Disposable2 disposable2)
            {
                this.Disposable1 = disposable1;
                this.Disposable2 = disposable2;
            }

            public Disposable1 Disposable1 { get; private set; }
            public Disposable2 Disposable2 { get; private set; }
        }

        [TestMethod]
        public void ShouldDisposeTransientsResolvedAsDependencies()
        {
            Disposable1 disp1; Disposable2 disp2; RequiresDisposables reqDisp;

            using (var container = new DefaultLifetimeScopeRezolver())
            {
                container.RegisterType<Disposable1>();
                container.RegisterType<Disposable2>();
                container.RegisterType<RequiresDisposables>();

                reqDisp = container.Resolve<RequiresDisposables>();
                disp1 = reqDisp.Disposable1;
                disp2 = reqDisp.Disposable2;
            }

            Assert.IsTrue(reqDisp.Disposed);
            Assert.IsTrue(disp1.Disposed);
            Assert.IsTrue(disp2.Disposed);
        }

        [TestMethod]
        public void ShouldDisposeTransientCreatedInChildScope()
        {
            Disposable1 disp1, disp1a;

            using (var container = new DefaultLifetimeScopeRezolver())
            {
                container.RegisterType<Disposable1>();
                disp1 = container.Resolve<Disposable1>();
                using (var childContainer = container.CreateLifetimeScope())
                {
                    disp1a = childContainer.Resolve<Disposable1>();
                }
                Assert.IsTrue(disp1a.Disposed);
                Assert.IsFalse(disp1.Disposed);
            }

            Assert.IsTrue(disp1.Disposed);
        }

        /// <summary>
        /// this is a test scenario taken from the DNX AspNet.DependencyInjection library.
        /// </summary>
        [TestMethod]
        public void SingletonShouldComeFromRootContainer()
        {
            SingletonDependency root, dep1, dep2;
            //the key element of this test is that we have a singleton object that is not explicitly registered
            //as scoped.  This means that, naturally, the first scope to resolve the instance will want to track
            //it.  In fact, however, it should be tracked in the root-level scoping container because it has 
            //effectively global lifetime.

            //this has interesting side effects for object targets, which are also effectively singletons - except
            //code outside the container will have created it first.  Who should dispose of those?
            using (var container = new DefaultLifetimeScopeRezolver())
            {
                container.Builder.Register(new SingletonTarget(ConstructorTarget.Auto<SingletonDependency>()));

                using (var childScope = container.CreateLifetimeScope())
                {
                    dep1 = childScope.Resolve<SingletonDependency>();
                }
                Assert.IsFalse(dep1.Disposed);

                using (var childScope = container.CreateLifetimeScope())
                {
                    dep2 = childScope.Resolve<SingletonDependency>();
                }
                Assert.IsFalse(dep2.Disposed);

                root = container.Resolve<SingletonDependency>();

                Assert.AreSame(root, dep1);
                Assert.AreSame(dep1, dep2);
            }

            Assert.IsTrue(root.Disposed);

        }

        [TestMethod]
        public void BuildLazyViaExpression()
        {
            //this is purely a piece of proof of concept work for the SingletonTarget
            var ctor = MethodCallExtractor.ExtractConstructorCall(() => new Lazy<SingletonDependency>(() => new SingletonDependency()));
            //let's see if we can build the lazy via an expression
            var expr = Expression.Lambda(Expression.New(ctor, Expression.Lambda(Expression.New(typeof(SingletonDependency)))));
            var method = expr.Compile();
            //should be a func
            var func = (Func<Lazy<SingletonDependency>>)method;
            var lazy = func();
            Assert.IsFalse(lazy.IsValueCreated);
            var instance = lazy.Value;
            Assert.IsTrue(lazy.IsValueCreated);
            var lazy2 = func();
            var instance2 = lazy2.Value;
            Assert.AreNotSame(lazy, lazy2);
        }
    }
}