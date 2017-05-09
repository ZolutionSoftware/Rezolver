# Rezolver

Rezolver is a [fast](docs/benchmarks.md), efficient, extensible, [open source](https://github.com/ZolutionSoftware/Rezolver) IOC container supporting .Net 
Core, .Net 4.5.2 and .Net 4.6 - with Asp.Net Core integration too.

This site contains API documentation and developer guides for getting the most out of it.  

## Features

_Linked topics provide high level overviews and examples in our developer guide - click on them to find out more! (*)_

- [Constructor Injection](docs/constructor-injection/index.md)
  - Multiple constructors supported
  - 'Intelligent' constructor discovery based on registered services
  - Named argument binding
  - Parameters with default arguments are supported
  - [Member injection](docs/constructor-injection/member-injection.md) (extensible)
- [Open generics](docs/constructor-injection/generics.md) (with specific closed generics taking precedence)
- [Decorators](docs/decorators.md)
  - Non-generic & Generic
  - Specialised generic (*where a decorator for `IFoo<>` is redecorated by another decorator for `IFoo<Bar>` only when
`IFoo<Bar>` is requested*)
  - Enumerables of decorated instances
- [Enumerables](docs/enumerables.md) (empty enumerables returned by default)
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
- Direct Asp.Net Core integration via extensions to the [Microsoft.Extensions.DependencyInjection](docs/nuget-packages/rezolver.microsoft.extensions.dependencyinjection.md) and 
[Microsoft.AspNetCore.Hosting](docs/nuget-packages/rezolver.microsoft.aspnetcore.hosting.md) packages via nuget packages (click the links to go to our package docs)
- Extensible configuration framework *(under review)*
  - Json configuration *(under review)*

_<small> * If a topic isn't linked, it's probably because we're still working on the documentation!</small>_

## Quick Links

- [Developer Guide](docs/index.md)
- [Benchmarks](docs/benchmarks.md)
- [Nuget Packages](docs/nuget-packages/index.md)
- [API Reference](api/index.md)
- [Release History (On Github)](https://github.com/ZolutionSoftware/Rezolver/releases)