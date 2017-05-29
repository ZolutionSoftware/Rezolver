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
            { typeof(string), typeof(string) },
            { typeof(string), typeof(object) },
            { typeof(string), typeof(IEnumerable<char>) },
            { typeof(BaseClassChild), typeof(BaseClass) },
            { typeof(BaseClassGrandchild), typeof(BaseClass) },
            { typeof(BaseClassGrandchild[]), typeof(BaseClassChild[]) },
            //Covariance
            { typeof(Func<BaseClassChild>), typeof(Func<BaseClass>) },
            //Contravariance
            { typeof(Action<BaseClass>), typeof(Action<BaseClassChild>) },
            //Variance combinations
            { typeof(Action<Func<BaseClassChild>>), typeof(Action<Func<BaseClass>>) },
            { typeof(Func<Action<BaseClassChild>>), typeof(Func<Action<BaseClass>>) }
        };

        [Theory]
        [MemberData("SupportsTypeData")]
        public void ShouldSupportType(Type targetType, Type shouldBeSupported)
        {
            var target = new TestTarget(targetType);
            Assert.True(target.SupportsType(shouldBeSupported));
        }

        [Fact]
        public void FunWithVariance()
        {
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
