# Enumerables of Generics

Now you've seen how Rezolver's [automatic enumerable injection works](../enumerables.md), let's move on to how 
generics work in the context of enumerable injection.

## Open Generics

You can register multiple open generics of the same type (e.g. `IFoo<>`), resolve an enumerable of 
`IFoo<Bar>`, and the container will create an enumerable containing an object for each open generic registration:

[!code-csharp[UsesAnyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/UsesAnyService.cs#example)]
[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example5)]

***

## Constrained Generics

When one or more constrained open generic types are registered against an open generic type, Rezolver will only
include results from registrations whose target type's constraints are met.

[!code-csharp[ConstrainedGenerics.cs](../../../../../test/Rezolver.Tests.Examples/Types/ConstrainedGenerics.cs#example)]
[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example5b)]

***

## Mixing open/closed generics

In many applications which use DI and generics, you often have 'common' open generic registrations, such as
those in the [first example above](#open-generics), and then specific registrations for one or more 
well-known *closed* generics.

When resolving a single instance for a given generic, Rezolver will of course select the newest *most specific* 
registration it can find.  This means that if you have registrations for both `IFoo<>` and `IFoo<Bar>` and 
request an instance of `IFoo<Bar>`, then the second registration is used.

> [!TIP]
> Rezolver's generic type matching works on an inside-out basis, so if you request `IGeneric<IFoo<IBar>>` from
> the container, it searches for registrations for the following types, in this order:
> - `IGeneric<IFoo<IBar>>`
> - `IGeneric<IFoo<>>`
> - `IGeneric<>`
> 
> This search order is the same regardless of the order the registrations are made.  So, if an 
> `IGeneric<IFoo<IBar>>` was the last to be added to the target container, it's still returned first.

If, however, you request an `IEnumerable<IFoo<Bar>>`, what should Rezolver return?

The answer is: it depends on your specific need.  Sometimes you might want all applicable objects to be returned
(the default behaviour), and sometimes you might only want the best-matched registrations (i.e. the first batch
of registrations found for one of the generic types being searched, as per the above Tip callout) to be used.

The good news is that it's easy to tell Rezolver which of the behaviours you want it to use - both globally and
on a per-service basis.

### All possible generics

> [!NOTE]
> The new default functionality described here represents a breaking change from 1.2.
> 
> The old behaviour can be re-enabled by setting the @Rezolver.Options.FetchAllMatchingGenerics option 
> to `false`, as shown in the next example.

Let's say that we have one open generic registration for `IUsesAnyService<>` to be used as a catch-all, but that
we also have specific implementations we want to use for `IUsesAnyService<IMyService>` in addition to the catch-all.

In this case, we simply need to add one or more registration(s) for the **_concrete_** generic type in addition
to the open generic registrations, and Rezolver will intelligently select all the generics that apply when 
building its enumerable.

So, given these extra generic types:

[!code-csharp[UsesIMyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/UsesIMyService.cs#example)]

We can do this:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example6)]

In the simplest terms, what we're seeing here is the container walking through all the possible generic service types
that could apply for the requested type, and returning all the instances produced by all those registrations, in
the order that those registrations were made.  So, if an open generic registration is made after a series of closed 
generic registrations, the open registration will still be last in the enumerable.

> [!NOTE]
> This is not the same as when resolving a single instance - which will return the last, best-matched registration
> for the generic type.  So if one or more closed generic registrations exist for the exact type requested, then 
> the one that was added last will be used for the single instance.
> ***
> Also note that **Constrained** generic implementations are *not* equal to closed generic registrations. So an 
> explicit registration against `IFoo<Bar>` will always beat a constrained generic implementation such as
> `MyFoo<T> : IFoo<T> where T : Bar` (see further down).

***

### Best match *only* (global)

Sometimes, you might only want objects in your enumerable which were registered against the best-matched generic, so, if you
have two open generic registrations for `IFoo<>` but a specific registration for `IFoo<Bar>`.  When requesting an 
enumerable of `IFoo<Baz>`, you want the two objects registered against the open generic.  However, when you request
an enumerable of `IFoo<Bar>`, you only want one object - from the specific `IFoo<Bar>` registration.

As mentioned in the note in the previous example, to do this, we use the @Rezolver.Options.FetchAllMatchingGenerics option
to control this globally for a container:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example6b)]

***

### Best match *only* (per-service)

We can also control this on a per-service basis - simply set the @Rezolver.Options.FetchAllMatchingGenerics option to
`false` for a given service type.

Here, we're using the same types that we introduced for the [constrained generics example](#constrained-generics), 
and disabling the 'fetch all' behaviour for a specific closed generic:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example6c)]

You can also set this against an open generic - which then has the same effect but for all generics based on the same
open generic (here we're disabling it for `IGeneric<>`)

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example6d)]

***

# Contravariance

Rezolver also supports generic contravariance which, in the case of 
enumerables, means getting multiple results from any and all registrations that 
match a particular type argument's bases or interfaces, in descending order of 'distance'
from the requested type.

For more on this, including concrete examples, see the [next steps](#next-steps) section
below.

***

# Next steps

- Learn about Rezolver's support for [lazy and eager enumerables](lazy-vs-eager.md) (note: all auto-generated enumerables are lazily 
evaluated by default)
- As [mentioned above](#contravariance), by default, Rezolver handles contravariant 
generic type parameters - see this in action with enumerables 
[in the section dedicated to generic contravariance](../contravariance.md)