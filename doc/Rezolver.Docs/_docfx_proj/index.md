# Rezolver

Rezolver is a [fast](docs/benchmarks.md), efficient, extensible, [open source](https://github.com/ZolutionSoftware/Rezolver) IOC container 
with Asp.Net Core integration.

The nuget package has binaries specifically targeted to these TFMs:

- `netstandard2.1`
- `netstandard2.0`
- `netcoreapp3.0`
- `netcoreapp2.2`
- `net472`
- `net48`

> [!TIP]
> While Rezolver is fast, whichever version you use, the `netstandard2.0` build is slightly slower because it's not able to use some dynamic IL
> generation that's used by the versions which target the other TFMs.
>
> From Version 3.0, Rezolver will only target `netstandard2.0` and `netstandard2.1`.

# Integration

In addition to simply using the Rezolver nuget package and creating a @Rezolver.Container directly, Rezolver also offers some off-the-shelf integrations:

- [.Net Core Generic Host 2.2+ and Asp.Net Core 3+](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) support is provided by the [Rezolver.Microsoft.Extensions.Hosting](docs/nuget-packages/rezolver.microsoft.extensions.hosting.md) package
- Low-level integration with the core Microsoft.Extensions.DependencyInjection DI abstractions (i.e creating an @System.IServiceProvider from an <xref:Microsoft.Extensions.DependencyInjection.IServiceCollection>) is provided by the [Rezolver.Microsoft.Extensions.DependencyInjection](docs/nuget-packages/rezolver.microsoft.extensions.dependencyinjection.md) package.  This package is used by the other two packages above.
- Asp.Net Core support (v2.2 and below) is provided by the [Rezolver.Microsoft.AspNetCore.Hosting](docs/nuget-packages/rezolver.microsoft.aspnetcore.hosting.md) package, the last version of which will be 2.3.

# Quickstart

Read the [quickstart tutorial](quickstart.md) - which shows how to create and use Rezolver containers.

# Release Highlights

For full release notes for each version - [see release notes on Github](https://github.com/ZolutionSoftware/Rezolver/releases).
- **2.1.0**
  - Added `netstandard2.1` & `netcoreapp3.0` to main `Rezolver` package.
    - We're forced to do this at the moment, because of the prior targeting of `netcoreapp2.2`.  Rezolver v3 will only target `netstandard2.0` and `netstandard2.1`.
  - `Rezolver.Microsoft.Extensions.Hosting` is now v3.0 and uses `Microsoft.Extensions.Hosting.Abstractions` v3.0.
    - Renamed and made public the `RezolverServiceProviderFactory` class to allow it to be used directly; e.g. for Blazor WebAssembly:
    ```cs
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseServiceProviderFactory(new RezolverServiceProviderFactory())
                .UseBlazorStartup<Startup>();
    }
    ```
  - `Rezolver.Microsoft.Extensions.DependencyInjection` is also now v3.0 and uses `Microsoft.Extensions.DependencyInjection.Abstractions` v3.0
- **2.0.0**
  - Major performance improvements
  - TFMs changed to `netStandard2.0`, `netcoreapp2.2`, `net472` and `net48` ([#78](https://github.com/ZolutionSoftware/Rezolver/issues/78))
  - Scopes are now always present ([#89](https://github.com/ZolutionSoftware/Rezolver/issues/89))
  - Got rid of a whole bunch of interfaces in favour of concrete classes with non-virtual (where possible) methods ([#90](https://github.com/ZolutionSoftware/Rezolver/issues/90)):
    - `IResolveContext` -> `ResolveContext`
    - `IContainer` -> `Container`
    - `IContainerScope` -> `ContainerScope`
    - `ICompiledTarget` -> retired for factory delegates, @Rezolver.IInstanceProvider and @Rezolver.IInstanceProvider`1
  - `OverridingContainer` renamed to `ChildContainer` ([#87](https://github.com/ZolutionSoftware/Rezolver/issues/87))
    - Constructor now no longer accepts an `IRootTargetContainer` on creation.  To register new services, you create a new instance and then register into it via its implementation of `IRootTargetContainer`
  - `OverridingTargetContainer` renamed to `ChildTargetContainer` ([#88](https://github.com/ZolutionSoftware/Rezolver/issues/88))
- **1.4.0**
  - [.Net Core Generic Host Support](docs/nuget-packages/rezolver.microsoft.extensions.hosting.md) via the `Rezolver.Microsoft.Extensions.Hosting` package
  - ['Autofactories'](docs/autofactories.md) -  Container-created factory methods (`e.g. Func<T>` or `delegate Foo FooFactory(IBar, IBaz)`) which create instances via the container/scope
  - [Automatic `Lazy<T>` creation](docs/lazy.md) - With instances created via the container/scope
  - Fixed a fatal bug in release builds of Asp.Net Core sites ([Issue #77](https://github.com/ZolutionSoftware/Rezolver/issues/77))
- **1.3.4**
  - Added [Sourcelink](https://github.com/dotnet/sourcelink) to the packaged assemblies.
  - Bumped Asp.Net Core integration packages to 2.1
- **1.3.3**
  - Bugfix release - see the release notes linked above for more
- **1.3.2**
  - [Generic Covariance](docs/variance/covariance.md)
  - [Mixed Variance](docs/variance/mixed.md)
  - [Enumerable Projections](docs/enumerables/projections.md)
  - [Targeting specific constructors of open generics](docs/constructor-injection/generics-manual-constructor.md)
  - [List member bindings](docs/member-injection/collections.md)
  - [Per-member bindings (Fluent API)](docs/member-injection/fluent-api.md)
- **1.3.1**  
  - Added .Net Standard 2.0 support and more.

<a name="features"></a>
# Feature Overview

_Linked topics provide high level overviews and examples in our developer guide - click on them to find out more! (*)_

## Core Features

- [Constructor Injection](docs/constructor-injection/index.md)
  - Multiple constructors supported
  - Up-front constructor selection
  - Automatic, 'intelligent' constructor discovery based on registered services
  - Named argument binding
  - Parameters with default arguments are supported
- [Member injection](docs/member-injection/index.md)
  - Automatic public property (and/or) field injection
  - [Collection initialisers](docs/member-injection/collections.md) (via enumerables)
  - [Fluent API](docs/member-injection/fluent-api.md) for per-member behaviours
  - [Extensible API](docs/member-injection/custom.md) for completely custom behaviours
- [Open Generic Constructor Injection](docs/constructor-injection/generics.md)
  - [Up-front constructor selection](docs/constructor-injection/generics-manual-constructor.md)
  - Specific closed generics take precedence
- [Factory Delegates](docs/delegates.md)
  - Argument injection
  - Explicit resolving supported inside factory bodies
- [Factory Expressions](docs/expressions.md)
  - Argument injection
  - Explicit resolving as above
- [Singletons](docs/lifetimes/singleton.md)
- [Constant services](docs/objects.md)
- [Hierarchical lifetime scoping](docs/lifetimes/container-scopes.md)
- [Scoped objects](docs/lifetimes/scoped.md) (i.e. 'singleton per scope')

## Enumerables, Arrays and Collections

- [Automatic enumerable injection](docs/enumerables.md) 
  - *Empty enumerables returned by default*
  - [Lazy and eager enumerables](docs/enumerables/lazy-vs-eager.md)
    - *Configurable on a per-type basis (added in 1.3)*
  - [Enumerables of generics](docs/enumerables/generics.md)
  - [Enumerable covariance support](docs/variance/covariance.md#enumerables)
  - [Projections](docs/enumerables/projections.md)
- [Array injection](docs/arrays-lists-collections/arrays.md)
- [`List<T>` injection](docs/arrays-lists-collections/lists.md)
  - Also `IList<T>` and `IReadOnlyList<T>`
- [`Collection<T>` injection](docs/arrays-lists-collections/collections.md)
  - Also `ICollection<T>`, `ReadOnlyCollection<T>` and `IReadOnlyCollection<T>`

## Advanced

- [Delegate injection](docs/autofactories.md) ('autofactories')
- [Lazy injection](docs/lazy.md)
- [Generic Variance](docs/variance/index.md)
  - [Generic Contravariance](docs/variance/contravariance.md) (`Action<IFoo>` resolved for `Action<Foo>`)
  - [Generic Covariance](docs/variance/covariance.md) (`Func<Foo>` resolved for `Func<IFoo>`)
  - [Mixed Variance](docs/variance/mixed.md) (`Func<IFoo, Bar>` resolved for `Func<Foo, IBar>`)
  - *Can be enabled and disabled globally and per-type*
  - *(As noted above) Enumerables and collections automatically include all generic variant matches*
- [Decorators](docs/decorators.md)
  - Non-generic & generic constructor injection
  - Specialised generic (*where a decorator for `IFoo<>` is redecorated by another decorator for `IFoo<Bar>` only when
`IFoo<Bar>` is requested*)
  - [Enumerables of decorated instances](docs/enumerables.md#decorators-and-enumerables)
  - [Decorator Delegates](docs/decorators/delegates.md)
  - Can decorate any of the built-in enumerable/collection types
- Constrained generics
- Child containers
  - Including child registration containers (lower-level overriding of registrations for similar but sibling containers)

## Other
- No 'prepare' phase - you can register targets in a container after you start using it
  - *Note - services which have already been used cannot yet be replaced, but mutable containers are planned.  In the meantime, you
can use child containers to create new registrations which override those of another container*.
- Extensible compiler framework
  - Expression tree compiler used by default
- Extensible 'targets'
  - Can write your own code which produces objects when Rezolver matches it to a requested type

_<small> * If a topic isn't linked, it's probably because we're still working on the documentation, or because it's covered in the parent topic</small>_

## Quick Links

- [Quickstart](quickstart.md)
- [Developer Guide](docs/index.md)
- [Nuget Packages](docs/nuget-packages/index.md)
- [API Reference](api/index.md)
- [Release History (On Github)](https://github.com/ZolutionSoftware/Rezolver/releases)