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
            targets.RegisterContainer(typeof(Func<>), new Func0TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,>), new Func1TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,>), new Func2TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,>), new Func3TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,>), new Func4TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,>), new Func5TargetContainer(targets));
            targets.RegisterContainer(typeof(Func<,,,,,,>), new Func6TargetContainer(targets));

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
        public void AutoFactory_ShouldHonourScope()
        {
            // Arrange
            var targets = CreateAutoFactoryTargetContainer();
            targets.RegisterScoped<Disposable>();
            var container = CreateContainer(targets);

            // When a factory is created, it is *bound* to the scope from which you created it.

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
                    Assert.NotSame(outer, inner);
                }
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
    }

    /// <summary>
    /// Target dictionary container which provides support for automatic production of Func<> delegates
    /// </summary>
    internal class FuncTargetContainerBase : GenericTargetContainer
    {
        internal FuncTargetContainerBase(IRootTargetContainer root, Type funcType)
            : base(root, funcType)
        {

        }

        public override ITarget Fetch(Type type)
        {
            Type genericType;
            if (!TypeHelpers.IsGenericType(type) || (genericType = type.GetGenericTypeDefinition()) != GenericType)
            {
                throw new ArgumentException($"Only {GenericType} is supported by this container", nameof(type));
            }

            // ust like EnumerableTargetContainer, we allow for specific Func<T> registrations
            var result = base.Fetch(type);

            if (result != null)
                return result;

            var typeArgs = TypeHelpers.GetGenericArguments(type);

            var requiredReturnType = typeArgs[typeArgs.Length - 1];

            var innerTarget = Root.Fetch(requiredReturnType);

            if (innerTarget == null)
                return null;

            // create a func target (new type) which wraps the inner target
            // TODO: if the target is not found, then emit one which wraps a late-bound resolve operation
            // -- unless parameters are required on the delegate type.
            return new AutoFactoryTarget(innerTarget, type, requiredReturnType, typeArgs.Take(typeArgs.Length - 1).ToArray());

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
            if(target.ParameterTypes.Length != 0)
            {
                parameters = target.ParameterTypes.Select((pt, i) => Expression.Parameter(pt, $"p{i}")).ToArray();
                Dictionary<Type, ParameterExpression> lookup = parameters.ToDictionary(pe => pe.Type);
                // we're going to add a compilation filter.  The correct way to do this is to grab any existing compilation filter
                // from the context and then set a new one with the new filters in it, and the original filter as the last entry
                var newFilters = new ExpressionCompilationFilters(
                    target.ParameterTypes.Select(t => 
                        new Func<ITarget, IExpressionCompileContext, IExpressionCompiler, Expression>((ta, ctx, cmp) => {
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

        }
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

    // etc...
}
