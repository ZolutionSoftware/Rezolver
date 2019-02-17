# Autofactories

Framework authors frequently need to be able to create instances of other developers' objects on-demand.  When an application is 
configured to run via an IOC container, this often means one of two things:

- Taking a dependency on an instance of the IOC container
  - *This can be achieved in Rezolver either by injecting an instance of @Rezolver.IResolveContext or <xref:System.IServiceProvider>*
- Taking a dependency on a factory delegate

The second of these two approaches is something that can be set up in Rezolver manually as follows:

[!code-csharp[DelegateExamples.cs](../../../../test/Rezolver.Tests.Examples/DelegateExamples.cs#example13)]

This creates a registration that will provide a general-purpose factory to resolve any type - in a way that honours the current scope - 
back through the container, but without exposing the container to the calling code.

However, this isn't terribly nice to work with at runtime (the factory delegate is far too 'open' - most of the time we'd want to 
limit the types that could be passed) and if we want to have more focused factories (e.g. `Func<MyService>`) then we would need 
a registration like this for each individual type that we'd want to create instances of - which defeats the point.

# Examples

## Basic functionality

Starting with Rezolver 1.4, however, Rezolver makes this kind of registration trivial, and also gives you some extra power.

The @Rezolver.AutoFactoryRegistrationExtensions.RegisterAutoFunc* overload provides a simple way to enable Rezolver to automatically
inject a @System.Func`1 or one of its related types, using its own registrations as the source of the object that is produced by that
factory when it is executed.

[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example1)]

It also supports enumerables - either as the created type:

[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example2)]

Or to resolve an enumerable of factories

[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example3)]

All of the above works for custom delegate types, too (so long as they have a non-`void` return type), via the 
@Rezolver.AutoFactoryRegistrationExtensions.RegisterAutoFactory* overload:

[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example4a)]
[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example4b)]

## Using arguments for dependencies

So far, all the delegates we've been building are nullary - that is, they have no parameters.  After all, what could Rezolver
possibly do with the arguments that you supply?  Well, the answer is that it can use them to fulfil any dependencies that are
required by any of the registrations used to by the delegate it creates:

[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example5)]

> [!WARNING]
> All parameter types ***must*** be unique otherwise the registration call will fail!

> [!TIP]
> Argument values are matched by ***type*** to dependencies which are being satisfied internally by the @Rezolver.Targets.ResolvedTarget
> when the delegate return type is resolved normally.  Most typically, this means arguments for constructor parameters.
> 
> This overriding of dependencies flows all the way down through all registrations which are used to satisfy a particular
> factory's output type - so if an argument type appears in the constructor of, save, five objects which are all built as part
> of a particular request, then all five constructor calls will receive the argument instead of 

This also works even if no registrations exist for the dependency type - shown below where we omit the registration
of the `IMyService` implementation:

[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example6)]

This functionality is particularly powerful in generic scenarios where you have a component which is responsible for handling
user-supplied objects at runtime that you can't inject via the container.

Again, all of this works with custom delegate types, too.

## Scopes and factories

The objects produced by Autofactories are tied to the scope that created the autofactory:

[!code-csharp[AutoFactoryExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoFactoryExamples.cs#example7)]