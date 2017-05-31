using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Targets
{
    /// <summary>
    /// testing elements of the TargetBase class as it's used by so many of the standard targets
    /// </summary>
    public class TargetBaseTests
    {
        private class TestTarget : TargetBase
        {
            public override Type DeclaredType { get; }

            public TestTarget(Type type)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                if (type.IsGenericTypeDefinition || type.ContainsGenericParameters)
                    throw new ArgumentException("TargetBase is not expected to be able to handle generic type definitions or open generic types", nameof(type));
                DeclaredType = type;
            }
        }

        public static TheoryData<Type, Type> SupportsTypeData = new TheoryData<Type, Type>()
        {
            // Target Type                                      Supports Type
            { typeof(string),                                   typeof(string) },
            { typeof(string),                                   typeof(object) },
            { typeof(string),                                   typeof(IEnumerable<char>) },
            { typeof(BaseClassChild),                           typeof(BaseClass) },
            { typeof(BaseClassGrandchild),                      typeof(BaseClass) },
            { typeof(BaseClassGrandchild[]),                    typeof(BaseClassChild[]) },
        };

        [Theory]
        [MemberData(nameof(SupportsTypeData))]
        public void ShouldSupportType(Type targetType, Type shouldBeSupported)
        {
            var target = new TestTarget(targetType);
            Assert.True(target.SupportsType(shouldBeSupported));
        }

        public static TheoryData<Type, Type> SupportsVariantTypeData = new TheoryData<Type, Type>()
        {
            // Covariance
            { typeof(Func<BaseClassChild>),                     typeof(Func<BaseClass>) },
            { typeof(Covariant<string>),                        typeof(ICovariant<IEnumerable<char>>) },
            { typeof(Covariant<string>),                        typeof(ICovariant<object>) },
            // Contravariance
            { typeof(Action<BaseClass>),                        typeof(Action<BaseClassChild>) },
            { typeof(Contravariant<BaseClass>),                 typeof(IContravariant<BaseClassChild>) },
            // Variance combinations
            // ---------------------
            // When combining a contravariant type param as an argument to a covariant
            // type param - the normal contravariance rules apply
            { typeof(Action<Func<BaseClass>>),                  typeof(Action<Func<BaseClassChild>>) },
            { typeof(Func<Action<BaseClass>>),                  typeof(Func<Action<BaseClassChild>>) },
            // class<class<type>> -> iface<iface<type>> works because class->interface is 'smaller' assignment
            { typeof(Covariant<Covariant<string>>),             typeof(ICovariant<ICovariant<object>>) },
            { typeof(Covariant<Covariant<string>>),             typeof(ICovariant<object>) },
            // outer type -> interface works purely because of standard assignment equality
            // first generic arguments must be of the same generic type (i.e. IContravariant<> because
            // that type parameter is itself contravariant, which would only allow bases or interfaces
            { typeof(Contravariant<IContravariant<string>>),    typeof(IContravariant<IContravariant<object>>) },
            { typeof(Contravariant<object>),                    typeof(IContravariant<IContravariant<object>>) }
        };

        [Theory]
        [MemberData(nameof(SupportsVariantTypeData))]
        public void ShouldSupportVariantType(Type tTarget, Type tSupports)
        {
            var target = new TestTarget(tTarget);
            Assert.True(target.SupportsType(tSupports));
        }

        public void FunWithVariance()
        {
            // allows nested class type because covariance allows derived/implementing types
            ICovariant<ICovariant<object>> cco = new Covariant<Covariant<string>>();
            // so, the inner type argument obeys contravariance rules (allowing base types or interfaces)
            // whilst the outer type obeys standard covariance - allowing more derived types.
            ICovariant<IContravariant<string>> cocco = new Covariant<Contravariant<IEnumerable<char>>>();
            // requires same IContravariant middle interface, but allows derived/implementing
            // types as innermost generic argument - because contravariance requires equal types
            // or bases of types.
            IContravariant<IContravariant<object>> coco = new Contravariant<IContravariant<string>>();
            IContravariant<IContravariant<object>> coco3 = new Contravariant<object>();
            // starting position:
            // if a class type is passed to a contravariant type parameter, then any of its
            // bases are also possible matches.
            // conversely, any derived types are possible matches for a 

            // function accepting a derived type can be assigned to a function
            // accepting its base
            Action<BaseClass> f1 = null;
            Action<BaseClassChild> u1 = null;
            u1 = f1;

            // this is equivalent to straight contravariance
            Func<Action<BaseClass>> f2 = null;
            Func<Action<BaseClassChild>> u2 = null;
            u2 = f2;

            // oddly enough - so is this
            Action<Func<BaseClass>> f3 = null;
            Action<Func<BaseClassChild>> u3 = null;
            u3 = f3;

            // revised rule - contravariant searches (i.e. for bases) are not affected by the
            // presence of covariant type parameters.


            // what about this
            Func<Action<Func<object>>> f4 = null;
            Func<Action<Func<string>>> u4 = null;
            u4 = f4;

            Action<Func<Action<BaseClass>>> f4a = null;
            Action<Func<Action<BaseClassChild>>> u4a = null;

            f4a = u4a;

            // contravariant type parameters effectively beat any contravariant parameters - so
            // if there's a contravariant type parameter in amongst a bunch of covariant parameters,
            // the type search will be towards the base classes.

            Func<Func<object>> f5a = null;
            Func<Func<string>> u5a = null;

            f5a = u5a;

            // covariant type parameters also don't interfere with themselves.

            // in this example, two nested contravariant types effectively reverse the inner
            // non-generic relationship - so a method accepting a delegate to a base type
            // can be assigned 
            Action<Action<BaseClass>> f5 = null;
            Action<Action<BaseClassChild>> u6 = null;
            f5 = u6;

            Action<Action<Action<BaseClass>>> f7 = null;
            Action<Action<Action<BaseClassChild>>> u8 = null;
            u8 = f7;

            Action<Action<Func<BaseClass>>> f9 = null;
            Action<Action<Func<BaseClassChild>>> u9 = null;
            f9 = u9;

            // variance is only considered when all type parameters which apply to a type 
            // argument are also variant.  So Generic<Func<string>> can only by assigned to
            // an instance of that type - even though Func<string> is variant - it's being passed
            // as an argument to a type parameter that is not.
        }
    }
}
