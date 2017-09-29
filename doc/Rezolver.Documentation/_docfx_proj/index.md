# Rezolver

Rezolver is a [fast](docs/benchmarks.md), efficient, extensible, [open source](https://github.com/ZolutionSoftware/Rezolver) IOC container 
with Asp.Net Core integration.

The nuget package is built for the following .Net Versions:

- .Net Standard 1.1
- .Net 4.5.1
- .Net 4.6

This site contains API documentation and developer guides for getting the most out of it.  

## Features

_Linked topics provide high level overviews and examples in our developer guide - click on them to find out more! (*)_

- [Constructor Injection](docs/constructor-injection/index.md)
  - Multiple constructors supported
  - 'Intelligent' constructor discovery based on registered services
  - Named argument binding
  - Parameters with default arguments are supported
  - [Member injection](docs/constructor-injection/member-injection.md) (extensible)
- [Open Generic Constructor Injection](docs/constructor-injection/generics.md) (with specific closed generics taking precedence)
- [Enumerables](docs/enumerables.md) (empty enumerables returned by default)
  - [Lazy and eager enumerables](docs/enumerables/lazy-vs-eager.md) - configurable on a per-type basis (new in 1.3)
  - [Enumerables of generics](docs/enumerables/generics.md) - from least generic (e.g. `IFoo<Bar>`) to most generic (`IFoo<>`)
- [Array injection](docs/arrays-lists-collections/arrays.md)
- [`List<T>` injection](docs/arrays-lists-collections/lists.md)
  - Also `IList<T>` and `IReadOnlyList<T>`
- [`Collection<T>` injection](docs/arrays-lists-collections/collections.md)
  - Also `ICollection<T>`, `ReadOnlyCollection<T>` and `IReadOnlyCollection<T>`
- [Decorators](docs/decorators.md)
  - Non-generic & generic constructor injection
  - Specialised generic (*where a decorator for `IFoo<>` is redecorated by another decorator for `IFoo<Bar>` only when
`IFoo<Bar>` is requested*)
  - [Enumerables of decorated instances](docs/enumerables.md#decorators-and-enumerables)
  - [Decorator Delegates](docs/decorators/delegates.md)
  - Can decorate any of the built-in enumerable/collection types
- [Generic Contravariance](docs/contravariance.md) (e.g. `Action<IFoo>` registration automatically used for `Action<Foo>`)
  - Can be enabled and disabled globally and per-type
  - Enumerables and collections automatically include all contravariant matches
- Constrained generics
- Child containers (overriding registrations in one container with those of another)
  - Child registration containers (lower-level overriding of registrations for similar but sibling containers)
- [Hierarchical lifetime scoping](docs/lifetimes/container-scopes.md)
- [Delegates](docs/delegates.md) and [Expressions](docs/expressions.md) as factories, with argument injection
  - Explicit resolving supported inside factory/expression bodies
- [Singletons](docs/lifetimes/singleton.md)
- [Objects as services](docs/objects.md)
- [Scoped objects](docs/lifetimes/scoped.md) (i.e. 'singleton per scope')
- No 'prepare' phase - you can register targets in a container after you start using it
  - *Note - services which have already been used cannot yet be replaced, but high-performance mutable containers are on their way*
- Extensible compiler framework
  - Expression tree compiler used by default
- Extensible 'targets'
  - Can write your own code which produces objects when Rezolver matches it to a request type
- Direct Asp.Net Core integration via extensions to the [Microsoft.Extensions.DependencyInjection](docs/nuget-packages/rezolver.microsoft.extensions.dependencyinjection.md) and 
[Microsoft.AspNetCore.Hosting](docs/nuget-packages/rezolver.microsoft.aspnetcore.hosting.md) packages via nuget packages (click the links to go to our package docs)
- Extensible configuration framework *(under review)*
  - Json configuration *(under review)*

_<small> * If a topic isn't linked, it's probably because we're still working on the documentation, or because it's covered in the parent topic</small>_

## Quick Links

- [Developer Guide](docs/index.md)
- [Nuget Packages](docs/nuget-packages/index.md)
- [API Reference](api/index.md)
- [Release History (On Github)](https://github.com/ZolutionSoftware/Rezolver/releases)