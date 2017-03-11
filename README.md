Rezolver
========

Rezolver is a high performance, portable (.NetStandard 1.1), open-source IOC container
designed and built by Zolution Software Ltd.

> You can get the [Rezolver Package from Nuget](https://www.nuget.org/packages/Rezolver/), and for more 
> packages, head over to the [packages documentation](http://rezolver.co.uk/developers/docs/nuget-packages/index.html).

The latest version - 1.1 - decouples the compilation stack from the rest of the framework,
making it, and the rest of the API, completely extensible - providing hooks to implement
interception and other more 'exotic' functionality.

More information, including API reference and developer how-tos, is available also on the
[Rezolver website](http://rezolver.co.uk).

> **Note -** writing docs takes a while(!) so if there's something you're looking for and can't find it
> on the website, then see if you can find examples in one of the [test projects](test/readme.md) - as nearly all
> the functionality is covered in there :)
> 
> Work on the documentation is going on all the time, and content will be pushed as and when it's good 
> enough (hopefully every day or two).

## Benchmarks

[Check out some benchmarks results for Rezolver](doc/Rezolver.Documentation/_docfx_proj/docs/benchmarks.md).
It finishes around top 10 in almost everything that's tested - and the speed will only improve
over time. 

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

## Planned/In Development

### For the framework (in no particular order)

- Migrate to MSBuild-based projects for VS 2017
- Traditional Asp.Net and Web API integration
- Low-level logging
  - *this was already in development, but due to recent changes needs reworking*
- Auto-injected factories e.g. register `IService` and request a `Func<IService>`
- Reflection-base compiler (faster first-call vs expression compiler)
  - Mixed compiler - uses reflection until expression tree is built asynchronously
- Statically compiled container - turns container into a module that you can reference
and use with signficantly less startup/compilation cost.
- Container 'storage' API for singletons and scopes
  - Will allow singletons to operate per-container and allow for more exotic scoping scenarios
- Self registration of non-abstract classes - e.g. request `MyService` without registering it
first.
  - Just-in-time API extracted from this to provide a base on which to build interception and automocking
- Auto-built WCF client 'experiment' (just gotta try :) )
- Conditional targets with caching support in `CachingContainerBase`
- Truly mutable containers - add new targets for an enumerable, or replace a service registration
after using them, and have the cache purge automatically.

### For [rezolver.co.uk](http://rezolver.co.uk)

- <strike>Styling of the footer (where'd that green come from!?)</strike>
- Full documentation of all targets
- Results of benchmarking via https://github.com/danielpalme/IocPerformance (*hint* - only child containers letting it down at the moment, otherwise v.quick)
- Scoping documentation
- Compilation walkthrough
- Experimental examples

# Contributing

Feel free to fork this repo, build from the source, and submit pull requests for new functionality or bugfixes!

The solution is currently a project.json-based VS2015 solution, requiring the .Net Core 1.0 Preview 2 SDK.
We will soon be migrating to an msbuild-based VS2017 solution.
