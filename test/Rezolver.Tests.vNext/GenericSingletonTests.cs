using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext
{
    public class GenericTypeCheckTests
    {
        public class TestGenericType<TArg1>
        {

        }

        [Fact]
        public void SingletonShouldCreateInstanceForEachUniqueType()
        {
            //a bug that occurred when interfacing with Asp.Net MVC6 - they use a singleton Generic for IOptions`1, and the code was 
            //only ever creating the one instance of any generic, so 2nd and subsequent calls would get the instance that was first
            //created, regardless of whether the generic types were the same!
            var target = GenericConstructorTarget.Auto(typeof(TestGenericType<>)).Singleton();
            var r = new DefaultRezolver();
            r.Register(target);

            var t = r.Resolve(typeof(TestGenericType<string>));
            //Causes InvalidCastException in unfixed code.
            var t2 = r.Resolve(typeof(TestGenericType<int>));
        }
    }
}
