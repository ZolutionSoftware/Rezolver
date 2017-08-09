# Lazy vs. Eager Enumerables

As described in the [main enumerables topic](../enumerables.md), Rezolver builds lazy enumerables by default, but can also
generate eager enumerables.  This topic describes the difference between the two and also shows how you can configure your
container to build enumerables differently for different types.

# Background

## What is a Lazy enumerable?

A lazy enumerable is simply one which produces results *as you enumerate it* rather than being backed by an array or collection
which already contains the objects.

As an example, the .Net function @System.Linq.Enumerable.Range* function generates a lazy enumerable which, when enumerated,
provides a range of *n* integer values starting from a given lower bound:

```cs
// an enumerable which starts at 0 and keeps returning values up to, and including, 9
var enumerable = Enumerable.Range(0, 10);

// an enumerable which starts at 100 and keeps returning values up to, and including, 199
var enumerable2 = Enumerable.Range(100, 100);
```

When you `foreach` these enumerables (or call the `.GetEnumerator()`) function, these lazy enumerables will
continue generating new results (via the `IEnumerator<int>.MoveNext` implementation) until *n* results have been returned.

Crucially, if you then `foreach` these enumerables a second time, you will get a new sequence - with the code that generates each
result being executed again each time.  The side effect of this being that if you do something like this:

```cs
var enumerable = Enumerable.Range(0, 10).Select(i => new Foo(i));

foreach(var o in enumerable)
{

}

//and then a second time

foreach(var o in enumerable)
{

}
```

The objects produced by the `Select` method are *always* new instances whenever you enumerate - so in the above example
**20** new instances of `Foo` are created - *not* 10.

## What is an eager enumerable?

Typically, an eager enumerable is one which has been created from a data structure which already contains all the objects which
will be enumerated.  The simplest example of this being an array:

```cs
var a = new[] { new Foo(), new Foo(), new Foo() };

foreach(var o in a)
{

}

// enumerating the same instances again

foreach(var o in a)
{

}
```

In this case, all the objects produced by the enumerable are already in memory, and the enumerator is merely walking an array,
linked list or whatever.

> [!TIP]
> Most .Net developers will already be familiar with the idea of 'realising' an enumerable to force it to become 'eager', using the
> @System.Linq.Enumerable.ToArray* or @System.Linq.Enumerable.ToList* extension methods.  These functions simply enumerate their
> input enumerable once, storing the results into an array/list or similar structure.

* * *
# Examples

## Lazy enumerable

To demonstrate Rezolver's default behaviour of creating lazy enumerables, we'll get it to construct an enumerable of the
first of these two (contrived) classes:

[!code-csharp[CallsYouBackOnCreate.cs](../../../../../test/Rezolver.Tests.Examples/Types/CallsYouBackOnCreate.cs#example)]

The example registers an `Action<CallsYouBackOnCreate>` callback, in addition to three entries for the `CallsYouBackOnCreate`
type, which increments a local counter - meaning that we can monitor how many times the constructor for that type is called.

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example9)]

There are a few takeaways from this example:

- Immediately after resolving an instance of the enumerable, we assert that no instances of `CallsYouBackOnCreate` have been created
- The two `foreach` loops verify that each instance produced from the enumerable is created *just-in-time*
- The instances produced by the enumerable are not retained by the underlying enumerator.  New instances are being created
every time enumeration occurs.

In the context of this example, clearly it seems odd that we'd want an enumerable that keeps generating new instances every
time we enumerate it.  In the real-world, however, most of the time we inject enumerables, the receiving object will 
enumerate once.  Equally, if the receiver does expect to enumerate multiple times and wants to *guarantee* that only the 
correct number of items will ever be produced exactly once, then a simply `.ToArray()` call will do the trick.

## Eager enumerable (global)

As with many other Rezolver behaviours, lazy/eager enumerable creation is controlled by an option which can be set 
on an @Rezolver.ITargetContainer either globally (which affects all enumerables) or on a per-type basis (see next).

The option that controls this behaviour is @Rezolver.Options.LazyEnumerables - which of course has a 
@Rezolver.Options.LazyEnumerables.Default value of `true` (thus when it's not explicitly set on a target container, lazy
enumerables are enabled).

This example is very similar to the [lazy enumerable example above](#lazy-enumerable) - but this time we're going to 
disable all lazy enumerables on the @Rezolver.ITargetContainer which underpins the container:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example10)]

In contrast to the previous example, then, note that all the instances within the enumerable are created as soon as
the container returns it to the caller (or injects it as a constructor/delegate argument) - and also note that the 
number of instances created doesn't change on repeated enumerations.

> [!WARNING]
> Do not confuse this behaviour with that of a singleton.  If we were to fetch two eager enumerables from the container,
> then both would create their instances up-front, independent of each other.

## Eager enumerable (per-service)

Obviously, being able to switch *all* enumerables to eager is likely to be a bit of a sledgehammer in many cases.  So
Rezolver also lets you control the production of lazy or eager enumerables on a per-type basis.

To do this, when setting the @Rezolver.Options.LazyEnumerables options to `false`, you also include the type of service
whose enumerables are to be affected (see the @Rezolver.OptionsTargetContainerExtensions.SetOption* overload for more):

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example11)]

* * *

> [!TIP]
> Remember - per-service options can also be defined for open generics and for common bases or interfaces of a given
> type.
> 
> This means you can set it for `BaseClass` and it will apply to any type derived from it (unless you also set it 
> specifically for that derived type).  Equally, setting it for the open generic `IGeneric<>` will apply to any 
> closed generic built from it (again - unless you expressly set it differently for that closed generic).

> [!TIP]
> Although it should be obvious by now - you can very easily invert the lazy enumerable behaviour to opt-in instead of
> opt-out by setting the @Rezolver.Options.LazyEnumerables to `false` globally, which then allows you to set it to
> `true` for specific services.

# Benefits of being lazy

The reasons why Rezolver defaults to lazy enumerables are the same as why you'd use them in any .Net project: 

- Lazy enumerables are generally faster to create than an eager enumerable containing the same results, because
typically the enumerable just wraps a function.  The eager version of the same enumerable will still use that function 
to produce the instances, but must also execute it and capture the results on creation.
- Lazy enumerables generally have a lower memory overhead.  The pathological case (and, admittedly, not necessarily 
relevant in the IOC world) is an enumerable that produces a theoretically infinite series.  Such enumerables can
clearly never be evaluated eagerly, because they require more storage than the machine (or in most theories of reality,
the universe itself!) has available.  The more realistic case is where you might have 100s of registrations for a given 
type, all the implementations of which consume big chunks of memory - creating an eager enumerable of these will 
instantly grab a chunk of memory for those instances, whereas a lazily evaluated enumerable would not.

That said, there are of course reasons why you'd still want to use an eager enumerable.  Perhaps the objects are singletons
and/or are 'hard' to create - in which case you might as well expend a bit of extra effort up front to create them so
you don't have to do it later.