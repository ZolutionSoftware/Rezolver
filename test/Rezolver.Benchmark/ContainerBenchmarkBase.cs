using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Benchmark
{
    public abstract class ContainerBenchmarkBase
    { 
        /// <summary>
        /// This is used to group together benchmarks for different container types
        /// which compare performance for the same operation.  One container might have
        /// multiple banchmarks for the same name.
        /// </summary>
        public abstract string BenchmarkName { get; }
        /// <summary>
        /// An extra string for this benchmark which can be used to discrimate between 
        /// two identically named benchmarks for the same container type.  For example,
        /// if benchmarking two different ways of doing the same thing.
        /// </summary>
        public virtual string ExtendedName { get; }

        public string DisplayName => ExtendedName != null ? $"{ BenchmarkName } - { ExtendedName }" : BenchmarkName;

        public abstract string ContainerType { get; }
        protected abstract void Run(Stopwatch sw);

        protected virtual void Prepare()
        {
            GC.Collect();
        }

        // TODO: make a RunAsync() method
        public BenchmarkResult Run(int count)
        {
            Prepare();
            Stopwatch sw = new Stopwatch();
            int current = 0;
            try
            {
                for (current = 0; current < count; current++)
                {
                    Run(sw);
                }
                // make sure the stopwatch is stopped
                sw.Stop();
            }
            catch(Exception ex)
            {
                // make sure the stopwatch is stopped
                sw.Stop();
                return new BenchmarkResult()
                {
                    Source = this,
                    Success = false,
                    Error = ex,
                    Count = current,
                    RunTime = sw.Elapsed
                };
            }
            finally
            {
                Cleanup();
            }

            return new BenchmarkResult()
            {
                Source = this,
                Success = true,
                Count = count,
                RunTime = sw.Elapsed
            };
        }

        public Task<BenchmarkResult> RunAsync(int count, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => { 
                Prepare();
                Stopwatch sw = new Stopwatch();
                int current = 0;
                try
                {
                    for (current = 0; current < count; current++)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            Run(sw);
                        else
                            break;
                    }
                    // make sure the stopwatch is stopped
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    // make sure the stopwatch is stopped
                    sw.Stop();
                    return new BenchmarkResult()
                    {
                        Source = this,
                        Success = false,
                        Error = ex,
                        Count = current,
                        RunTime = sw.Elapsed
                    };
                }
                finally
                {
                    Cleanup();
                }

                return new BenchmarkResult()
                {
                    Source = this,
                    Success = true,
                    Count = current,
                    RunTime = sw.Elapsed
                };
            });
        }

        protected virtual void Cleanup()
        {
            GC.Collect();
        }
    }
}
