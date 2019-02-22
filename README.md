![Rezolver Logo](https://raw.githubusercontent.com/ZolutionSoftware/Rezolver/master/doc/Rezolver.Documentation/Content/rz_square_white_on_orange_256x256.png)

Rezolver
========

Rezolver is a high performance, portable (.NetStandard 1.1 & 2.0), open-source IOC container.

> You can get the [Rezolver Package from Nuget](https://www.nuget.org/packages/Rezolver/), and for more 
> packages, head over to the [packages documentation](http://rezolver.co.uk/developers/docs/nuget-packages/index.html).

For more information, including API reference and developer how-tos, head on over to the 
[Rezolver website](http://rezolver.co.uk).

[Follow us on twitter](https://twitter.com/RezolverIOC) for code or documentation updates/release notifications etc.

[![Work in Progress](https://badge.waffle.io/ZolutionSoftware/Rezolver.png?label=in%20progress&title=In%20Progress)](http://waffle.io/ZolutionSoftware/Rezolver) 
[![Items ready to go](https://badge.waffle.io/ZolutionSoftware/Rezolver.png?label=ready&title=Ready)](http://waffle.io/ZolutionSoftware/Rezolver)

---

# Version Highlights

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
