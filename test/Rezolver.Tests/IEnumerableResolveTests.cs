using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class IEnumerableResolveTests
    {
        public class MissingService
        {

        }

        [Fact]
        public void ShouldResolveEmptyEnumerableOfMissingService()
        {
            DefaultRezolver rezolver = new DefaultRezolver();
            var result = rezolver.Resolve<IEnumerable<MissingService>>();
        }
    }
}
