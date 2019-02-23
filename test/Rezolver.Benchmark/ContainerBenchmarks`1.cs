using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver.Benchmark
{
    /// <summary>
    /// Owns one or more benchmarks for a particular type of container
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    public abstract class ContainerBenchmarks<TContainer> : IContainerBenchmarks
    {
        public abstract string ContainerType { get; }

        public abstract class BenchmarkBase : ContainerBenchmarkBase
        {
            internal ContainerBenchmarks<TContainer> _owner;
            
            public sealed override string ContainerType => _owner?.ContainerType ?? "INVALID";

            /// <summary>
            /// Name of the benchmark - defaults to derived type name.
            /// </summary>
            public override string BenchmarkName => GetType().Name;

            internal BenchmarkBase()
            {

            }
        }

        // benchmark in which the container is created and configured as part of the benchmark.
        // the simplest of benches.  No additional preparation is performed.
        public abstract class UnpreparedBenchmark : BenchmarkBase
        {

        }

        /// <summary>
        /// These benchmarks (the most common for 'real' containers) split the preparation of the
        /// container - creation, registration of services - out into a separate step, with
        /// the prepared container then being passed to the benchmark method implementation
        /// </summary>
        public abstract class PreparedBenchmark : BenchmarkBase
        {
            protected abstract TContainer CreateAndPrepareContainer();
            protected abstract void Run(TContainer container, Stopwatch sw);

            private TContainer _preparedContainer;
            protected override void Prepare()
            {
                base.Prepare();
                _preparedContainer = CreateAndPrepareContainer();
                if (_preparedContainer == null)
                    throw new InvalidOperationException("Container was not created and prepared");
            }

            protected sealed override void Run(Stopwatch sw)
            {
                Run(_preparedContainer, sw);
            }
        }

        public IEnumerable<ContainerBenchmarkBase> CreateBenchmarks()
        {
            var benchTypes = this.GetType()
                .GetNestedTypes()
                .Where(t => !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null && typeof(BenchmarkBase).IsAssignableFrom(t));

            return benchTypes.Select(CreateBenchmarkInstance);
        }

        private BenchmarkBase CreateBenchmarkInstance(Type type)
        {
            var result = (BenchmarkBase)Activator.CreateInstance(type);
            result._owner = this;
            return result;
        }
    }
}
