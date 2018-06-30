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
                using(var innerScope = outerScope.CreateScope())
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
            var baseExpression = compiler.BuildResolveLambda(target.InnerTarget, context.NewContext(target.ReturnType));

            // if there are parameters, we have to replace any Resolve calls in the inner expression with
            // parameter expressions on the inner lambda, and feed them through from the outer lambda.



            var lambda = Expression.Lambda(target.DelegateType, 
                Expression.Convert(Expression.Invoke(baseExpression, context.ResolveContextParameterExpression), target.ReturnType));
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

    // etc...
}
