using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark.Types
{
    public class RequiresSimpleType
    {
        readonly SimpleType _simple;
        public RequiresSimpleType(SimpleType simple)
        {
            _simple = simple;
        }
    }
}
