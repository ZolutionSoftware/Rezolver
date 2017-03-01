# Rezolver Nuget Package

[See package page on nuget](https://www.nuget.org/packages/Rezolver).

The core nuget package for using Rezolver in your class library or application, includes:

  - All the core abstractions and classes required to create @Rezolver.TargetContainer and @Rezolver.Container objects.
  - The core @Rezolver.ITarget implementations which allow you to create/retrieve objects using all
the standard techniques documented here (constructor injection, enumerables, delegates and expressions etc)
from the get-go.
  - The expression compiler which builds bespoke factories for each service you resolve in your container

Simply add the package, add an `using`/`import` for the `Rezolver` namespace and you're all set.