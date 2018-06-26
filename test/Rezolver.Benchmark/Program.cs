using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using McMaster.Extensions.CommandLineUtils;
using Rezolver.Benchmark.Benchmarks;

namespace Rezolver.Benchmark
{
    class Program
    {
        static int BenchDotNetMain(string[] args)
        {
            var summary = BenchmarkRunner.Run<CreationBenches>();
            return 0;
        }

        const int DEFAULT_COUNT = 50000;
        const int DEFAULT_TIMEOUT = 10;
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption();
            app.ThrowOnUnexpectedArgument = false;
            var fullBench = app.Option("-f|--full", "If specified, then a full benchmark will be run using Benchmark.Net", CommandOptionType.NoValue);
            var listArg = app.Option("-l|--list", "Lists the available benchmarks and containers which run them", CommandOptionType.NoValue);
            var optCount = app.Option("-c|--count <INT>", $"Number of loops each benchmark should run (default={DEFAULT_COUNT})", CommandOptionType.SingleValue);
            var optTimeOut = app.Option("-t|--timeout <INT>", $"Maximum number seconds a benchmark is allowed to run for (default={DEFAULT_TIMEOUT}", CommandOptionType.SingleValue);
            var optInclude = app.Option("-i|--include <INCLUDE>", "Specific benchmarks to include.  Cannot be used with -x", CommandOptionType.MultipleValue);
            var optExclude = app.Option("-x|--exclude <EXCLUDE>", "Specific benchmarks to exclude.  Cannot be used with -i", CommandOptionType.MultipleValue);
            var optExcludeExtended = app.Option("-xx|--excludeextended", "If provided, extended benchmarks will not be included", CommandOptionType.NoValue);
            var optIncludeCont = app.Option("-iC|--includeCont <INCLUDE>", "Specific container types to include.  Cannot be used with -xC", CommandOptionType.MultipleValue);
            var optExcludeCont = app.Option("-xC|--excludeCont <EXCLUDE>", "Specific container types to exclude.  Cannot be used with -iC", CommandOptionType.MultipleValue);

            var appExe = new Func<Task<int>>(async () =>
            {
                if (fullBench.HasValue())
                    return BenchDotNetMain(app.RemainingArguments.ToArray());

                var benches = LoadBenchmarks();

                if (listArg.HasValue())
                {
                    ListBenchmarks(benches);
                    return -1;
                }

                int count = DEFAULT_COUNT;
                int timeOut = DEFAULT_TIMEOUT;

                if ((optCount.HasValue() && !int.TryParse(optCount.Value(), out count))
                || (optTimeOut.HasValue() && !int.TryParse(optTimeOut.Value(), out timeOut)))
                {
                    app.ShowHelp();
                    return -1;
                }

                benches = FilterBenchmarks(benches, optInclude, optExclude);

                if (benches == null)
                {
                    app.ShowHelp();
                    return -1;
                }

                Func<Benchmark, IEnumerable<ContainerBenchmarkBase>> typeFilter;

                if (optIncludeCont.HasValue())
                {
                    if (optExcludeCont.HasValue())
                    {
                        app.ShowHelp();
                        return -1;
                    }
                    typeFilter = b => b.IncludeContainerTypes(optIncludeCont.Values);
                }
                else if (optExcludeCont.HasValue())
                    typeFilter = b => b.ExcludeContainerTypes(optExcludeCont.Values);
                else
                    typeFilter = b => b.ContainerBenchmarks;

                if (optExcludeExtended.HasValue())
                {
                    var oldFilter = typeFilter;

                    typeFilter = b => oldFilter(b).Where(cb => cb.ExtendedName == null);
                }

                List<BenchmarkResult> results = new List<BenchmarkResult>();
                BenchmarkResult last = null;
                WriteLine($"- - - - - - - - - - - - - - - - - - - - - - - - - - ");
                WriteLine($"Running benchmarks. Loop count is { count }, Max run time is { timeOut }s.");
                WriteLine();

                // cancellation token which controls whether we continue to wait for a key for early termination.
                CancellationTokenSource awaitKeyCancel;
                // cancellation token which controls whether we continue to wait for a benchmark to finish
                CancellationTokenSource awaitBenchmarkCancel;
                Task awaitKey;
                Stopwatch masterSW = new Stopwatch();

                foreach (var toRun in benches)
                {
                    foreach (var containerBench in typeFilter(toRun))
                    {
                        WriteLine($"Running { containerBench.DisplayName } ({ containerBench.ContainerType })...", ConsoleColor.White);

                        Write("Press any key to cancel...", ConsoleColor.DarkCyan);

                        awaitBenchmarkCancel = new CancellationTokenSource(TimeSpan.FromSeconds(timeOut));
                        awaitKeyCancel = new CancellationTokenSource();

                        awaitKey = CancelBenchmark(awaitKeyCancel, awaitBenchmarkCancel);

                        masterSW.Restart();
                        results.Add(last = await containerBench.RunAsync(count, awaitBenchmarkCancel.Token));
                        masterSW.Stop();

                        ClearLine();

                        if (!awaitKeyCancel.IsCancellationRequested)
                        {
                            awaitKeyCancel.Cancel();
                            await awaitKey;
                        }

                        if (last.Success)
                        {
                            if (last.Count == count)
                            {
                                WriteLine($"{ toRun.Name } finished successfully. Overall time: { masterSW.Elapsed.TotalMilliseconds }ms.", ConsoleColor.Green);
                                WriteLine($"Benchmark time: { last.RunTime.TotalMilliseconds }ms. { last.Count / last.RunTime.TotalMilliseconds } ops/ms", ConsoleColor.Green);
                            }
                            else
                            {
                                WriteLine($"{ toRun.Name } did not run to completion. { last.Count } loops completed in { masterSW.Elapsed.TotalMilliseconds }ms.", ConsoleColor.Yellow);
                                WriteLine($"Benchmark time: { last.RunTime.TotalMilliseconds }ms.  { last.Count / last.RunTime.TotalMilliseconds } ops/ms (PROVISIONAL)", ConsoleColor.DarkYellow);
                            }
                        }
                        else
                        {
                            WriteLine($"{ toRun.Name } failed. Error message: { last?.Error?.Message }.", ConsoleColor.Red);
                            WriteLine($"Loops Completed: { last.Count }. Time taken: { last.RunTime.TotalMilliseconds }ms.", ConsoleColor.Red);
                        }
                        WriteLine();
                    }
                }
                return 0;
            });

            app.OnExecute(appExe);

            return app.Execute(args);
        }

        private static Task CancelBenchmark(CancellationTokenSource awaitKeyCancel, CancellationTokenSource awaitBenchmarkCancel)
        {
            return Task.Factory.StartNew(() =>
            {
                // make sure there are no keys waiting to be gulped
                SwallowKeys();
                while (!Console.KeyAvailable && !awaitKeyCancel.IsCancellationRequested)
                {
                    Task.Delay(100).Wait();
                }

                if (!awaitBenchmarkCancel.IsCancellationRequested)
                    awaitBenchmarkCancel.Cancel();
            });
        }

        private static void ClearLine()
        {
            int toDelete = Console.CursorLeft;
            if (toDelete != 0)
            {
                Console.CursorLeft = 0;
                Console.Write("".PadLeft(toDelete));
                Console.CursorLeft = 0;
            }
        }

        private static void WriteLine(string msg = null, ConsoleColor? color = null)
        {
            if(!string.IsNullOrWhiteSpace(msg))
                Write(msg, color);
            Console.WriteLine();
        }

        private static void Write(string msg, ConsoleColor? color = null)
        {
            if (color == null)
                Console.Write(msg);
            else
            {
                var previousColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                Console.Write(msg);
                Console.ForegroundColor = previousColor;
            }
        }

        private static void SwallowKeys()
        {
            while (Console.KeyAvailable)
            {
                // swallow up any pressed keys
                Console.ReadKey(true);
            }
        }

        static IEnumerable<Benchmark> FilterBenchmarks(IEnumerable<Benchmark> benches, CommandOption optInclude, CommandOption optExclude)
        {
            HashSet<string> groupFilter = new HashSet<string>();

            if (optInclude.HasValue())
            {
                if (optExclude.HasValue())
                {
                    return null;
                }
                groupFilter = new HashSet<string>(optInclude.Values);
                benches = benches.Where(bg => groupFilter.Contains(bg.Name));
            }
            else if (optExclude.HasValue())
            {
                groupFilter = new HashSet<string>(optExclude.Values);
                benches = benches.Where(bg => !groupFilter.Contains(bg.Name));
            }

            return benches;
        }

        private static void ListBenchmarks(IEnumerable<Benchmark> groups)
        {
            WriteLine($"Total number of benchmark groups available: { groups.Count() }", ConsoleColor.White);
            WriteLine();

            foreach (var group in groups)
            {
                WriteLine($"{ group.Name }:", ConsoleColor.White);
                WriteLine("".PadRight(group.Name.Length + 1, '-'), ConsoleColor.White);
                WriteLine($"{ string.Join(", ", group.ContainerBenchmarks.Select(t => t.ContainerType)) }");
                WriteLine();
            }
        }

        private static IEnumerable<Benchmark> LoadBenchmarks()
        {
            var allBenches = new IContainerBenchmarks[]
            {
                new NoContainerBenchmarks(),
                new RezolverBenchmarks()
            }.SelectMany(cb => cb.CreateBenchmarks());

            return allBenches.GroupBy(b => b.BenchmarkName)
                .Select(g => new Benchmark(g.Key, g));
        }
    }
}
