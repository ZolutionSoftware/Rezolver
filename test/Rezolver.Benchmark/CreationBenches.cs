using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using Rezolver.Benchmark.Types;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Benchmark
{
    [CoreJob]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [MemoryDiagnoser]
    public class CreationBenches
    {
        private Lazy<Singleton> _singleton;
        private Container _containerPrepped;
        private Container _containerCompiled;
        private Consumer _consumer;

        [GlobalSetup(Targets = new[] { nameof(No_New), nameof(No_WithArg), nameof(No_Complex), nameof(No_Enumerable) })]
        public void SetupNo()
        {
            _singleton = new Lazy<Singleton>(() => new Singleton());
            _consumer = new Consumer();
        }

        [GlobalSetup(Targets = new[] { nameof(Rezolver_New), nameof(Rezolver_WithArg), nameof(Rezolver_Complex), nameof(Rezolver_Enumerable) })]
        public void SetupPrepped()
        {
            _containerPrepped = new Container();
            _consumer = new Consumer();

            _containerPrepped.RegisterType<SimpleType>();
            _containerPrepped.RegisterType<SimpleType2>();
            _containerPrepped.RegisterType<SimpleType3>();
            _containerPrepped.RegisterType<RequiresSimpleType>();
            _containerPrepped.RegisterType<RequiresSimpleType2>();
            _containerPrepped.RegisterType<RequiresSimpleType3>();
            _containerPrepped.RegisterType<RequiresLots>();
            _containerPrepped.RegisterSingleton<Singleton>();
            _containerPrepped.RegisterType<Types.RequiresLotsAndSingleton>();

            var instances = Warmup(_containerPrepped);
        }

        private class CreateSimpleTypeTarget : ITarget, ICompiledTarget
        {
            public ITarget SourceTarget => this;
            public int Id { get; } = TargetBase.NextId();
            public bool UseFallback => false;
            public Type DeclaredType => typeof(SimpleType);
            public ScopeBehaviour ScopeBehaviour => ScopeBehaviour.Implicit;
            public ScopePreference ScopePreference => ScopePreference.Current;

            public object GetObject(ResolveContext context)
            {
                return new SimpleType();
            }

            public bool SupportsType(Type type)
            {
                return type == typeof(SimpleType);
            }
        }

        [GlobalSetup(Targets = new[] { nameof(Rezolver_New_ICompiled) })]
        public void SetupCompiled()
        {
            _containerCompiled = new Container();
            _consumer = new Consumer();

            _containerCompiled.Register(new CreateSimpleTypeTarget());

            var first = _containerCompiled.Resolve<SimpleType>();
        }

        private List<object> Warmup(IContainer container)
        {
            // literally just calls each of the warm methods once to force compilation
            return new List<object>
            {
                Rezolver_New(),
                Rezolver_WithArg(),
                Rezolver_Complex(),
                Rezolver_CreateEnumerable()
            };
        }

        #region no container benchmarks

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("New")]
        public SimpleType No_New() => new SimpleType();

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("NewCtorArg")]
        public RequiresSimpleType No_WithArg() => new RequiresSimpleType(new SimpleType());

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Complex")]
        public RequiresLotsAndSingleton No_Complex()
            => new Types.RequiresLotsAndSingleton(new Types.RequiresLots(new RequiresSimpleType(new SimpleType()),
                    new RequiresSimpleType2(new SimpleType2()), new RequiresSimpleType3(new SimpleType3()), new SimpleType(), new SimpleType2(), new SimpleType3()), _singleton.Value);

        private IEnumerable<ISimpleType> No_CreateEnumerable()
        {
            yield return new SimpleType();
            yield return new SimpleType2();
            yield return new SimpleType3();
        }

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Enumerable-Simple")]
        public void No_Enumerable()
        {
            No_CreateEnumerable().Consume(_consumer);
        }

        #endregion

        #region rezolver benchmarks (ICompiledTarget)

        [Benchmark]
        [BenchmarkCategory("New")]
        public SimpleType Rezolver_New_ICompiled() => _containerCompiled.Resolve<SimpleType>();

        #endregion

        #region rezolver benchmarks (prepared)

        [Benchmark]
        [BenchmarkCategory("New")]
        public SimpleType Rezolver_New() => _containerPrepped.Resolve<SimpleType>();

        [Benchmark]
        [BenchmarkCategory("NewCtorArg")]
        public RequiresSimpleType Rezolver_WithArg() => _containerPrepped.Resolve<RequiresSimpleType>();

        [Benchmark]
        [BenchmarkCategory("Complex")]
        public RequiresLotsAndSingleton Rezolver_Complex() => _containerPrepped.Resolve<RequiresLotsAndSingleton>();

        private IEnumerable<ISimpleType> Rezolver_CreateEnumerable() => _containerPrepped.ResolveMany<ISimpleType>();

        [Benchmark]
        [BenchmarkCategory("Enumerable-Simple")]
        public void Rezolver_Enumerable() => _containerPrepped.ResolveMany<ISimpleType>().Consume(_consumer);

        #endregion
    }
}
