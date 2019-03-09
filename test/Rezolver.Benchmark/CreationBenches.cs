using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using Rezolver.Benchmark.Types;
using Rezolver.Targets;
using System;
using System.Collections.Generic;

namespace Rezolver.Benchmark
{
    [CoreJob]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [MemoryDiagnoser]
    public class CreationBenches
    {
        private Lazy<Singleton> _singleton;
        private Container _containerPreppedGeneric;
        private Container _containerPreppedNonGeneric;
        private Container _containerCompiled;
        private Consumer _consumer;

        [GlobalSetup(Targets = new[] { nameof(No_New), nameof(No_WithArg), nameof(No_Complex), nameof(No_Enumerable) })]
        public void SetupNo()
        {
            _singleton = new Lazy<Singleton>(() => new Singleton());
            _consumer = new Consumer();
        }

        private void RegisterTypesInContainer(Container container)
        {
            container.RegisterType<SimpleType>();
            container.RegisterType<SimpleType2>();
            container.RegisterType<SimpleType3>();
            container.RegisterType<RequiresSimpleType>();
            container.RegisterType<RequiresSimpleType2>();
            container.RegisterType<RequiresSimpleType3>();
            container.RegisterType<RequiresLots>();
            container.RegisterSingleton<Singleton>();
            container.RegisterType<Types.RequiresLotsAndSingleton>();
        }

        [GlobalSetup(Targets = new[] { nameof(Rezolver_New_Generic), nameof(Rezolver_WithArg_Generic), nameof(Rezolver_Complex_Generic), nameof(Rezolver_Enumerable_Generic) })]
        public void SetupPreppedGeneric()
        {
            _containerPreppedGeneric = new Container();
            _consumer = new Consumer();

            RegisterTypesInContainer(_containerPreppedGeneric);

            var instances = WarmupGeneric(_containerPreppedGeneric);
        }

        [GlobalSetup(Targets = new[] { nameof(Rezolver_New_NonGeneric), nameof(Rezolver_WithArg_NonGeneric), nameof(Rezolver_Complex_NonGeneric), nameof(Rezolver_Enumerable_NonGeneric) })]
        public void SetupPreppedNonGeneric()
        {
            _containerPreppedNonGeneric = new Container();
            _consumer = new Consumer();

            RegisterTypesInContainer(_containerPreppedNonGeneric);

            var instances = WarmupNonGeneric(_containerPreppedNonGeneric);
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

        private List<object> WarmupGeneric(Container container)
        {
            // literally just calls each of the warm methods once to force compilation
            return new List<object>
            {
                Rezolver_New_Generic(),
                Rezolver_WithArg_Generic(),
                Rezolver_Complex_Generic(),
                Rezolver_CreateEnumerable_Generic()
            };
        }

        private List<object> WarmupNonGeneric(Container container)
        {
            // literally just calls each of the warm methods once to force compilation
            return new List<object>
            {
                Rezolver_New_NonGeneric(),
                Rezolver_WithArg_NonGeneric(),
                Rezolver_Complex_NonGeneric(),
                Rezolver_CreateEnumerable_NonGeneric()
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
        [BenchmarkCategory("Enumerable")]
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

        #region rezolver benchmarks (Generic, prepared)

        [Benchmark]
        [BenchmarkCategory("New")]
        public SimpleType Rezolver_New_Generic() => _containerPreppedGeneric.Resolve<SimpleType>();

        [Benchmark]
        [BenchmarkCategory("NewCtorArg")]
        public RequiresSimpleType Rezolver_WithArg_Generic() => _containerPreppedGeneric.Resolve<RequiresSimpleType>();

        [Benchmark]
        [BenchmarkCategory("Complex")]
        public RequiresLotsAndSingleton Rezolver_Complex_Generic() => _containerPreppedGeneric.Resolve<RequiresLotsAndSingleton>();

        private IEnumerable<ISimpleType> Rezolver_CreateEnumerable_Generic() => _containerPreppedGeneric.ResolveMany<ISimpleType>();

        [Benchmark]
        [BenchmarkCategory("Enumerable")]
        public void Rezolver_Enumerable_Generic() => _containerPreppedGeneric.ResolveMany<ISimpleType>().Consume(_consumer);

        #endregion

        #region rezolver benchmarks (Generic, prepared)

        [Benchmark]
        [BenchmarkCategory("New")]
        public SimpleType Rezolver_New_NonGeneric() => (SimpleType)_containerPreppedNonGeneric.Resolve(typeof(SimpleType));

        [Benchmark]
        [BenchmarkCategory("NewCtorArg")]
        public RequiresSimpleType Rezolver_WithArg_NonGeneric() => (RequiresSimpleType)_containerPreppedNonGeneric.Resolve(typeof(RequiresSimpleType));

        [Benchmark]
        [BenchmarkCategory("Complex")]
        public RequiresLotsAndSingleton Rezolver_Complex_NonGeneric() => (RequiresLotsAndSingleton)_containerPreppedNonGeneric.Resolve(typeof(RequiresLotsAndSingleton));

        private IEnumerable<ISimpleType> Rezolver_CreateEnumerable_NonGeneric() => (IEnumerable<SimpleType>)_containerPreppedNonGeneric.Resolve(typeof(IEnumerable<SimpleType>));

        [Benchmark]
        [BenchmarkCategory("Enumerable")]
        public void Rezolver_Enumerable_NonGeneric() => _containerPreppedNonGeneric.ResolveMany<ISimpleType>().Consume(_consumer);

        #endregion
    }
}
