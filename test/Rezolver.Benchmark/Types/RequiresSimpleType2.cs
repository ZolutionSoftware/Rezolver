using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark.Types
{
    public class RequiresSimpleType2
    {
        readonly SimpleType2 _simple;
        public RequiresSimpleType2(SimpleType2 simple)
        {
            _simple = simple;
        }
    }
}
