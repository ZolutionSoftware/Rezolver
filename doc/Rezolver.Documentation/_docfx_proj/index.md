# Rezolver

Rezolver is a [fast](docs/benchmarks.md), efficient, extensible, [open source](https://github.com/ZolutionSoftware/Rezolver) IOC container 
with Asp.Net Core integration.

The nuget package has binaries specifically targeted to these .Net versions/standards:

- .Net Standard 2.0
- .Net Standard 1.1
- .Net 4.5.1
- .Net 4.6

## Asp.Net Core Support

Asp.Net Core Support (v2.0 and v1.1) is provided by the 
[Rezolver.Microsoft.AspNetCore.Hosting package](docs/nuget-packages/rezolver.microsoft.aspnetcore.hosting.md),
supported by the [Rezolver.Microsoft.Extensions.DependencyInjection package](docs/nuget-packages/Rezolver.Microsoft.Extensions.DependencyInjection.md)
which provides an implementation of the new .Net Core DI container abstraction.


# Release Highlights

For full release notes for each version - [see release notes on Github](https://github.com/ZolutionSoftware/Rezolver/releases).

- **1.3.2**
  - [Generic Covariance](docs/variance/covariance.md)
  - [Mixed Variance](docs/variance/mixed.md)
  - [Enumerable Projections](docs/enumerables/projections.md)
  - [Targeting specific constructors of open generics](docs/constructor-injection/generics-manual-constructor.md)
  - [List member bindings](docs/member-injection/collections.md)
  - [Per-member bindings (Fluent API)](docs/member-injection/fluent-api.md)
- **1.3.1**  
  - Added .Net Standard 2.0 support and more.

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
  - *Note - services which have already been used cannot yet be replaced, but high-performance mutable containers are on their way*
- Extensible compiler framework
  - Expression tree compiler used by default
- Extensible 'targets'
  - Can write your own code which produces objects when Rezolver matches it to a requested type

_<small> * If a topic isn't linked, it's probably because we're still working on the documentation, or because it's covered in the parent topic</small>_

## Quick Links

- [Developer Guide](docs/index.md)
- [Nuget Packages](docs/nuget-packages/index.md)
- [API Reference](api/index.md)
- [Release History (On Github)](https://github.com/ZolutionSoftware/Rezolver/releases)