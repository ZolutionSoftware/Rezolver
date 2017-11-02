# Automatic Enumerable Injection

By default, a @Rezolver.TargetContainer (the default @Rezolver.ITargetContainer used by all containers in the 
Rezolver framework) is configured to allow any @Rezolver.ContainerBase derivative to resolve an `IEnumerable<Service>`.

The contents of this enumerable will depend on how many times the @Rezolver.ITargetContainer.Register*
method has been called against the target type `Service`:

- If no target has been registered against the type, then the enumerable will be empty
- Otherwise, the enumerable will contain instances obtained from each @Rezolver.ITarget that was registered
against that type, in the order they were registered.

> [!NOTE]
> The functionality described here depends on two target container options: @Rezolver.Options.AllowMultiple 
> and @Rezolver.Options.EnableEnumerableInjection - which are both configured to be equivalent to `true` by 
> default for all @Rezolver.ITargetContainer instances.

You are not restricted in the targets you use to produce instances for an enumerable (e.g. @Rezolver.Targets.ObjectTarget,
@Rezolver.Targets.ConstructorTarget or <xref:Rezolver.Targets.DelegateTarget>), and each one can have its
own lifetime (scoped/singleton etc).

> [!TIP]
> By default, Rezolver will build 'lazy' enumerables, but can be configured to build 'eager' enumerables - for
> more on this, read the topic [Lazy vs Eager Enumerables](enumerables/lazy-vs-eager.md)

> [!WARNING]
> In an Asp.Net Core 2.0 application, however, eager enumerables are the default until the fix for 
> [this issue in the aspnet/DependencyInjection repo](https://github.com/aspnet/DependencyInjection/issues/589)
> has been pushed to an official package release.

## Resolving enumerables

To resolve an enumerable from a container or through a scope, you can simply use `IEnumerable<Foo>` as the input
type for a @Rezolver.IContainer.Resolve* call:

```cs
var enumerable = container.Resolve<IEnumerable<Foo>>();
```

However, the `ResolveMany` extension methods (see <xref:Rezolver.ContainerResolveExtensions.ResolveMany*>
and <xref:Rezolver.ContainerScopeResolveExtensions.ResolveMany*>) provide a shortcut which allow you to pass
just the element type of the enumerable to reduce the 'angle-bracket percent' of code which directly resolves
enumerables (which of course you won't be doing because you're not using 'service location' are you!? :wink: ):

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

***

## Automatic Covariance

Since the `T` type parameter in `IEnumerable<T>` is a covariant, Rezolver automatically resolves all types
which are reference compatible with the `T` that you specify.  So, resolving an `IEnumerable<IFoo>` will 
result in an enumerable containing *all* objects produced from registrations made against types which 
implement `IFoo` (the registrations do not need to have been made explicitly against the `IFoo` interface).

A concrete example of this can be found in the [section on covariance](variance/covariance.md#enumerables).

***

## Decorators and Enumerables

[Decorators](decorators.md) that have been registered against the **element type** of an enumerable will be applied to all 
instances that the container produces for the enumerable.  This also applies to stacked decorators (where multiple 
decorators are applied on top of one other).

So, we have two decorator types for `IMyService`:

[!code-csharp[MyServiceDecorators.cs](../../../../test/Rezolver.Tests.Examples/Types/MyServiceDecorators.cs#example)]

And in this example we'll have one of those decorators being used to decorate three registrations:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example7)]

If more decorators were added, of course - then each element would be 're-decorated' accordingly.

> [!TIP]
> You can also decorate instances of`IEnumerable<T>`, `IList<T>`, `ICollection<T>` - plus any other supported
> collection types.  [Read about this now](decorators/collections.md).

***

## Explicit `IEnumerable<T>` registrations

Although you get `IEnumerable<T>` handling automatically when the @Rezolver.Configuration.InjectEnumerables configuration 
is applied (and not also disabled by the @Rezolver.Options.EnumerableInjection option being set to `false`), it doesn't 
prevent you from manually adding registrations for specific `IEnumerable<>` types which override the default behaviour.

For example, let's say that you have two registrations for services which share a common interface, but they have only
been registered against their concrete type (perhaps it's historical code you can't risk changing). Your code 
now wants an enumerable of that common interface.  Well, assuming you know what the specific registrations are - you can
use delegate registrations (note, there are *lots* of ways, this is just the most illustrative):

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example8)]

> [!WARNING]
> As soon as you create a manual registration for a particular `IEnumerable<T>` type, Rezolver no longer has any control
> over how that particular enumerable is produced - so none of the enumerable-related options demonstrated in this documentation 
> will be honoured.

***

## Disabling Enumerable Injection

As with much of the built-in functionality in Rezolver, you can control whether enumerables are automatically 
built or not through the use of options and configuration.

In the case of enumerable injection, it is a feature that is enabled by an @Rezolver.ITargetContainerConfig
that is added by default to a set of configuration objects in the @Rezolver.TargetContainer.DefaultConfig 
collection which is applied, by default, to all new instances of the @Rezolver.TargetContainer class (which
is, in turn, used by both the main container types - @Rezolver.Container and <xref:Rezolver.ScopedContainer>).

This means that, to disable it, you have to either remove that config (it's the 
@Rezolver.Configuration.InjectEnumerables configuration singleton) from the collection, or set the 
@Rezolver.Options.EnableEnumerableInjection option to `false` on the target container before the configuration
is applied.

You also have another choice, in that you can either do this directly on the @Rezolver.TargetContainer.DefaultConfig
collection, which will affect *all* instances of @Rezolver.TargetContainer, or you can 
@Rezolver.CombinedTargetContainerConfig.Clone* the collection and apply it only to a single target container.

It's the second of these options that the example shows, below:

[!code-csharp[EnumerableExamples.cs](../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example12)]

Notice that we're explicitly creating a @Rezolver.TargetContainer and passing a cloned config to it; passing
the result to the @Rezolver.Container constructor.

> [!TIP]
> Many other examples throughout this documentation show options simply being set directly on the container.
> In those cases, it's because the options are used by logic within Rezolver to make decisions.
> Enumerable injection, however, is configured at the point a new @Rezolver.TargetContainer is *created*, 
> because the configuration is applied by one of its constructors; hence the different approach is required in
> order to ensure the option is set *before* the configuration gets applied.  The 
> @Rezolver.Configuration.InjectEnumerables configuration looks for the @Rezolver.Options.EnableEnumerableInjection
> option and, if it's `false` it disables itself.

> [!WARNING]
> If you disable automatic enumerable injection then the other 
> [automatic collection-type injection behaviour](arrays-lists-collections/index.md) will only work when you 
> explicitly register the correct `IEnumerable<T>` for it.

# Order of enumerables

> [!NOTE]
> This is new in 1.3.1

The order that an enumerable returns its items is **always** equal to the order in which the underlying registrations
were made.

* * *

# Next steps
- Read about how Rezolver handles building [enumerables of generics](enumerables/generics.md) from open and closed
generic registrations.
- Learn about Rezolver's support for [lazy and eager enumerables](enumerables/lazy-vs-eager.md) (note: all auto-generated enumerables are lazily 
evaluated by default)
- Rezolver also supports [arrays, lists and collection injection](arrays-lists-collections/index.md)