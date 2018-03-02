using System.Collections.Generic;

namespace Rezolver.Benchmark
{
    public interface IContainerBenchmarks
    {
        string ContainerType { get; }

        IEnumerable<ContainerBenchmarkBase> CreateBenchmarks();
    }
}