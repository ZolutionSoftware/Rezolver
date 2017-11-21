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
    public interface IMemberBindingsBuilder<TInstance>
    {
        MemberBindingBuilder<TInstance, TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression);
        IMemberBindingBehaviour Build();
    }

    internal interface IMemberBindingBuilder
    {
        MemberInfo Member { get; }
        MemberBinding Build();
    }

    public class MemberBindingBuilder<TInstance, TMember> : IMemberBindingBuilder
    {
        private static readonly BindableCollectionType CollectionTypeInfo = typeof(TMember).GetBindableCollectionTypeInfo();
        private Func<MemberBinding> BindingFactory;

        public MemberInfo Member { get; private set; }
        public IMemberBindingsBuilder<TInstance> Parent { get; private set; }

        internal MemberBindingBuilder(MemberInfo member, IMemberBindingsBuilder<TInstance> parent)
        {
            Parent = parent;
            Member = member;
        }

        internal IMemberBindingsBuilder<TInstance> SetFactory(Func<MemberBinding> factory)
        {
            if (BindingFactory != null)
                throw new InvalidOperationException("Binding factory has already been set");

            BindingFactory = factory;
            return Parent;
        }

        MemberBinding IMemberBindingBuilder.Build()
        {
            return BindingFactory?.Invoke() ?? new MemberBinding(Member);
        }

        public IMemberBindingsBuilder<TInstance> ToType<TTarget>()
            where TTarget : TMember
        {
            return SetFactory(() => new MemberBinding(Member, typeof(TTarget)));
        }

        public IMemberBindingsBuilder<TInstance> ToType(Type type)
        {
            return SetFactory(() => new MemberBinding(Member, type));
        }

        public IMemberBindingsBuilder<TInstance> ToTarget(ITarget target)
        {
            return SetFactory(() => new MemberBinding(Member, target));
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
            if (CollectionTypeInfo == null)
                throw new InvalidOperationException($"The type { typeof(TMember) } is not a type suitable for collection initialisation.");
        }

        private IMemberBindingsBuilder<TInstance> AsCollectionInternal(Type elementType)
            => AsCollectionInternal(Target.Resolved(typeof(IEnumerable<>).MakeGenericType(elementType ?? CollectionTypeInfo.ElementType)));

        private IMemberBindingsBuilder<TInstance> AsCollectionInternal(ITarget target)
            => SetFactory(() => new ListMemberBinding(Member, target, CollectionTypeInfo.ElementType, CollectionTypeInfo.AddMethod));

        public MemberBindingBuilder<TInstance, TNextMember> Bind<TNextMember>(Expression<Func<TInstance, TNextMember>> memberBindingExpression)
            => Parent.Bind(memberBindingExpression ?? throw new ArgumentNullException(nameof(memberBindingExpression)));
    }

    public class MemberBindingsBuilder<TInstance> : IMemberBindingsBuilder<TInstance>
    {
        private readonly Dictionary<MemberInfo, IMemberBindingBuilder> _bindingBuilders = new Dictionary<MemberInfo, IMemberBindingBuilder>();

        private void AddBinding(IMemberBindingBuilder builder)
        {
            if (_bindingBuilders.ContainsKey(builder.Member))
                throw new ArgumentException($"Member { builder.Member.Name } has already been bound", nameof(builder));
            _bindingBuilders[builder.Member] = builder;
        }

        public IMemberBindingBehaviour Build() => new BindSpecificMembersBehaviour(_bindingBuilders.Values.Select(b => b.Build()));

        public MemberBindingBuilder<TInstance, TMember> Bind<TMember>(Expression<Func<TInstance, TMember>> memberBindingExpression)
        {
            var member = MethodCallExtractor.ExtractMemberAccess(memberBindingExpression);
            if (member == null)
                throw new ArgumentException($"The expression {{{ memberBindingExpression }}} must have a member access expression as its body, and it must be a member belonging to the type { typeof(TInstance) }", nameof(memberBindingExpression));

            var toAdd = new MemberBindingBuilder<TInstance, TMember>(member, this);

            try
            {
                AddBinding(toAdd);
            }
            catch(ArgumentException aex)
            {
                throw new ArgumentException($"The member '{ member }', represented by the expression {{{ memberBindingExpression }}} has already been bound", nameof(memberBindingExpression), aex);
            }

            return toAdd;
        }
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
        public void Members_ShouldBindAll()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterObject(10);             //single int property
            targets.RegisterObject("hello world");  //single string property
            targets.RegisterObject(30m);            //List decimal property
            targets.RegisterObject(40m);
            DateTime now = DateTime.UtcNow;         //Bind to collection of datetimes
            targets.RegisterObject(now);            
            targets.RegisterObject(now.AddDays(1));
            targets.RegisterType<HasMembers>(MemberBindingBehaviour.BindAll);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasMembers>();

            // Assert
            Assert.Equal(10, result.BindableInt);
            Assert.Equal("hello world", result.BindableString);
            // never bound because it's read only enumerable
            Assert.Equal(new[] { 1.0, 2.0, 3.0 }, result.ReadOnlyDoubles);
            Assert.Equal(new[] { 30m, 40m }, result.BindableListOfDecimals);
            Assert.Equal(new[] { now, now.AddDays(1) }, result.CollectionBindableListOfDateTimes);

            // this is failing because it's binding to the IEnumerable<Decimal>, int constructor of List<T>
            // see #67
        }

        [Fact]
        public void Members_ExplicitBinding_ShouldBindNoneByDefault()
        {
            // Arrange
            var targets = CreateTargetContainer();
            // note - by forcibly calling this overload, we'll trigger a BindSpecificMembersBehaviour to be
            // created with no member bindings
            targets.RegisterType((Action<MemberBindingsBuilder<HasMembers>>)null);

            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasMembers>();

            // Assert
            Assert.Equal(0, result.BindableInt);
            Assert.Null(result.BindableString);
            // never bound because it's read only enumerable
            Assert.Equal(new[] { 1.0, 2.0, 3.0 }, result.ReadOnlyDoubles);
            Assert.Null(result.BindableListOfDecimals);
            Assert.Equal(new DateTime[0], result.CollectionBindableListOfDateTimes);
        }


        [Fact]
        public void Members_ShouldExplicitlyBindCollectionMember()
        {
            // Arrange
            // This time, going to disable List injection and force the decimals into 
            // being resolved as enumerables so the collection binder can do its thang.
            var config = TargetContainer.DefaultConfig.Clone();
            config.ConfigureOption<Options.EnableCollectionInjection>(false);
            var targets = CreateTargetContainer(configOverride: config);

            targets.RegisterObject(10m);
            targets.RegisterObject(20m);

            targets.RegisterType<HasMembers>(b => b.Bind(hm => hm.BindableListOfDecimals).AsCollection());
            
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<HasMembers>();

            // Assert
            Assert.Equal(new[] { 10m, 20m }, result.BindableListOfDecimals);

        }
    }
}
