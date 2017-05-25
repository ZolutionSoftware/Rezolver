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

        [Fact]
        public void ShouldMatchClosedBaseForContravariantTypeParameter()
        {
            ITargetContainer targets = new TargetContainer();
#error need to adjust TypeHelpers to allow for contravariance (and covariance) in the IsCompatibleWith method.
            //var target = Target.ForType<Contravariant<BaseClass>>();
            //targets.Register()            
        }

        ////testing overload resolution based on interface/base

        //private interface ISuper { }
        //private class Super : ISuper { }

        //private interface ISub { }
        //private class Sub : Super, ISub { }

        //private class Supersub : Sub { }

        //private interface IGenericSuperSub<out T> { }

        //public class GenericSuperSub<T> : IGenericSuperSub<T> { }

        //[Fact]
        //public void Foo()
        //{
        //    // generates CS0121 because interfaces are equal, regardless of whether 
        //    // a type directly implements it or does so via a base.
        //    // Covariance rule: if multiple closed generic registrations apply
        //    //var o = new Sub();
        //    //var s = TestInterfaceOverload(o);

        //    // whereas overloading by base class alone behaves as you would expect: the compiler finds the overload
        //    // whose parameter type is 'closest' to the static type.
        //    var o = new Sub();
        //    Assert.Equal("Sub", TestBaseOverload(o));

        //    var o2 = new Supersub();
        //    Assert.Equal("Sub", TestBaseOverload(o2));

        //    // also generates a CS0121 because bases are overloaded independently of interfaces
        //    // therefore, if two overloads apply to both a base and interface of an argument, the
        //    // compiler cannot determine which is the best.  Ergo: 
        //    var o10 = new Sub();
        //    Assert.Equal("Super", TestBaseAndInterfaceOverload(o10));
        //}

        //private string TestInterfaceOverload(ISuper obj)
        //{
        //    return "ISuper";
        //}
         
        //private string TestInterfaceOverload(ISub obj)
        //{
        //    return "ISub";
        //}

        //private string TestBaseOverload(Super obj)
        //{
        //    return "Super";
        //}

        //private string TestBaseOverload(Sub obj)
        //{
        //    return "Sub";
        //}

        //private string TestBaseAndInterfaceOverload(Super obj)
        //{
        //    return "Super";
        //}

        //private string TestBaseAndInterfaceOverload(ISub obj)
        //{
        //    return "ISub";
        //}
    }
}
