Rezolver
========

Rezolver is a high performance, portable (.NetStandard 1.1), open-source IOC container built in pre-Brexit UK, with love :)

> You can get the [Rezolver Package from Nuget](https://www.nuget.org/packages/Rezolver/), and for more 
> packages, head over to the [packages documentation](http://rezolver.co.uk/developers/docs/nuget-packages/index.html).

More information, including API reference and developer how-tos, is available also on the
[Rezolver website](http://rezolver.co.uk).

> **Note -** writing docs takes a while(!) so if there's something you're looking for and can't find it
> on the website, then see if you can find examples in one of the [test projects](test/readme.md) - as nearly all
> the functionality is covered in there :)

[Follow us on twitter](https://twitter.com/RezolverIOC) for code or documentation updates/release notifications etc.

## Benchmarks

[Check out some benchmarks results for Rezolver](http://rezolver.co.uk/developers/docs/benchmarks.html).
It finishes around top 10 in *almost* everything that's tested - and the speed will only improve
over time. 

## Features

- [Constructor injection](http://rezolver.co.uk/developers/docs/constructor-injection/index.html)
  - Multiple constructors supported
  - 'Intelligent' constructor discovery based on registered services
  - Named argument binding
  - Parameters with default arguments are supported
  - [Member injection](http://rezolver.co.uk/developers/docs/constructor-injection/member-binding.html) (extensible)
- [Open generics](http://rezolver.co.uk/developers/docs/constructor-injection/generics.html) 
  - Specific closed generics take precedence
- Decorators (non-generic and generic)
- [Enumerables](http://rezolver.co.uk/developers/docs/enumerables.html) (empty enumerables returned by default)
- Child containers (overriding registrations in one container with those of another)
  - Child registration containers (lower-level overriding of registrations for similar but sibling containers)
- Hierarchical lifetime scoping
- [Delegates](http://rezolver.co.uk/developers/docs/delegates.html) and [Expressions](http://rezolver.co.uk/developers/docs/expressions.html) as factories, with argument injection
  - Explicit resolving supported inside factory/expression bodies
- Singletons
- [Objects as services](http://rezolver.co.uk/developers/docs/objects.html)
- Scoped objects (i.e. 'singleton per scope')
- No 'prepare' phase - you can register targets in a container after you start using it
  - *Note - services which have already been used cannot yet be replaced*
- Extensible compiler framework
  - Expression tree compiler used by default
- Direct Asp.Net Core integration via extensions to the [Microsoft.Extensions.DependencyInjection](http://rezolver.co.uk/developers/docs/nuget-packages/rezolver.microsoft.extensions.dependencyinjection.html) 
and [Microsoft.AspNetCore.Hosting](http://rezolver.co.uk/developers/docs/nuget-packages/rezolver.microsoft.aspnetcore.hosting.html) packages via nuget packages (click the links to go to our package docs)
- Extensible configuration framework *(still in development)*
  - Json configuration *(still in development)*

## Planned/In Development 

Current Rezolver work item status:

[![Work in Progress](https://badge.waffle.io/ZolutionSoftware/Rezolver.png?label=in%20progress&title=In%20Progress)](http://waffle.io/ZolutionSoftware/Rezolver) 
[![Items ready to go](https://badge.waffle.io/ZolutionSoftware/Rezolver.png?label=ready&title=Ready)](http://waffle.io/ZolutionSoftware/Rezolver)

Click on the badges to see the whole board, including also the backlog, and what's been done.

### For the framework (in no particular order)

Here's a bit more detail on some of the bigger things that have been planned for ages (but might not yet be on the board :) )

- <strike>Migrate to MSBuild-based projects for VS 2017</strike>
- Traditional Asp.Net and Web API integration
- Low-level logging
  - *this was already in development, but due to recent changes needs reworking*
- Auto-injected factories e.g. register `IService` and request a `Func<IService>`
- Reflection-based compiler (faster first-call vs expression compiler)
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
- Mutable containers - *without* speed penalties

### For [rezolver.co.uk](http://rezolver.co.uk)

- <strike>Styling of the footer (where'd that green come from!?)</strike>
- Full documentation of all targets ***85%* done**
- <strike>Results of benchmarking via https://github.com/danielpalme/IocPerformance (*hint* - only child containers letting it down at the moment, otherwise v.quick)</strike> - [here](http://rezolver.co.uk/developers/docs/benchmarks.html)
- Scoping documentation
- Snazzier homepage ;)
- Compilation walkthrough
- Experimental examples

# Contributing

Feel free to fork this repo, build from the source, and submit pull requests for new functionality or bugfixes!

The solution is now an msbuild-based VS2017 solution.
