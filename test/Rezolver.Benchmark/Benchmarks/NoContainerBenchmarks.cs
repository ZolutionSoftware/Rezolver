using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rezolver.Benchmark.Types;

namespace Rezolver.Benchmark.Benchmarks
{
    public class NoContainerBenchmarks : ContainerBenchmarks<NoContainer>
    {
        public override string ContainerType => "No-Container";

        public sealed class Simple : UnpreparedBenchmark
        {
            protected override void Run(Stopwatch sw)
            {
                sw.Start();
                var a = new SimpleType();
                sw.Stop();
            }
        }

        public sealed class RequiresSimple : UnpreparedBenchmark
        {
            protected override void Run(Stopwatch sw)
            {
                sw.Start();
                var a = new RequiresSimpleType(new SimpleType());
                sw.Stop();
            }
        }

        public sealed class RequiresLotsAndSingleton : UnpreparedBenchmark
        {
            private Lazy<Singleton> _singleton = new Lazy<Singleton>(() => new Singleton());
            protected override void Run(Stopwatch sw)
            {
                sw.Start();
                var a = new Types.RequiresLotsAndSingleton(new Types.RequiresLots(new RequiresSimpleType(new SimpleType()),
                    new RequiresSimpleType2(new SimpleType2()), new RequiresSimpleType3(new SimpleType3()), new SimpleType(), new SimpleType2(), new SimpleType3()), _singleton.Value);
                sw.Stop();
            }
        }
    }
}
