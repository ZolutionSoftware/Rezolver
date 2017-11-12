using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        //tests in here are based on similar scenarios to the Binding tests
        //for ConstructorTarget in the main test suite - except there are fewer, because
        //there are actually very few unique cases for a compiler to deal with when it comes
        //to executing constructors.

        //Most of the complexity comes from things like RezolvedTargets etc.

        [Fact]
        public void Constructor_ImplicitCtor()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<NoCtor>();
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<NoCtor>();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Constructor_OneParamCtor_WithResolvedArg()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<OneCtor>();
            targets.RegisterObject(OneCtor.ExpectedValue);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<OneCtor>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OneCtor.ExpectedValue, result.Value);
        }

        [Fact]
        public void Constructor_TwoParamCtor_WithResolvedArgs()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<TwoCtors>();
            targets.RegisterObject(10);
            targets.RegisterObject("hello world");
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<TwoCtors>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.I);
            Assert.Equal("hello world", result.S);
        }

        [Fact]
        public void Constructor_ShouldAutoInjectIResolveContext()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<RequiresResolveContext>();

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<RequiresResolveContext>();

            // Assert
            Assert.NotNull(result.Context);

        }

        [Fact]
        public void Constructor_CtorSelectedByNamedArgs()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var target = Target.ForType<TwoCtors>(new { s = Target.ForObject("hello world") });
            targets.Register(target);

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<TwoCtors>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("hello world", result.S);

        }

        [Fact]
        public void Constructor_ShouldAutoBindCollectionMember()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var target = Target.ForType<HasCollectionMember>(MemberBindingBehaviour.BindAll);
            targets.Register(target);
            targets.RegisterObject(10);
            targets.RegisterType<HasCollectionMember>(f => 
                f.AutoBind(o => o.Numbers)
                .Bind(o => o.Numbers).ToType<MyDisposable>()
                .Bind(o => o.Numbers).AsCollection());

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCollectionMember>();

            // Assert
            Assert.Equal(new[] { 10 }, result.Numbers);
        }


    }

    public class BindingFactory<TInstance>
    {
        private ITargetContainer _targets;
        private List<MemberBinding> _bindings;
        private HashSet<Guid> _boundFactories;

        public BindingFactory(ITargetContainer targets)
        {
            _targets = targets;
            _bindings = new List<MemberBinding>();
            _boundFactories = new HashSet<Guid>();
        }

        public IEnumerable<MemberBinding> Bindings { get => _bindings.AsReadOnly(); }

        public class MemberBindingFactory<TMember>
        {
            private readonly Guid _id = Guid.NewGuid();
            private readonly System.Reflection.MemberInfo _member;
            private readonly BindingFactory<TInstance> _parent;

            public MemberBindingFactory(System.Reflection.MemberInfo member, BindingFactory<TInstance> parent)
            {
                _member = member;
                _parent = parent;
            }

            private BindingFactory<TInstance> AddToParent(MemberBinding binding)
            {
                if (_parent._boundFactories.Contains(_id))
                    return _parent;
                _parent._bindings.Add(binding);
                _parent._boundFactories.Add(_id);
                return _parent;
            }

            public static implicit operator MemberBinding(MemberBindingFactory<TMember> factory)
            {
                return new MemberBinding(factory._member);
            }

            public BindingFactory<TInstance> Auto()
            {
                return AddToParent(this);
            }

            public BindingFactory<TInstance> ToType<TTarget>()
            {
                return AddToParent(new MemberBinding(_member, typeof(TTarget)));
            }

            public BindingFactory<TInstance> ToType(Type type)
            {
                return AddToParent(new MemberBinding(_member, type));
            }

            public BindingFactory<TInstance> ToTarget(ITarget target)
            {
                return AddToParent(new MemberBinding(_member, target));
            }

            public BindingFactory<TInstance> AsCollection()
            {
                var collInfo = typeof(TMember).GetBindableCollectionTypeInfo();
                if (collInfo == null)
                    throw new InvalidOperationException($"The type { typeof(TMember) } is not a type suitable for collection initialisation.");
                return AddToParent(
                    new ListMemberBinding(_member, Target.Resolved(typeof(IEnumerable<>).MakeGenericType(collInfo.ElementType)), collInfo.ElementType, collInfo.AddMethod)
                    );
            }
        }

        public MemberBindingFactory<TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression)
        {
            var member = MethodCallExtractor.ExtractMemberAccess(memberBindingExpression);
            if (member == null)
                throw new ArgumentException($"Could not identify a member from the expression {{{ memberBindingExpression }}}", nameof(memberBindingExpression));

            return new MemberBindingFactory<TMember>(member, this);
        }

        public BindingFactory<TInstance> AutoBind<TMember>(Expression<Func<TInstance, TMember>> memberExpression)
        {
            var member = MethodCallExtractor.ExtractMemberAccess(memberExpression);
            if (member == null)
                throw new ArgumentException($"Could not identify a member from the expression {{{ memberExpression }}}", nameof(memberExpression));

            return new MemberBindingFactory<TMember>(member, this).Auto();
        }
    }
    public static class ExtraRegistrationExtensions
    {
        public static void RegisterType<TInstance>(this ITargetContainer targets, Action<BindingFactory<TInstance>> bindingFactory)
        {
            var factory = new BindingFactory<TInstance>(targets);
            bindingFactory?.Invoke(factory);
            var bindings = factory.Bindings;
        }
    }
}
