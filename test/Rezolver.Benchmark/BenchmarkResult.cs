using System;

namespace Rezolver.Benchmark
{
    public class BenchmarkResult
    {
        public bool Success { get; internal set; }
        public Exception Error { get; internal set; }
        public TimeSpan RunTime { get; internal set; }
        public int Count { get; internal set; }
        internal ContainerBenchmarkBase Source { get; set; }

    }
}
