Rezolver
========

Zolution Software's own portable IOC container.

In this branch, we are building a vNext-only build (VS 2015) that should still be compatible with earlier
framework profiles via the new multi-targeting support in vNext.

The process is taking a while as we cope with alterationis to the pre-release framework and time pressures elsewhere!

Dev branch notes
================

Next test suite to port - LazyTargetTests.

Features
--------

- Automatic constructor discovery (most greedy)
- Use specific constructor with specific argument bindings
- Open generics
- Containers overriding other containers
- Lifetime scoping
- Custom expressions
- JSON Configuration (via [Rezolver.Configuration.Json](TODO THIS LINK (nuget)))
- (Xml Configuration is planned)
- Asp vNext Support (in development)
- Extensible API (both for binding objects and for configuration)
- Plus much more besides :)

More in-depth Documentation for Rezolver is available [on our website](http://www.zolution.co.uk/Rezolver),
it's not finished yet, but there's a lot to get you going on with.

It's a good idea to look at the source code for the [numerous unit tests](TODO THIS LINK).