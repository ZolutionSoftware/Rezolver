using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Types
{
    public class DecoratorForAny<T> : IDecorated<T>
    {
        public IDecorated<T> Decorated { get; }

        public DecoratorForAny(IDecorated<T> decorated)
        {
            Assert.NotNull(decorated);
            Decorated = decorated;
        }
    }
}
