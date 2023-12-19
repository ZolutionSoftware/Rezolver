# Rezolver is now archived - no more updates will be pushed to the repository.

The library is still fully functional, but will not be updated to future versions of .Net, sorry!

Thanks for all the support :)

![Rezolver Logo](https://raw.githubusercontent.com/ZolutionSoftware/Rezolver/master/doc/Rezolver.Documentation/Content/rz_square_white_on_orange_256x256.png)

Rezolver
========

Rezolver is a high performance open-source IOC container for .Net Core.

> You can get the [Rezolver Package from Nuget](https://www.nuget.org/packages/Rezolver/), and for more 
> packages, head over to the [packages documentation](http://rezolver.co.uk/developers/docs/nuget-packages/index.html).

For more information, including API reference and developer how-tos, head on over to the 
[Rezolver website](http://rezolver.co.uk).

[Follow us on twitter](https://twitter.com/RezolverIOC) for code or documentation updates/release notifications etc.

[![Build Status](https://dev.azure.com/zolution/Rezolver/_apis/build/status/Rezolver-Nupkg?branchName=master)](https://dev.azure.com/zolution/Rezolver/_build/latest?definitionId=8&branchName=master)

---

# Version Highlights

## 2.0.0

- TFMs changed for main package ([#78](https://github.com/ZolutionSoftware/Rezolver/issues/78)):
  - `NetStandard2.0`, `netcoreapp2.2`, `net472` and `net48`
  - The targeting of specific runtimes is to allow some more advanced caching technology, based on `System.Reflection.Emit`, to work.  In theory, once `netstandard2.1` is here, 
these will be able to be removed in favour of simply targeting the `2.0` and `2.1` flavours of .Net Standard.
- Major performance improvements:
  - Enumerables +2500%
  - Constructor injection up +368%
  - Compilation speed has also been improved.
- Scopes are now always present ([#89](https://github.com/ZolutionSoftware/Rezolver/issues/89))
  - But, by default, the `Container` class won't support instance tracking or explicitly scoped registrations. You need to create a scope to do this, unless...
  - You use `ScopedContainer` instead, which creates its own 'root' disposable scope on creation.  This will be used by default by Asp.Net Core)
- Got rid of a whole bunch of interfaces in favour of concrete classes with non-virtual (where possible) methods ([#90](https://github.com/ZolutionSoftware/Rezolver/issues/90)):
  - `IResolveContext` -> `ResolveContext`
  - `IContainer` -> `Container`
  - `IContainerScope` -> `ContainerScope`
  - `ICompiledTarget` -> retired for factory delegates, `Rezolver.IInstanceProvider` and `Rezolver.IInstanceProvider<T>`
- Got rid of a bunch of `Resolve` extension methods in favour of concrete methods on the types
- New Container/Scope/Context behaviour:
  - `IServiceProvider` is now explicitly implemented by all three of `Container`, `ContainerScope` and `ResolveContext`
  - All three also have their own `Resolve` method implementations, the behaviour of which is slightly different for each:
    - All `Container` instances have a `Scope`
      - The default root scope used by `Container` doesn't track instances, only child scopes
      - `ScopedContainer` should be used if you want the root scope to track instances (this is the default for Asp.Net Core and Generic Host integration)
      - Resolving through a `Container` uses the scope on the `ResolveContext`, but defaults to its own `Scope` when calling the `Resolve` methods which only take a type
    - All `ContainerScope` instances have a `Container`
      - Resolving through a `Scope` routes the call to its own `Container`, but with a `ResolveContext` which fixes the `Scope` to that scope.
    - `ResolveContext` now only has a `Scope` (the `Container` property just proxies the Container from the scope)
    - Resolving through a `ResolveContext` routes the call to its own `Scope` (and therefore `Container`)
- `OverridingContainer` renamed to `ChildContainer` ([#87](https://github.com/ZolutionSoftware/Rezolver/issues/87))
  - Constructor now no longer accepts an `IRootTargetContainer` on creation.  To register new services, you create a new instance and then register into it via its implementation of `IRootTargetContainer`
- `OverridingTargetContainer` renamed to `ChildTargetContainer` ([#88](https://github.com/ZolutionSoftware/Rezolver/issues/88))


## 1.4.0

- Added [Generic Host Support](http://rezolver.co.uk/developers/docs/nuget-packages/rezolver.microsoft.extensions.hosting.html)
- Added [Autofactory Injection](http://rezolver.co.uk/developers/docs/autofactories.html)
- Added [Automatic `Lazy<T>` Injection](http://rezolver.co.uk/developers/docs/lazy.html)
- Fixed a [critical bug (#77)](https://github.com/ZolutionSoftware/Rezolver/issues/77) for Release-build Asp.Net Core Apps 

## 1.3.4

Primary purpose of this release was to have a build that's been built and tested explicitly against the .Net Core 2.1
runtime and Asp.Net Core 2.1.

- Added [SourceLink](https://github.com/dotnet/sourcelink) Support
- Core library now targets `.NetStandard1.1`, `.NetStandard2.0` and `net45` (Removed `net461` as it was pointless)

### Asp.Net Core 2.1 Integration

- Rezolver.Microsoft.AspNetCore.Hosting updated dependency to Asp.Net Core 2.1
- Rezolver.Microsoft.Extensions.DependencyInjection updated dependency to Asp.Net Core 2.1

---

## 1.3.3

> As always, for the full list of changes, check out [the version history](https://github.com/ZolutionSoftware/Rezolver/releases).

Bugfixes for generics handling, specifically:

- Singletons matched contravariantly or covariantly did not honour the pattern
- Registering `Foo<T, U> : IFoo<IBar<T, U>>` against `<IFoo<IBar<,>>` caused an `IndexOutOfRangeException` 

---

## 1.3.2

- Generic covariance
- Mixed variance
- Enumerable Projections
- List member bindings (collection initialisation)
- Selection of constructors on open generics
- Fluent API to build per-member bindings

---

## 1.3 & 1.3.1

- Support for Asp.Net Core 2.0
- Contravariance
- Decorator Delegates
- List, Collection and Array injection
- ... Plus loads of bug fixes and other enhancements :)

All these features are documented [on our website](http://rezolver.co.uk).

---

# Performance

Rezolver has now been incorporated into @DanielPalme's [excellent IOCPerformance benchmark](http://www.palmmedia.de/Blog/2011/8/30/ioc-container-benchmark-performance-comparison).

Read [our own notes on Rezolver's performance in this benchmark](http://rezolver.co.uk/developers/docs/benchmarks.html), which provide some context on the areas where it can be improved.

> Hint: Unless you're creating child containers with additional registrations, Rezolver's performance is right up there with the fastest.

# Contributing

Feel free to fork this repo, build from the source, and submit pull requests for new functionality or bugfixes!
