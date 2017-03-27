# Enumerables

By default, a @Rezolver.TargetContainer (the default @Rezolver.ITargetContainer used by all containers in the 
Rezolver framework) comes with a behaviour enabled which allows any @Rezolver.ContainerBase derivative to resolve
an `IEnumerable<Service>`.

The contents of this enumerable will depend on how many times the @Rezolver.ITargetContainer.Register*
method has been called against the target type `Service`:

- If no target has been registered against the type, then the enumerable will be empty
- Otherwise, the enumerable will contain instances obtained from each @Rezolver.ITarget that was registered
against that type, in the order they were registered.

> [!NOTE]
> Automatic resolving of enumerables is a configurable behaviour which can currently be disabled when creating 
> a @Rezolver.TargetContainer via its constructor.  It can also be explicitly opted-in (if it has been disabled)
> by calling the @Rezolver.EnumerableTargetBuilderExtensions.EnableEnumerableResolving* extension method.
> 
> Disabling the behaviour through the constructor might be removed in a future version, as we intend to move to a 
> configuration callback-based mechanism for configuring containers.

You are not restricted in the targets you use to produce instances for an enumerable, and each one can have its
own lifetime (scoped/singleton etc).

# Basic examples

## Empty enumerable

Here's an example where we only register the service we're going to create, which has a single constructor that requires
an `IEnumerable<IMyService>`.

First, the type `RequiresEnumerableOfServices`:

[!code-csharp[RequiresEnumerableOfServices.cs](../../../../test/Rezolver.Tests.Examples/Types/RequiresEnumerableOfServices.cs#example)]

And then the test:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example1)]

## Enumerables of ConstructorTargets

Here, we actually register each type one after another:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example2)]

#In progress!

This documentation is incomplete - please keep coming back to learn more.