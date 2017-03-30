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

* * *

# Advanced examples

On to generics and decorators, now...

## Enumerables of open generics

You can also register multiple open generics of the same type (e.g. `IFoo<>`) and then resolve an enumerable of 
`IFoo<Bar>`, and the container will create an enumerable containing an object for each open generic registration:

[!code-csharp[UsesAnyService.cs](../../../../test/Rezolver.Tests.Examples/Types/UsesAnyService.cs#example)]
[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example5)]

> [!NOTE]
> When working with generics, the enumerable handler searches for the first generic registration which has least
> one @Rezolver.ITarget whose @Rezolver.ITarget.UseFallback is `false` - searching from least generic to most
> generic (e.g. `Foo<Bar>` is less generic than `Foo<>`).
>
> So if you request an `IEnumerable<Foo<Bar>>`, targets are first sought for `Foo<Bar>` and, if none are found,
> it then searches for `Foo<>`.  The side effect of this is that your enumerable will always only contain objects
> produced from targets registered against the most-specific generic type that's applicable for the type requested.  The
> next example expands on this.

## Mixing open/closed generics

Let's say that we have one open generic registration for `IUsesAnyService<>` to be used as a catch-all, but that
when `IMyService` is used, we have two types that we want to use instead.

In this case, we still have an open generic registration, but we want it to be superseded for certain generic types by two
specialised registrations where the inner generic type argument is known.

Given these extra generic types:

[!code-csharp[UsesIMyService.cs](../../../../test/Rezolver.Tests.Examples/Types/UsesIMyService.cs#example)]

We can do this:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example6)]

So, as soon as we want an `IEnumerable<IUsesAnyService<IMyService>>`, the enumerable will use *only* the
two explicit registrations made against the closed generic type `IUsesAnyService<IMyService>`, but if we
request any other type, we only get items produced by registrations against the open generic `IUsesAnyService<>`.

> [!WARNING]
> There is currently *no* way to fall back on an open generic registration for given generic once you make a 
> registration for a closed generic.  The framework might, however, be extended to allow you to make an explicit 
> registration which instructs the container to fall back to a more generic registration and include any results from
> that in the enumerable.

It doesn't matter what order you register the open generics and closed generics - the logic is applied on a type-by-type
basis; but the order of an individual enumerable of a given type is, however, governed by the order of registration.

## Decorators and Enumerables

> [!NOTE]
> At the time of writing, the [decorators topic](decorators.md) has not been written, so this is preview of how to 
> use decorators as well as how they work in enumerables.

Decorators that have been registered against the element type of an enumerable will be applied to all instances 
that the container produces for the enumerable.  This also applies to stacked decorators (where multiple decorators are
applied on top of one other).

So, we have two decorator types for `IMyService`:

[!code-csharp[MyServiceDecorators.cs](../../../../test/Rezolver.Tests.Examples/Types/MyServiceDecorators.cs#example)]

And in this example we'll have one of those decorators being used to decorate three registrations:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example7)]

> [!NOTE]
> As the first comment states - you cannot currently call any `RegisterDecorator` extension method via an @Rezolver.IContainer,
> or, more importantly, a @Rezolver.ContainerBase derivative.
> 
> This is because the extensions require an `@Rezolver.ITargetContainerOwner`, which is not implemented by these types.
> This will change with the 1.2 release, though (see [Issue #25](https://github.com/ZolutionSoftware/Rezolver/issues/25)).

This example only shows one decorator - in reality you might need to use more than one - and Rezolver is happy to let you.

### In progress

This documentation is incomplete - please keep coming back to learn more.