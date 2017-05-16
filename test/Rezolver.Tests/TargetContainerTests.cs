using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainerTests
    {
        [Fact]
        public void ShouldRegisterNullObjectTarget()
        {
            // <example1>
            ITarget t = new ObjectTarget(null);
            ITargetContainer r = new TargetContainer();
            r.Register(t, serviceType: typeof(object));
            var t2 = r.Fetch(typeof(object));
            Assert.Same(t, t2);
            // </example1>
        }

        [Fact]
        public void ShouldNotRegisterIfTypesDontMatch()
        {
            ITarget t = new ObjectTarget("hello world");
            ITargetContainer r = new TargetContainer();
            Assert.Throws<ArgumentException>(() => r.Register(t, serviceType: typeof(int)));
        }

        [Fact]
        public void ShouldNotAllowNullTypeOnFetch()
        {
            ITargetContainer r = new TargetContainer();
            Assert.Throws<ArgumentNullException>(() => r.Fetch(null));
        }

        [Fact]
        public void ShouldRegisterForImplicitType()
        {
            ITarget t = new ObjectTarget("hello word");
            ITargetContainer rezolverBuilder = new TargetContainer();
            rezolverBuilder.Register(t);
            var t2 = rezolverBuilder.Fetch(typeof(string));
            Assert.Same(t, t2);
        }

        [Fact]
        public void ShouldSupportTwoRegistrations()
        {
            ITargetContainer builder = new TargetContainer();
            var simpleType = new NoCtor();

            ITarget target1 = Target.ForObject("hello world");
            ITarget target2 = Target.ForObject(new NoCtor());
            builder.Register(target1);
            builder.Register(target2);
            Assert.Same(target1, builder.Fetch(typeof(string)));
            Assert.Same(target2, builder.Fetch(typeof(NoCtor)));
        }

        [Fact]
        public void ShouldSupportRegisteringOpenGenericAndFetchingAsClosed()
        {
            ITargetContainer builder = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));
            builder.Register(target, typeof(IGeneric<>));
            ///this should be trivial
            var fetched = builder.Fetch(typeof(IGeneric<>));
            Assert.Same(target, fetched);
            var fetchedClosed = builder.Fetch(typeof(IGeneric<int>));
            Assert.Same(target, fetchedClosed);
        }

        [Fact]
        public void ShouldSupportRegisteringSpecialisationOfGeneric()
        {
            ITargetContainer builder = new TargetContainer();
            builder.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
            var fetched = builder.Fetch(typeof(IGeneric<int>));
            Assert.NotNull(fetched);
            Assert.False(fetched.UseFallback);
        }

        [Fact]
        public void ShouldFavourSpecialisationOfGenericInt()
        {
            ITargetContainer builder = new TargetContainer();
            var notExpected = Target.ForType(typeof(Generic<>));
            var expected = Target.ForType(typeof(AltGeneric<int>));
            builder.Register(notExpected, typeof(IGeneric<>));
            builder.Register(expected, typeof(IGeneric<int>));
            var fetched = builder.Fetch(typeof(IGeneric<int>));
            Assert.NotSame(notExpected, fetched);
            Assert.Same(expected, fetched);
        }

        [Fact]
        public void ShouldSupportRegisteringAndRetrievingGenericWithGenericParameter()
        {
            ITargetContainer builder = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));
            builder.Register(target, typeof(IGeneric<>));
            var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
            Assert.Same(target, fetched);
        }

        [Fact]
        public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase()
        {
            //can't think what else to call this scenario!
            ITargetContainer builder = new TargetContainer();
            var target = Target.ForType(typeof(NestedGenericA<>));
            builder.Register(target, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));
            var fetched = builder.Fetch(typeof(IGeneric<IEnumerable<int>>));
            Assert.Same(target, fetched);
        }

        [Fact]
        public void ShouldFavourGenericSpecialisationOfGeneric()
        {
            ITargetContainer builder = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));

            //note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
            //supply an open generic as a type parameter to a generic is not valid.
            var target2 = Target.ForType(typeof(NestedGenericA<>));

            builder.Register(target, typeof(IGeneric<>));
            builder.Register(target2, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));
            var fetched = builder.Fetch(typeof(IGeneric<IEnumerable<int>>));
            Assert.Same(target2, fetched);
            var fetched2 = builder.Fetch(typeof(IGeneric<int>));
            Assert.Same(target, fetched2);
        }

        //[Fact]
        //public void ShouldSupportWideningGenericWhenConstraintsAreSpecific()
        //{
        //	//usually, if we have a target type with two parameters, then we can't use it 
        //	//when it's requested via a generic type with fewer parameters.
        //	//however, if there are type constraints involved, then we might be able to.
        //	Assert.False(true);
        //}

        public interface IMultipleRegistration
        {

        }

        public class MultipleRegistration1 : IMultipleRegistration
        {
        }

        public class MultipleRegistration2 : IMultipleRegistration
        {

        }

        [Fact]
        public void ShouldSupportRegisteringMultipleTargetsOfTheSameType()
        {
            ITargetContainer builder = new TargetContainer();
            builder.RegisterMultiple(new[] { Target.ForType<MultipleRegistration1>(), Target.ForType<MultipleRegistration1>() }, typeof(IMultipleRegistration));

            var fetched = builder.Fetch(typeof(IEnumerable<IMultipleRegistration>));
            Assert.NotNull(fetched);
            Assert.False(fetched.UseFallback);
        }

        [Fact]
        public void ShouldSupportRegisteringMultipleTargetsOfDifferentTypeWithCommonInterface()
        {
            ITargetContainer targets = new TargetContainer();

            targets.RegisterMultiple(new[] { Target.ForType<MultipleRegistration1>(), Target.ForType<MultipleRegistration2>() }, typeof(IMultipleRegistration));

            var fetched = targets.Fetch(typeof(IEnumerable<IMultipleRegistration>));
            Assert.NotNull(fetched);
            Assert.False(fetched.UseFallback);
        }

        [Fact]
        public void ShouldReturnFallbackTargetForUnregisteredIEnumerable()
        {
            ITargetContainer targets = new TargetContainer();
            var result = targets.Fetch(typeof(IEnumerable<int>));
            Assert.NotNull(result);
            Assert.True(result.UseFallback);
        }

        /// <summary>
        /// Represents an intent to create an ITargetContainer for a specific purpose.
        /// 
        /// The derived type determines the type of target container required.
        /// </summary>
        private class TargetContainerOptions
        {
            public ITargetContainer Owner { get; set; }

            public virtual void Validate()
            {
                
            }
        }

        /// <summary>
        /// Options for a target container 
        /// </summary>
        private class ChildTargetContainerOptions : TargetContainerOptions
        {
            public Type Type { get; set; }

            public override void Validate()
            {
                if (Type == null) throw new InvalidOperationException("Type must not be null");
                if (Owner == null) throw new InvalidOperationException("Owner must not be null");
            }
        }

        /// <summary>
        /// Indicates an intent to create an ITargetContainer that can store a list of targets
        /// by the same type.
        /// </summary>
        private class TargetListOptions : ChildTargetContainerOptions
        {
            public bool DisableMultiple { get; set; } = false;
        }

        /// <summary>
        /// Indicates an intent to create an ITargetContainer that can store multiple lists of targets
        /// of non-related types.  This is typically the root of most target container 'trees'
        /// </summary>
        private class TargetDictionaryOptions : TargetContainerOptions { }

        /// <summary>
        /// Indicates an intent to create an ITargetContainer that is bound to a specific open generic type
        /// </summary>
        private class GenericTargetContainerOptions : ChildTargetContainerOptions
        {
            /// <summary>
            /// Enables matching interface or delegate registrations when a matching type parameter is covariant and
            /// a registration for a closed generic type uses a type argument for the same parameter which is derived 
            /// from, or which implements the type passed.
            /// </summary>
            public bool EnableCovariance { get; set; } = false;
            public bool EnableContravariance { get; set; } = false;
            /// <summary>
            /// If true, then calling <see cref="ITargetContainer.FetchAll(Type)"/> with a method should return
            /// all targets registered aainst 
            /// </summary>
            public bool AlwaysFetchAllMatchingTargets { get; set; } = true;

            public override void Validate()
            {
                base.Validate();
                Type t;
                t.GenericTypeArguments[0].
                if (!TypeHelpers.IsGenericTypeDefinition(Type)) throw new InvalidOperationException("Type must be an open generic");
            }
        }

        //TODO: should be able to resolve an Action<Base> when requesting Action<Derived> (contravariance)
        // TODO: should be able to resolve a Func<Derived> when requesting Func<Base> (covariance)

        //The code which 
	}
}
