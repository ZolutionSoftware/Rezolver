using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public partial class TargetContainerTests
    {
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

            // Arrange
            ITargetContainer targets = new TargetContainer();
            var target = new TestTarget(tTarget, false, true, ScopeBehaviour.None);
            targets.Register(target);

            // Act
            var fetched = targets.Fetch(toFetch);

            // Assert
            Assert.Same(target, fetched);
        }
    }
}
