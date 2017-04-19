using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Types
{
    public class DecoratorForAny2<T> : IDecorated<T>
    {
        public IDecorated<T> Decorated { get; }

        public DecoratorForAny2(IDecorated<T> decorated)
        {
            Assert.NotNull(decorated);
            Decorated = decorated;
        }
    }
}
