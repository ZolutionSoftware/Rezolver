using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Rezolver.TypeExtensions;

namespace Rezolver.Tests.Compilation.Specification
{
    public class MemberBindingsBuilder<TInstance> : IMemberBindingsBuilder<TInstance>
    {
        private ITargetContainer _targets;
        private List<MemberBinding> _bindings;
        private HashSet<Guid> _boundFactories;

        public MemberBindingsBuilder(ITargetContainer targets)
        {
            _targets = targets;
            _bindings = new List<MemberBinding>();
            _boundFactories = new HashSet<Guid>();
        }

        public IEnumerable<MemberBinding> Bindings { get => _bindings.AsReadOnly(); }

        public IMemberBindingBehaviour CreateBehaviour()
        {
            return new BindSpecificMembersBehaviour(_bindings);
        }

        public class MemberBindingBuilder<TMember> : IMemberBindingsBuilder<TInstance>
        {
            private static readonly BindableCollectionType BindableCollectionType = typeof(TMember).GetBindableCollectionTypeInfo();

            private readonly Guid _id = Guid.NewGuid();
            private readonly System.Reflection.MemberInfo _member;
            private readonly MemberBindingsBuilder<TInstance> _parent;

            internal MemberBindingBuilder(System.Reflection.MemberInfo member, MemberBindingsBuilder<TInstance> parent)
            {
                _member = member;
                _parent = parent;
            }

            private MemberBindingsBuilder<TInstance> AddToParent(MemberBinding binding)
            {
                // TODO: Change this so the binding is always written - either to a new slot,
                // or overwriting the last-registered binding for the same member.  Then we
                // can insta-add bindings as soon as Bind() is called, removing the need for the Auto() 
                // method.
                if (_parent._boundFactories.Contains(_id))
                    throw new InvalidOperationException("Cannot bind the same member twice");

                _parent._bindings.Add(binding);
                _parent._boundFactories.Add(_id);
                return _parent;
            }

            public static implicit operator MemberBinding(MemberBindingBuilder<TMember> factory)
            {
                return new MemberBinding(factory._member);
            }

            public MemberBindingsBuilder<TInstance> Auto()
            {
                return AddToParent(this);
            }

            public MemberBindingsBuilder<TInstance> ToType<TTarget>()
                where TTarget : TInstance
            {
                return AddToParent(new MemberBinding(_member, typeof(TTarget)));
            }

            public MemberBindingsBuilder<TInstance> ToType(Type type)
            {
                return AddToParent(new MemberBinding(_member, type));
            }

            public MemberBindingsBuilder<TInstance> ToTarget(ITarget target)
            {
                return AddToParent(new MemberBinding(_member, target));
            }

            public MemberBindingsBuilder<TInstance> AsCollection()
            {
                ValidateCollectionType();
                return AsCollectionInternal((Type)null);
            }

            public MemberBindingsBuilder<TInstance> AsCollection<TElement>()
            {
                ValidateCollectionType();
                return AsCollection(typeof(TElement));
            }

            public MemberBindingsBuilder<TInstance> AsCollection(Type elementType)
            {
                ValidateCollectionType();
                return AsCollectionInternal(elementType);
            }

            private void ValidateCollectionType()
            {
                if (BindableCollectionType == null)
                    throw new InvalidOperationException($"The type { typeof(TMember) } is not a type suitable for collection initialisation.");
            }

            private MemberBindingsBuilder<TInstance> AsCollectionInternal(Type elementType)
            {
                return AsCollectionInternal(Target.Resolved(typeof(IEnumerable<>).MakeGenericType(elementType ?? BindableCollectionType.ElementType)));
            }

            private MemberBindingsBuilder<TInstance> AsCollectionInternal(ITarget target)
            {
                return AddToParent(new ListMemberBinding(_member, target, BindableCollectionType.ElementType, BindableCollectionType.AddMethod));
            }

            public MemberBindingBuilder<TMember1> Bind<TMember1>(Expression<Func<TInstance, TMember1>> memberBindingExpression)
            {
                return AddToParent(this).Bind(memberBindingExpression);
            }

            IMemberBindingBehaviour IMemberBindingsBuilder<TInstance>.CreateBehaviour()
            {
                return AddToParent(this).CreateBehaviour();
            }
        }

        public MemberBindingBuilder<TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression)
        {
            var member = MethodCallExtractor.ExtractMemberAccess(memberBindingExpression);
            if (member == null)
                throw new ArgumentException($"The expression {{{ memberBindingExpression }}} must have a member access expression as its body, and it must be a member belonging to the type { typeof(TInstance) }", nameof(memberBindingExpression));

            return new MemberBindingBuilder<TMember>(member, this);
        }
    }
    public static class ExtraRegistrationExtensions
    {
        public static void RegisterType<TInstance>(this ITargetContainer targets, Action<MemberBindingsBuilder<TInstance>> bindingFactory)
        {
            var factory = new MemberBindingsBuilder<TInstance>(targets);
            bindingFactory?.Invoke(factory);
            targets.RegisterType<TInstance>(factory.CreateBehaviour());
        }
    }

    public partial class CompilerTestsBase
    {
        [Fact]
        public void Constructor_ShouldAutoBindCollectionMember()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(10);
            targets.RegisterType<HasCollectionMember>(MemberBindingBehaviour.BindAll);

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCollectionMember>();

            // Assert
            Assert.Equal(new[] { 10 }, result.Numbers);
        }


        [Fact]
        public void Constructor_ShouldExplicitlyBindCollectionMember()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(10);
            targets.RegisterType<HasCollectionMember>(f => f.Bind(hcm => hcm.Numbers));

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCollectionMember>();

            // Assert
            Assert.Equal(new[] { 10 }, result.Numbers);

        }
    }
}
