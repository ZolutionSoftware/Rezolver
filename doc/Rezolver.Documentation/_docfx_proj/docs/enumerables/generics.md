# Enumerables of Generics

Now you've seen how Rezolver's [automatic enumerable injection works](../enumerables.md), let's move on to how 
generics (specifically, open generic registrations) work.

## Open Generics

You can register multiple open generics of the same type (e.g. `IFoo<>`), resolve an enumerable of 
`IFoo<Bar>`, and the container will create an enumerable containing an object for each open generic registration:

[!code-csharp[UsesAnyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/UsesAnyService.cs#example)]
[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example5)]

## Constrained Generics

When one or more constrained open generic types are registered against an open generic type, Rezolver will only attempt
to include results from those registrations if all their target type's constraints are met.

[!code-csharp[ConstrainedGenerics.cs](../../../../../test/Rezolver.Tests.Examples/Types/ConstrainedGenerics.cs#example)]
[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example5b)]

## Mixing open/closed generics

In many applications which use DI and generics, you often have 'common' open generic registrations, such as
those in the 'enumerables of open generics' example above, and then specific registrations for one or more 
well-known *closed* generics.

When resolving a single instance for a given generic, Rezolver will of course select the newest *most specific* 
registration it can find.  This means that if you have registrations for both `IFoo<>` and `IFoo<Bar>` and 
request an instance of `IFoo<Bar>`, then the second registration is used.

If, however, you request an `IEnumerable<IFoo<Bar>>`, what should Rezolver return?

The answer is: it depends on your specific need.  Sometimes you might want all applicable objects to be returned,
and sometimes you might only want the best-matched registrations to be used.

The good news is that it's easy to tell Rezolver which of the behaviours you want it to observe.

### All possible generics

> [!NOTE]
> The functionality described here represents a breaking change from 1.2 - which did not allow you to mix enumerables
> of objects from closed *and* open generic registrations.  The old behaviour can be re-enabled by setting the 
> @Rezolver.Options.FetchAllMatchingGenerics option to <c>false</c>, as shown in the next example.

Let's say that we have one open generic registration for `IUsesAnyService<>` to be used as a catch-all, but that
when `IMyService` is used, we have two other types that we also want to use.

In this case, we simply need to add one or more registration(s) for the concrete generic type, and Rezolver will 
intelligently select all the generics that apply when building its enumerable.

So, given these extra generic types:

[!code-csharp[UsesIMyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/UsesIMyService.cs#example)]

We can do this:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example6)]

When Rezolver matches its registrations for `IEnumerable<IUsesAnyService<IMyService>>`, it sees the 
two explicit registrations made against the closed generic type `IUsesAnyService<IMyService>` ***and*** the 
registration against the open generic `IUsesAnyService<>` - hence you get an enumerable with *three* items.

The order you register open generics and closed generics doesn't matter - the logic is applied on a type-by-type
basis running from most specific to least specific; however, the order that objects 
appear in the enumerable which come from the same generic type registration (i.e. `IFoo<>` or `IFoo<Bar>`) is honoured.

### Best match *only*

Sometimes, you might only want objects in your enumerable which were registered against the best-matched generic, so, if you
have two open generic registrations for `IFoo<>` but a specific registration for `IFoo<Bar>`.  When requesting an 
enumerable of `IFoo<Baz>`, you want the two objects registered against the open generic.  However, when you request
an enumerable of `IFoo<Bar>`, you only want one object - from the specific `IFoo<Bar>` registration.

As mentioned in the note in the previous example, to do this, we use the @Rezolver.Options.FetchAllMatchingGenerics option
to control this globally for a container:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example6b)]
