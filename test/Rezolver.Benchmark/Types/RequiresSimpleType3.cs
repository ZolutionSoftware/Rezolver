using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark.Types
{
    public class RequiresSimpleType3
    {
        readonly SimpleType3 _simple;
        public RequiresSimpleType3(SimpleType3 simple)
        {
            _simple = simple;
        }
    }
}
