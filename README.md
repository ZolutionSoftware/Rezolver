Rezolver
========

Zolution Software's own portable IOC container.

Currently building a vNext-only build (VS 2015) that should still be compatible with earlier
framework profiles via the new multi-targeting support in vNext.

Features
--------

- Expression-based for all code generation
- Constructor binding
  - Greedy matching (i.e most parameters - default)
  - Specific constructor with specific argument bindings
  - Just-in-time matching based on available services (in development)
- Open generics
- Child Containers
  - Override and extend parent containers even if they're compiled
  - Named Containers (with fallback)
- Lifetime scoping
- Custom expressions
  - With explicit 'resolve' expression so you can call back into the container for arguments in your expression
- Support for new Microsoft.Extensions.DependencyInjection framework
  - One test failing at present
- Configuration framework (needs some more work, but can be used)
  - JSON Configuration
  - Xml Configuration (planned)
  - Your own configuration
- Extensible API
  - Provide your own Expression-building targets for registrations
  - Override the compiler
  - Override all container and scope behaviour
- All features are unit tested
  - Core library (Rezolver.dll) > 75% coverage.
  - More tests will be added as issues raised and features added
- Plus much more besides :)

Some more in-depth, but in-progress (and possibly wrong), documentation for Rezolver is available [on our website](http://www.zolution.co.uk/Rezolver).

Some of the typenames have changed since it was first written.  I'll be fixing that.