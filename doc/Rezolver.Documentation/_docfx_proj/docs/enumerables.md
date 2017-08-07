# Enumerables

By default, a @Rezolver.TargetContainer (the default @Rezolver.ITargetContainer used by all containers in the 
Rezolver framework) is configured to allow any @Rezolver.ContainerBase derivative to resolve an `IEnumerable<Service>`.

The contents of this enumerable will depend on how many times the @Rezolver.ITargetContainer.Register*
method has been called against the target type `Service`:

- If no target has been registered against the type, then the enumerable will be empty
- Otherwise, the enumerable will contain instances obtained from each @Rezolver.ITarget that was registered
against that type, in the order they were registered.

> [!NOTE]
> The functionality described here depends on two target container options: @Rezolver.Options.AllowMultiple 
> and @Rezolver.Options.EnumerableInjection - which are both configured to be equivalent to `true` by default
> for all @Rezolver.ITargetContainer instances.

You are not restricted in the targets you use to produce instances for an enumerable (e.g. @Rezolver.Targets.ObjectTarget,
@Rezolver.Targets.ConstructorTarget or @Rezolver.Targets.DelegateTarget), and each one can have its
own lifetime (scoped/singleton etc).

> [!TIP]
> By default, Rezolver will build 'lazy' enumerables, but can be configured to build 'eager' enumerables - for
> more on this, read the topic [Lazy vs Eager Enumerables](enumerables/lazy-vs-eager.md)

## Resolving enumerables

To resolve an enumerable from a container or through a scope, you can simply use `IEnumerable<Foo>` as the input
type for a @Rezolver.IContainer.Resolve* call:

```cs
var enumerable = container.Resolve<IEnumerable<Foo>>();
```

However, the `ResolveMany` extension method (see <xref:Rezolver.ContainerResolveExtensions.ResolveMany*>
and <xref:Rezolver.ContainerScopeResolveExtensions.ResolveMany*>) provide a shortut which allow you to pass
just the element type of the enumerable to reduce the 'angle-bracket percent' of code which directly resolves
enumerables:

```cs
// this is equivalent to resolving IEnumerable<Foo>
var enumerable2 = container.ResolveMany<Foo>();
```

Most, if not all, the examples in this section use this shortcut - but it's worth noting that the two methods are
absolutely equivalent.

## Empty enumerable

By default, you don't need to register anything against a particular type in order to be able to inject an enumerable
of that type.  If there are no registrations, then an empty enumerable will be injected instead.

Here's an example where we only register the service we're going to create, which has a single constructor that requires
an `IEnumerable<IMyService>`.

First, the type `RequiresEnumerableOfServices`:

[!code-csharp[RequiresEnumerableOfServices.cs](../../../../test/Rezolver.Tests.Examples/Types/RequiresEnumerableOfServices.cs#example)]

And then the test:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example1)]

## Multiple new objects

Here, we associate three types (created by constructor injection) to a common service type, which are all then included
in the auto-injected enumerable:

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

If more decorators were added, of course - then each element would be 're-decorated' accordingly.

## Explicit `IEnumerable<T>` registrations

Although you get `IEnumerable<T>` handling automatically, it doesn't prevent you from manually adding registrations
which override the default behaviour.

For example, let's say that you have two registrations for services which share a common interface, but they have only
been registered against their concrete type (perhaps it's historical code you can't risk changing). Your code 
now wants an enumerable of that common interface.  Well, assuming you know what the specific registrations are - you can
use delegate registrations (note, there are *lots* of ways, this is just the most illustrative):

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example8)]

* * *

# Next steps
- Recommend going to take (another) look at [decorators](decorators.md) - although, at the time of writing it might 
not be written yet!
- The [generic constructor injection](constructor-injection/generics.md) documentation contains more useful guidance 
about open generics etc.