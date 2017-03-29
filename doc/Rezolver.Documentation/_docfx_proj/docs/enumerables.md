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

## Empty enumerable

Here's an example where we only register the service we're going to create, which has a single constructor that requires
an `IEnumerable<IMyService>`.

First, the type `RequiresEnumerableOfServices`:

[!code-csharp[RequiresEnumerableOfServices.cs](../../../../test/Rezolver.Tests.Examples/Types/RequiresEnumerableOfServices.cs#example)]

And then the test:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example1)]

## Same `ITarget` type

Here, we register each type one after another:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example2)]

## Mixed `ITarget` types

This time, we're mixing it up a bit by registering multiple different target types (a constructor target, a delegate
target and an object target), *and* we're also using a delegate to create the `RequiresEnumerableOfServices` just
to show that the enumerable can be injected into a delegate as you'd expect:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example3)]

## Mixed lifetimes

When registering targets, you have three lifetimes at your disposal:

- Transient (a new object created for every @Rezolver.IContainer.Resolve* call)
- Singleton (only one object is ever created)
- Scoped (one object created per <xref:Rezolver.IContainerScope>)

> [!NOTE]
> Of course, the @Rezolver.Targets.ObjectTarget (see [objects as services](objects.md)) is technically a singleton, also,
> but that's because it wraps a constant reference supplied by you.

If you register multiple targets for the same type, **_and_** those targets have different lifetimes, then those lifetimes are
honoured if the container injects an `IEnumerable<>`.

The following example registers three implementations for `IMyService` again which will appear in the enumerables in the 
following positions:

- `[0]`: A singleton 
- `[1]`: A scoped object
- `[2]`: A transient

It then resolves `IEnumerable<IMyService>` multiple times - twice from the root container (which
is a @Rezolver.ScopedContainer so that the scoped `MyService2` registration has a scope to 'live' in) and then
twice again from a child scope:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example4)]

To summarise:

- `[0]` is *always* the same reference
- `[1]` is created once per enclosing scope (remember - the container itself is a scope in this example)
- `[2]` is created once per call

# In progress - (mixed lifetimes next)

This documentation is incomplete - please keep coming back to learn more.