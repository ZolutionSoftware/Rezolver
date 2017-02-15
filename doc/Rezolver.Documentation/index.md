# Rezolver

Welcome to the home of Rezolver - here you'll find guides, walkthroughs, deep-dives and 
reference for all of the APIs and functionality in the Rezolver library.

## What is Rezolver?

Rezolver is an [open source](https://github.com/ZolutionSoftware/Rezolver) IOC container that, out of the box, supports all the standard 
features supported by most =other popular IOC containers out there.  It's also highly extensible - with practically the entire stack open
to extension immediately after referencing the core library.

It's been built specifically with the .Net Core framework in mind, supporting the NetStandard1.1 profile for (almost!) maximum portability

##Features

- 'Intelligent' constructor discovery based on registered services
- Open generics (with specific closed generics taking precedence)
- Decorators (non-generic and generic)
- Enumerables (empty enumerables returned by default)
- Child containers (overriding registrations in one container with those of another)
- Lifetime scoping (incl. child lifetimes)
- Delegates and Expressions as factories, with argument injection
- Singletons
- Pre-built objects
- Scoped objects (i.e. 'singleton per scope')
- Extensible compiler framework
  - Expression tree compiler used by default
- Integration with Microsoft.Extensions.DependencyInjection via nuget packages
- Extensible configuration framework *(still in development)*
  - Json configuration *(still in development)*

## Quick Links

- [Getting Started](rezolver-usage/)
<!--- [Nuget Packages](rezolver-usage/nuget-packages/) -->
- [API Reference](rezolver-api/)

### Please note - this documentation is a work in progress!

Rezolver is currently undergoing an API change which has invalidated some of the documentation we had before.

Once everything is built and re-tested, all the documentation will be added ASAP to provide in-depth guides and tutorials
which are then guaranteed to stand the test of time!

So, keep coming back!  We'll get there :)

##On Github

If you're the kind of person who likes to learn from seeing the code - then check out the [Rezolver repo on Github](https://github.com/ZolutionSoftware/Rezolver),
there are some example applications on there, and you can look at the unit tests to get an idea of how to create and configure containers.