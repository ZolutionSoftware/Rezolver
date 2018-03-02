using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Benchmark.Types
{
    public class RequiresLots
    {
        private readonly RequiresSimpleType _rs1;
        private readonly RequiresSimpleType2 _rs2;
        private readonly RequiresSimpleType3 _rs3;
        private readonly SimpleType _s1;
        private readonly SimpleType2 _s2;
        private readonly SimpleType3 _s3;

        public RequiresLots(RequiresSimpleType rs1,
            RequiresSimpleType2 rs2,
            RequiresSimpleType3 rs3,
            SimpleType s1,
            SimpleType2 s2,
            SimpleType3 s3)
        {
            _rs1 = rs1;
            _rs2 = rs2;
            _rs3 = rs3;
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
        }
    }
}
