using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Rezolver.TypeExtensions;

namespace Rezolver.Tests.Compilation.Specification
{
    public interface IBuilder<T>
    {
        T Build();
    }

    public interface IMemberBindingsBuilder<TInstance> : IBuilder<IMemberBindingBehaviour>
    {
        MemberBindingBuilder<TInstance, TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression);
    }

    public class MemberBindingBuilder<TInstance, TMember> : IBuilder<MemberBinding>, IBuilder<IMemberBindingBehaviour>
    {
        private static readonly BindableCollectionType BindableCollectionType = typeof(TMember).GetBindableCollectionTypeInfo();

        private readonly System.Reflection.MemberInfo _member;
        private readonly MemberBindingsBuilder<TInstance> _parent;
        private Func<MemberBinding> _bindingFactory;

        internal MemberBindingBuilder(MemberInfo member, MemberBindingsBuilder<TInstance> parent)
        {
            _member = member;
            _parent = parent;
        }

        public IMemberBindingsBuilder<TInstance> SetFactory(Func<MemberBinding> factory)
        {
            if (_bindingFactory != null)
                throw new InvalidOperationException("Binding factory has already been set");

            _bindingFactory = factory;
            return _parent;
        }

        public IMemberBindingsBuilder<TInstance> ToType<TTarget>()
            where TTarget : TMember
        {
            return SetFactory(() => new MemberBinding(_member, typeof(TTarget)));
        }

        public IMemberBindingsBuilder<TInstance> ToType(Type type)
        {
            return SetFactory(() => new MemberBinding(_member, type));
        }

        public IMemberBindingsBuilder<TInstance> ToTarget(ITarget target)
        {
            return SetFactory(() => new MemberBinding(_member, target));
        }

        public IMemberBindingsBuilder<TInstance> AsCollection()
        {
            ValidateCollectionType();
            return AsCollectionInternal((Type)null);
        }

        public IMemberBindingsBuilder<TInstance> AsCollection<TElement>()
        {
            ValidateCollectionType();
            return AsCollection(typeof(TElement));
        }

        public IMemberBindingsBuilder<TInstance> AsCollection(Type elementType)
        {
            ValidateCollectionType();
            return AsCollectionInternal(elementType);
        }

        private void ValidateCollectionType()
        {
            if (BindableCollectionType == null)
                throw new InvalidOperationException($"The type { typeof(TMember) } is not a type suitable for collection initialisation.");
        }

        private IMemberBindingsBuilder<TInstance> AsCollectionInternal(Type elementType)
            => AsCollectionInternal(Target.Resolved(typeof(IEnumerable<>).MakeGenericType(elementType ?? BindableCollectionType.ElementType)));

        private IMemberBindingsBuilder<TInstance> AsCollectionInternal(ITarget target)
            => SetFactory(() => new ListMemberBinding(_member, target, BindableCollectionType.ElementType, BindableCollectionType.AddMethod));

        public MemberBindingBuilder<TInstance, TNextMember> Bind<TNextMember>(Expression<Func<TInstance, TNextMember>> memberBindingExpression)
            => _parent.Bind(memberBindingExpression ?? throw new ArgumentNullException(nameof(memberBindingExpression)));

        IMemberBindingBehaviour IBuilder<IMemberBindingBehaviour>.Build() => _parent.Build();

        MemberBinding IBuilder<MemberBinding>.Build() => _bindingFactory?.Invoke() ?? new MemberBinding(_member);
    }

    public class MemberBindingsBuilder<TInstance> : IMemberBindingsBuilder<TInstance>
    {
        private Dictionary<MemberInfo, IBuilder<MemberBinding>> _bindingBuilders;

        public MemberBindingsBuilder()
        {
            _bindingBuilders = new Dictionary<MemberInfo, IBuilder<MemberBinding>>();
        }

        public MemberBindingBuilder<TInstance, TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression)
        {
            var member = MethodCallExtractor.ExtractMemberAccess(memberBindingExpression);
            if (member == null)
                throw new ArgumentException($"The expression {{{ memberBindingExpression }}} must have a member access expression as its body, and it must be a member belonging to the type { typeof(TInstance) }", nameof(memberBindingExpression));
            if (_bindingBuilders.ContainsKey(member))
                throw new ArgumentException($"The member represented by the expression {{{ memberBindingExpression }}} has already been bound", nameof(memberBindingExpression));
            var toReturn = new MemberBindingBuilder<TInstance, TMember>(member, this);
            _bindingBuilders[member] = toReturn;
            return toReturn; ;
        }

        public IMemberBindingBehaviour Build() => new BindSpecificMembersBehaviour(_bindingBuilders.Values.Select(b => b.Build()));
    }
    public static class ExtraRegistrationExtensions
    {
        public static void RegisterType<TInstance>(this ITargetContainer targets, Action<MemberBindingsBuilder<TInstance>> bindingFactory)
        {
            var factory = new MemberBindingsBuilder<TInstance>();
            bindingFactory?.Invoke(factory);
            targets.RegisterType<TInstance>(factory.Build());
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
            targets.RegisterType<HasCollectionMember>(f => 
                f.Bind(hcm => hcm.Numbers)
                .Bind(hcm => hcm.Numbers).AsCollection()
                .Bind(hcm => hcm.Numbers).ToType<System.Collections.ObjectModel.ReadOnlyCollection<int>>());

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasCollectionMember>();

            // Assert
            Assert.Equal(new[] { 10 }, result.Numbers);

        }
    }
}
