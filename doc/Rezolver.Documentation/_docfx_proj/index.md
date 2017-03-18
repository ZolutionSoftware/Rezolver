# Rezolver

Rezolver is a [fast](docs/benchmarks.md), efficient, extensible, [open source](https://github.com/ZolutionSoftware/Rezolver) IOC container supporting .Net 
Core.  This site contains API documentation and developer guides for getting the most out of it.  

## Features

_Linked topics provide high level overviews and examples in our developer guide - click on them to find out more! (*)_

- [Constructor Injection](docs/constructor-injection/index.md)
  - Multiple constructors supported
  - 'Intelligent' constructor discovery based on registered services
  - Named argument binding
  - Parameters with default arguments are supported
  - [Member injection](docs/constructor-injection/member-binding.md) (extensible)
- [Open generics](docs/constructor-injection/generics.md) (with specific closed generics taking precedence)
- [Decorators](docs/decorators.md) (non-generic and generic)
- Enumerables (empty enumerables returned by default)
- Child containers (overriding registrations in one container with those of another)
  - Child registration containers (lower-level overriding of registrations for similar but sibling containers)
- Hierarchical lifetime scoping
- [Delegates](docs/delegates.md) and Expressions as factories, with argument injection
  - Explicit resolving supported inside factory/expression bodies
- Singletons
- [Objects as services](docs/objects.md)
- Scoped objects (i.e. 'singleton per scope')
- No 'prepare' phase - you can register targets in a container after you start using it
  - *Note - services which have already been used cannot yet be replaced*
- Extensible compiler framework
  - Expression tree compiler used by default
- Integration with [Microsoft.Extensions.DependencyInjection](docs/nuget-packages/rezolver.microsoft.extensions.dependencyinjection.md) and 
[Microsoft.AspNetCore.Hosting](docs/nuget-packages/rezolver.microsoft.aspnetcore.hosting.md) via nuget packages
- Extensible configuration framework *(still in development)*
  - Json configuration *(still in development)*

_<small> * If a topic isn't linked, it's probably because we're still working on the documentation!</small>_

## Quick Links

- [Developer Guide](docs/index.md)
- [Benchmarks](docs/benchmarks.md)
- [Nuget Packages](docs/nuget-packages/index.md)
- [API Reference](api/index.md)
- [Release History (On Github)](https://github.com/ZolutionSoftware/Rezolver/releases)