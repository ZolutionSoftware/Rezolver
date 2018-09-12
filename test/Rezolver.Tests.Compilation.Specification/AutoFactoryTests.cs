using Rezolver.Compilation.Expressions;
using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        private IRootTargetContainer CreateAutoFactoryTargetContainer()
        {
            var targets = CreateTargetContainer();

            // common funcs
            targets.RegisterContainer(typeof(Func<>), new Func0TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,>), new Func1TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,>), new Func2TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,>), new Func3TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,>), new Func4TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,>), new Func5TargetContainer(targets));

            // extended funcs
            targets.RegisterContainer(typeof(Func<,,,,,,>), new Func6TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,>), new Func7TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,>), new Func8TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,>), new Func9TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,,>), new Func10TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,,,>), new Func11TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,,,,>), new Func12TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,,,,,>), new Func13TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,,,,,,>), new Func14TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,,,,,,,>), new Func15TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,,,,,,,,,,,>), new Func16TargetContainer(targets));

            targets.SetOption((IExpressionBuilder)new AutoFactoryTargetBuilder(), typeof(AutoFactoryTarget));
            return targets;
        }

        [Fact]
        public void AutoFactory_ShouldCreateSimple()
        {
            // simplest scenario - auto-creating a Func<T> instead of producing a T

            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterType<NoCtor>();
            var container = CreateContainer(targets);

            // Act
            Func<NoCtor> result = container.Resolve<Func<NoCtor>>();

            // Assert
            Assert.NotNull(result);
            var instance = result();
            var instance2 = result();
            Assert.NotNull(instance);
            Assert.NotNull(instance2);
            Assert.NotSame(instance, instance2);
        }

        [Fact]
        public void AutoFactory_DisposableShouldHonourImplicitScope()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterType<Disposable>();
            var container = CreateContainer(targets);

            // When a factory is created, it is *bound* to the scope from which you created it,
            // so will only create objects while that scope is alive - and the same is true for all
            // disposable instances that it creates.
            // In this case, we're testing objects which *happen* to be disposable and which are, therefore, implicitly scoped.

            Disposable outer1, outer2, inner1, inner2;
            using (var outerScope = container.CreateScope())
            {
                // Act
                var outerFactory = outerScope.Resolve<Func<Disposable>>();
                outer1 = outerFactory();
                outer2 = outerFactory();

                using (var innerScope = outerScope.CreateScope())
                {
                    var innerFactory = innerScope.Resolve<Func<Disposable>>();
                    inner1 = innerFactory();
                    inner2 = innerFactory();

                    // Assert
                    Assert.NotSame(outer1, inner1);
                    Assert.NotSame(inner1, inner2);
                }
                Assert.True(inner1.Disposed);
                Assert.True(inner2.Disposed);
                Assert.False(outer1.Disposed);
                Assert.False(outer2.Disposed);
            }

            Assert.True(outer1.Disposed);
            Assert.True(outer2.Disposed);
        }


        [Fact]
        public void AutoFactory_DisposableShouldHonourExplicitScope()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterScoped<Disposable>();
            var container = CreateContainer(targets);

            // When a factory is created, it is *bound* to the scope from which you created it,
            // so will only create objects while that scope is alive - and the same is true for all
            // disposable instances that it creates.  

            // This test looks specifically at explicitly scoped objects and, therefore, tests that 
            // the factory produces the same instance according to the scope that the factory was built from.

            Disposable outer, inner;
            using (var outerScope = container.CreateScope())
            {
                // Act
                var outerFactory = outerScope.Resolve<Func<Disposable>>();
                outer = outerFactory();
                var outer2 = outerFactory();

                using (var innerScope = outerScope.CreateScope())
                {
                    var innerFactory = innerScope.Resolve<Func<Disposable>>();
                    inner = innerFactory();
                    var inner2 = innerFactory();

                    // Assert
                    Assert.NotSame(outer, inner);
                    Assert.Same(inner, inner2);
                }
                Assert.Same(outer, outer2);
                Assert.True(inner.Disposed);
                Assert.False(outer.Disposed);
            }
            Assert.True(outer.Disposed);
            Assert.Equal(1, outer.DisposedCount);
            Assert.Equal(1, inner.DisposedCount);
        }

        [Fact]
        public void AutoFactory_SingletonShouldHonourRootScope()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();

            targets.RegisterSingleton<Disposable>();
            var container = CreateContainer(targets);

            // this time, different factories should bind to the same singleton

            Disposable outer, inner;
            using (var outerScope = container.CreateScope())
            {
                // Act
                var outerFactory = outerScope.Resolve<Func<Disposable>>();
                outer = outerFactory();
                using (var innerScope = outerScope.CreateScope())
                {
                    var innerFactory = innerScope.Resolve<Func<Disposable>>();
                    inner = innerFactory();

                    // Assert
                    Assert.Same(outer, inner);
                }
                Assert.False(outer.Disposed);
            }
            Assert.True(outer.Disposed);
        }

        [Fact]
        public void AutoFactory_MultipleFactoriesShouldStillHonourExplicitScoping()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();

            // this time we'll register a scoped object and create one scope,
            // then we'll resolve multiple factories (verifying that they are different instances)
            // then try to resolve instances from them.  Each factory should produce the same instance.

            targets.RegisterScoped<Disposable>();
            var container = CreateContainer(targets);

            // Act
            Disposable instance1, instance2, instance3;
            using (var scope = container.CreateScope())
            {
                // resolve multiple factories
                var fact1 = scope.Resolve<Func<Disposable>>();
                var fact2 = scope.Resolve<Func<Disposable>>();
                var fact3 = scope.Resolve<Func<Disposable>>();

                instance1 = fact1();
                instance2 = fact2();
                instance3 = fact3();

                // Assert
                Assert.NotSame(fact1, fact2);
                Assert.NotSame(fact2, fact3);

                Assert.Same(instance1, instance2);
                Assert.Same(instance2, instance3);
            }

            Assert.True(instance1.Disposed);
        }

        [Fact]
        public void AutoFactory_ShouldAcceptResolvedDependencyAsArgument()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();

            targets.RegisterType<RequiresInt>();
            var container = CreateContainer(targets);

            // Act
            var factory = container.Resolve<Func<int, RequiresInt>>();

            // Assert
            var instance = factory(10);

            Assert.NotNull(instance);
            Assert.Equal(10, instance.IntValue);
            var instance2 = factory(20);
            Assert.NotSame(instance, instance2);
            Assert.NotNull(instance2);
            Assert.Equal(20, instance2.IntValue);
        }

        [Fact]
        public void AutoFactory_ShouldUseArgumentEvenWhenDependencyRegistered()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterType<RequiresInt>();
            targets.RegisterObject(10);
            var container = CreateContainer(targets);

            // Act
            var factory = container.Resolve<Func<int, RequiresInt>>();
            var normalInstance = container.Resolve<RequiresInt>();

            // Assert
            Assert.Equal(10, normalInstance.IntValue);
            Assert.Equal(20, factory(20).IntValue);
        }

        [Fact]
        public void AutoFactory_ShouldSupportEnumerableViaEnumerableContainer()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterType<BaseClass>();
            targets.RegisterType<BaseClassChild>();
            targets.RegisterType<BaseClassGrandchild>();

            var container = CreateContainer(targets);

            // Act
            var factory = container.Resolve<Func<IEnumerable<BaseClass>>>();
            var instance = factory().ToArray();

            // Assert
            Assert.Collection(instance, i => Assert.IsType<BaseClass>(i), i => Assert.IsType<BaseClassChild>(i), i => Assert.IsType<BaseClassGrandchild>(i));
        }

        [Fact]
        public void AutoFactory_ShouldBeAbleToResolveAnEnumerableOfAutoFactoriesViaCovariance()
        {
            // Similar to above, but the implementation here we're registering the individual types and then
            // getting an IEnumerable<Func<BaseClass>>.
            // Key things being tested here are:
            // 1) That the implementation honours the covariance required by IEnumerable<out T>
            // 2) That the order of the generated delegates reflects the registration order of the underlying targets

            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterType<BaseClass>();
            targets.RegisterType<BaseClassChild>();
            targets.RegisterType<BaseClassGrandchild>();

            var container = CreateContainer(targets);

            // Act
            var factories = container.ResolveMany<Func<BaseClass>>();
            var instances = factories.Select(f => f());

            // Assert
            Assert.Collection(instances, i => Assert.IsType<BaseClass>(i), i => Assert.IsType<BaseClassChild>(i), i => Assert.IsType<BaseClassGrandchild>(i));
        }

        [Fact]
        public void AutoFactory_ShouldBeAbleToResolveAnEumerableOfParameterisedAutoFactoriesViaCovariance()
        {
            // this time, it's like above, but we're also doing an enumerable of automatic 
            // parameterised factories, and we want to test that the parameters work correctly

            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterType<OneCtor>();
            targets.RegisterType<OneCtorAlt1>();
            targets.RegisterType<OneCtorAlt2>();

            var container = CreateContainer(targets);

            // Act
            var factories = container.ResolveMany<Func<int, NoCtor>>();
            var instances = factories.Select(f => f(50));

            // Assert
            Assert.Collection(instances, i => Assert.IsType<OneCtor>(i), i => Assert.IsType<OneCtorAlt1>(i), i => Assert.IsType<OneCtorAlt2>(i));
            Assert.Equal(new[] { 50, 50, 50 }, instances.Select(i => i.Value));
        }
    }

    /// <summary>
    /// Target dictionary container which provides support for automatic production of Func<> delegates
    /// </summary>
    internal class FuncTargetContainerBase : GenericTargetContainer
    {
        private Type[] _objectArgsForFunc;
        internal FuncTargetContainerBase(IRootTargetContainer root, Type funcType)
            : base(root, funcType)
        {
            root.TargetRegistered += Root_TargetRegistered;

            // prepare the type arguments needed to cover the func's argument types (which will
            // all be set to object) for when adding known types for any which receive specific 
            // registrations.  E.g. when a caller registers IFoo, we mark Func<IFoo> as known,
            // but also Func<object, object, IFoo> etc.
            var genericTypeParameters = TypeHelpers.GetGenericArguments(funcType);
            _objectArgsForFunc = new Type[genericTypeParameters.Length - 1];
            for(var f = 0; f < _objectArgsForFunc.Length; f++)
            {
                _objectArgsForFunc[f] = typeof(object);
            }
        }

        private void Root_TargetRegistered(object sender, Events.TargetRegisteredEventArgs e)
        {
            // bit squeaky, this - will slow up registrations, hence why the configuration will split the auto 
            // func registration between 'common' funcs and 'extended' funcs.
            var type = GenericType.MakeGenericType(_objectArgsForFunc.Concat(new[] { e.Type }).ToArray());
            Root.AddKnownType(type);
        }

        public override ITarget Fetch(Type type)
        {
            Type genericType;
            if (!TypeHelpers.IsGenericType(type) || (genericType = type.GetGenericTypeDefinition()) != GenericType)
            {
                throw new ArgumentException($"Only {GenericType} is supported by this container", nameof(type));
            }

            // just like EnumerableTargetContainer, we allow for specific Func<T> registrations
            var result = base.Fetch(type);

            if (result != null)
                return result;

            var typeArgs = TypeHelpers.GetGenericArguments(type);

            var requiredReturnType = typeArgs[typeArgs.Length - 1];

            var innerTarget = Root.Fetch(requiredReturnType);
            // returning NULL above for AutoFactory_ShouldBeAbleToResolveAnEumerableOfParameterisedAutoFactoriesViaCovariance
            if (innerTarget == null)
                return null;

            // create a func target (new type) which wraps the inner target
            // TODO: if the target is not found, then emit one which wraps a late-bound resolve operation
            // -- unless parameters are required on the delegate type.
            return new AutoFactoryTarget(innerTarget, type, requiredReturnType, typeArgs.Take(typeArgs.Length - 1).ToArray());
        }

        public override IEnumerable<ITarget> FetchAll(Type type)
        {
            // required to allow interoperability with the FetchAll() functionality; because the targets we return are
            // not in the underlying dictionary, so we have to 
            var baseResult = base.FetchAll(type);
            if (!baseResult.Any())
                return new[] { Fetch(type) };
            return baseResult;
        }
    }

    internal class AutoFactoryTargetBuilder : ExpressionBuilderBase<AutoFactoryTarget>
    {
        protected override Expression Build(AutoFactoryTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var newContext = context.NewContext(target.ReturnType);
            ParameterExpression[] parameters = new ParameterExpression[0];
            // if there are parameters, we have to replace any Resolve calls for the parameter types in 
            // the inner expression with parameter expressions fed from the outer lambda
            if (target.ParameterTypes.Length != 0)
            {
                parameters = target.ParameterTypes.Select((pt, i) => Expression.Parameter(pt, $"p{i}")).ToArray();
                Dictionary<Type, ParameterExpression> lookup = parameters.ToDictionary(pe => pe.Type);
                // we're going to add a compilation filter.  The correct way to do this is to grab any existing compilation filter
                // from the context and then set a new one with the new filters in it, and the original filter as the last entry
                var newFilters = new ExpressionCompilationFilters(
                    target.ParameterTypes.Select(t =>
                        new Func<ITarget, IExpressionCompileContext, IExpressionCompiler, Expression>((ta, ctx, cmp) =>
                        {
                            if (ta is ResolvedTarget && ta.DeclaredType == t && lookup.TryGetValue(ta.DeclaredType, out ParameterExpression replacement))
                                return replacement;
                            return null;
                        })
                    ).ToArray()
                );
                var existingFilter = newContext.GetOption<IExpressionCompilationFilter>();
                if (existingFilter != null)
                    newFilters.Add(existingFilter);
                newContext.SetOption<IExpressionCompilationFilter>(newFilters);
            }

            var baseExpression = compiler.BuildResolveLambda(target.InnerTarget, newContext);
            var lambda = Expression.Lambda(target.DelegateType,
                Expression.Convert(Expression.Invoke(baseExpression, context.ResolveContextParameterExpression), target.ReturnType), parameters);
            return lambda;
        }
    }


    internal class AutoFactoryTarget : TargetBase
    {
        public override Type DeclaredType => DelegateType;

        public ITarget InnerTarget { get; }
        public Type DelegateType { get; }
        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }

        public AutoFactoryTarget(ITarget innerTarget, Type delegateType, Type returnType, Type[] parameterTypes)
            : base(innerTarget.Id) // note here - passing the ID in from the inner target to preserve the order.
        {
            InnerTarget = innerTarget ?? throw new ArgumentNullException(nameof(innerTarget));
            DelegateType = delegateType ?? throw new ArgumentNullException(nameof(delegateType));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ParameterTypes = parameterTypes ?? Type.EmptyTypes;
            if (ParameterTypes.Distinct().Count() != ParameterTypes.Length)
                throw new ArgumentException($"Invalid auto factory delegate type: {delegateType} - all parameter types must be unique", nameof(parameterTypes));
        }
    }

    internal class Func0TargetContainer : FuncTargetContainerBase
    {
        internal Func0TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<>))
        {
            //root.TargetRegistered += Root_TargetRegistered;
        }

        //private void Root_TargetRegistered(object sender, Events.TargetRegisteredEventArgs e)
        //{
        //    this.Root.AddKnownType(typeof(Func<>).MakeGenericType(e.Type));
        //}
    }

    internal class Func1TargetContainer : FuncTargetContainerBase
    {
        internal Func1TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,>))
        {

        }
    }

    internal class Func2TargetContainer : FuncTargetContainerBase
    {
        internal Func2TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,>))
        {

        }
    }

    internal class Func3TargetContainer : FuncTargetContainerBase
    {
        internal Func3TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,>))
        {

        }
    }

    internal class Func4TargetContainer : FuncTargetContainerBase
    {
        internal Func4TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,>))
        {

        }
    }

    internal class Func5TargetContainer : FuncTargetContainerBase
    {
        internal Func5TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,>))
        {

        }
    }

    internal class Func6TargetContainer : FuncTargetContainerBase
    {
        internal Func6TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,>))
        {

        }
    }

    internal class Func7TargetContainer : FuncTargetContainerBase
    {
        internal Func7TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,>))
        {

        }
    }

    internal class Func8TargetContainer : FuncTargetContainerBase
    {
        internal Func8TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,>))
        {

        }
    }

    internal class Func9TargetContainer : FuncTargetContainerBase
    {
        internal Func9TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,>))
        {

        }
    }

    internal class Func10TargetContainer : FuncTargetContainerBase
    {
        internal Func10TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,,>))
        {

        }
    }

    internal class Func11TargetContainer : FuncTargetContainerBase
    {
        internal Func11TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,,,>))
        {

        }
    }

    internal class Func12TargetContainer : FuncTargetContainerBase
    {
        internal Func12TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,,,,>))
        {

        }
    }

    internal class Func13TargetContainer : FuncTargetContainerBase
    {
        internal Func13TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,,,,,>))
        {

        }
    }

    internal class Func14TargetContainer : FuncTargetContainerBase
    {
        internal Func14TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,,,,,,>))
        {

        }
    }

    internal class Func15TargetContainer : FuncTargetContainerBase
    {
        internal Func15TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,,,,,,,>))
        {

        }
    }

    internal class Func16TargetContainer : FuncTargetContainerBase
    {
        internal Func16TargetContainer(IRootTargetContainer root)
            : base(root, typeof(Func<,,,,,,,,,,,,,,,,>))
        {

        }
    }
}
