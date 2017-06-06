using Rezolver.Tests.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainerGenericTests
    {
        [Fact]
        public void ShouldSupportRegisteringOpenGenericAndFetchingAsClosed()
        {
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));
            targets.Register(target, typeof(IGeneric<>));
            ///this should be trivial
            var fetched = targets.Fetch(typeof(IGeneric<>));
            Assert.Same(target, fetched);
            var fetchedClosed = targets.Fetch(typeof(IGeneric<int>));
            Assert.Same(target, fetchedClosed);
        }

        [Fact]
        public void ShouldSupportRegisteringSpecialisationOfGeneric()
        {
            ITargetContainer targets = new TargetContainer();
            targets.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
            var fetched = targets.Fetch(typeof(IGeneric<int>));
            Assert.NotNull(fetched);
            Assert.False(fetched.UseFallback);
        }

        [Fact]
        public void ShouldFavourSpecialisationOfGenericInt()
        {
            ITargetContainer targets = new TargetContainer();
            var notExpected = Target.ForType(typeof(Generic<>));
            var expected = Target.ForType(typeof(AltGeneric<int>));
            targets.Register(notExpected, typeof(IGeneric<>));
            targets.Register(expected, typeof(IGeneric<int>));
            var fetched = targets.Fetch(typeof(IGeneric<int>));
            Assert.NotSame(notExpected, fetched);
            Assert.Same(expected, fetched);
        }

        [Fact]
        public void ShouldSupportRegisteringAndRetrievingGenericWithGenericParameter()
        {
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));
            targets.Register(target, typeof(IGeneric<>));
            var fetched = targets.Fetch(typeof(IGeneric<IGeneric<int>>));
            Assert.Same(target, fetched);
        }

        [Fact]
        public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase()
        {
            //can't think what else to call this scenario!
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(NestedGenericA<>));
            targets.Register(target, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));
            var fetched = targets.Fetch(typeof(IGeneric<IEnumerable<int>>));
            Assert.Same(target, fetched);
        }

        [Fact]
        public void ShouldFavourGenericSpecialisationOfGeneric()
        {
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));

            //note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
            //supply an open generic as a type parameter to a generic is not valid.
            var target2 = Target.ForType(typeof(NestedGenericA<>));

            targets.Register(target, typeof(IGeneric<>));
            targets.Register(target2, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));
            var fetched = targets.Fetch(typeof(IGeneric<IEnumerable<int>>));
            Assert.Same(target2, fetched);
            var fetched2 = targets.Fetch(typeof(IGeneric<int>));
            Assert.Same(target, fetched2);
        }

        public static TheoryData<Type, Type> ContravariantTypeData = new TheoryData<Type, Type>
        {
            // Target Type                                  // Type to Fetch

            // Contravariant Interface
            { typeof(IContravariant<BaseClass>),            typeof(IContravariant<BaseClassChild>) },
            { typeof(IContravariant<BaseClass>),            typeof(IContravariant<BaseClassGrandchild>) },
            // Contravariant Delegate
            { typeof(Action<BaseClass>),                    typeof(Action<BaseClassChild>) },
            { typeof(Action<BaseClass>),                    typeof(Action<BaseClassGrandchild>) },
            // Generic base/interface matching contravariant parameter
            { typeof(IContravariant<IGeneric<string>>),     typeof(IContravariant<Generic<string>>) },
            { typeof(IContravariant<IContravariant<IContravariant<object>>>), typeof(IContravariant<IContravariant<IContravariant<string>>>) }   
        };

        [Theory]
        [MemberData(nameof(ContravariantTypeData))]
        public void ShouldFetchContravariant(Type tTarget, Type toFetch)
        {
            // this theory specifically tests that if we register a target for a generic which
            // has contravariant type parameters, then it will be found automatically.

            // the actual handling of creating an instance is tested in the compiler spec tests
            // covering the ConstructorTarget
            ITargetContainer targets = new TargetContainer();
            var target = new TestTarget(tTarget, false, true, ScopeBehaviour.None);
            targets.Register(target);
            var fetched = targets.Fetch(toFetch);

            Assert.Same(target, fetched);
        }

        [Fact]
        public void ShouldNotFetchConstrainedGenericForIncompatibleType()
        {
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(ConstrainedGeneric<>));
            targets.Register(target, typeof(IGeneric<>));

            // so this should return a fallback target.
            var fetched = targets.Fetch(typeof(IGeneric<string>));
            Assert.True(fetched.UseFallback);      
        }
    }
}
