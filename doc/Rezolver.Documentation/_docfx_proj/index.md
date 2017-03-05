# Rezolver

Rezolver is a fast, efficient, extensible, [open source](https://github.com/ZolutionSoftware/Rezolver) IOC container supporting .Net 
Core.  This site contains API documentation and developer guides for getting the most out of it.  

## Features

- Constructor injection
  - Multiple constructors supported
  - 'Intelligent' constructor discovery based on registered services
  - Named argument binding
  - Parameters with default arguments are supported
  - Member injection (extensible)
- Open generics (with specific closed generics taking precedence)
- Decorators (non-generic and generic)
- Enumerables (empty enumerables returned by default)
- Child containers (overriding registrations in one container with those of another)
  - Child registration containers (lower-level overriding of registrations for similar but sibling containers)
- Hierarchical lifetime scoping
- Delegates and Expressions as factories, with argument injection
  - Explicit resolving supported inside factory/expression bodies
- Singletons
- Pre-built objects
- Scoped objects (i.e. 'singleton per scope')
- No 'prepare' phase - you can register targets in a container after you start using it
  - *Note - services which have already been used cannot yet be replaced*
- Extensible compiler framework
  - Expression tree compiler used by default
- Integration with Microsoft.Extensions.DependencyInjection and Microsoft.AspNetCore.Hosting via nuget packages
- Extensible configuration framework *(still in development)*
  - Json configuration *(still in development)*

## Quick Links

- [Developer Guide](docs/index.md)
<!--- [Nuget Packages](docs/nuget-packages/index.md) -->
- [API Reference](api/index.md)