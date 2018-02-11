# Collection Member Injection

## Background

Sometimes you might have a class like this:

```cs
class HasFoos
{
    public IList<Foo> FooCollection { get; } = new List<Foo>();
}
```

> [!NOTE]
> Pay close attention to the way that the `FooCollection` is declared here - in particular, that it is read-only - 
> this is a key part of Rezolver's automatic support for collection initialisers.

In C# world, you can create an instance of this class *and* add elements to that collection by leveraging
[collection initialisers](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers#collection-initializers).

So, instead of doing this:

```cs
var hasFoos = new HasFoos();
hasFoos.FooCollection.Add(new Foo(1));
hasFoos.FooCollection.Add(new Foo(2));
hasFoos.FooCollection.Add(new Foo(3));
```

You can do this:

```cs
var hasFoos = new HasFoos() {
    FooCollection = {
        new Foo(1),
        new Foo(2),
        new Foo(3)
    }
};
```

The two approaches, whilst syntactically different, actually perform the same function - a collection initialiser
binds to a publicly accessible `Add` method and calls it repeatedly for each item in the initialiser on the newly
created object before the resulting object is returned to you.

As the documentation referenced above explains, in addition to a compliant `Add` method, the other prerequisite 
is that the type supports @System.Collections.IEnumerable.

## Rezolver's rules

If you are binding members in Rezolver using one of the [standard behaviours](index.md#standard-behaviours),
for example the @Rezolver.BindAllMembersBehaviour behaviour, then the requirements for a member to be considered
for _**automatic**_ collection binding are as follows:

1. It must be a public read-only property
2. Its type must implement `IEnumerable<T>`
3. Its type must have a publicly accessible `void Add` method which accepts a single parameter whose type is equal to the element type of its `IEnumerable<T>` interface

> [!NOTE]
> In C#, inherited interface methods, such as `ICollection<T>.Add(T)` and the like are also considered
> to be candidates for collection initialisation.
> 
> Currently Rezolver doesn't support this, but [issue #71](https://github.com/ZolutionSoftware/Rezolver/issues/71) 
> is tracking that as a bug for future implementation.

Luckily, this means that all the most common collection types used in .Net applications (`List<T>`, `IList<T>`, `Collection<T>` etc)
are automatically supported.

But Rezolver is not limited only to 'recognised' collection types - if you write your own custom collection type,
then, so long as it satisfies requirements 2 & 3 above, Rezolver can inject items into that as well.

## Resolving `IEnumerable<T>` is required

When Rezolver performs collection injection, it does so by resolving an `IEnumerable<T>` from the container to
get the elements that are to be added to the collection.  To determine the element type of the enumerable that
is resolved, Rezolver simply matches the `IEnumerable<T>` that is supported by the member's type.

Since Rezolver, by default, supports [automatic enumerable injection](../enumerables.md), all you have to do is
add registrations for that element type and Rezolver will automatically expose those registrations as elements
in the enumerable which will then be injected into the collection exposed by the member.

> [!NOTE]
> As the enumerable documentation linked above details, Rezolver's enumerables also match individual 
> registrations covariantly, so requesting an instance of `IEnumerable<MyBase>`, for example, will actually
> yield an enumerable containing all instances from any registration made against a type that derives from
> `MyBase`.

By default, also, if you don't have any registrations for a given element type, Rezolver will simply yield empty
enumerables for those types instead of throwing an error.

> [!TIP]
> As detailed in the enumerable documentation - you can still provide explicit registrations for any concrete
> `IEnumemrable<T>` type if you have an enumerable which must be built in a specific way.

## Why only read-only properties?

When a property is writable, Rezolver's standard behaviours will assume that you want that property to be injected
by *assignment* rather than by adding to it.  You can still inject them using the standard
behaviours - but any items that might have been automatically placed in the collection when the owning type
was created will be lost.

> [!TIP]
> Later in this topic, you'll see how you can use the [fluent API](fluent-api.md) to instruct Rezolver to bind
> a writable collection property using collection injection, instead of assignment injection.

Of course, [by default](../arrays-lists-collections/index.md), Rezolver can also build instances of classes such as `List<T>`, `IList<T>`, `Collection<T>` 
etc, so in many cases, injection-by-assignment will still often work.

* * *

# Examples

## Automatic Injection

We start with a simple class that has a readonly `IList<T>` member, much like the one introduced at the top of 
this page, except it initalises its collection with a single item from the get-go:

[!code-csharp[HasInjectableCollection.cs](../../../../../test/Rezolver.Tests.Examples/Types/HasInjectableCollection.cs#example)]

Now we can register the types `MyService2` and `MyService3` under their native types and, because, by default,
Rezolver will match registrations for any type compatible with `T` when resolving an `IEnumerable<T>`, they will 
be added into the `Services` list when a `HasInjectableCollection` instance is constructed.

Note that we are using the aforementioned @Rezolver.MemberBindingBehaviour.BindAll behaviour when we register the
type.  We could also have used the @Rezolver.MemberBindingBehaviour.BindProperties behaviour to achieve the same
result.

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example9)]

## Custom Generic Collection Type

As mentioned earlier, Rezolver supports custom collection types for member injection, so long as the type 
[obeys a few simple rules](#rezolvers-rules).  Here's perhaps the simplest generic collection type we can create:

[!code-csharp[CustomCollection.cs](../../../../../test/Rezolver.Tests.Examples/Types/CustomCollection.cs#example)]

And then we have a generic type which has one of these as a member:

[!code-csharp[HasCustomCollection.cs](../../../../../test/Rezolver.Tests.Examples/Types/HasCustomCollection.cs#example)]

Now let's use it to inject some numbers and strings:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example10)]

## Explicit Injection (fluent API)

If you're reading this documentation in topic-order then you won't yet have come across the 
[fluent member binding API](fluent-api.md), so you might want to skip ahead to that topic and then come back - this 
section is linked from there.

When using the fluent API - which operates an opt-in approach to member binding - if you mark a read-only collection
member like those shown above for binding with the `Bind()` extension method, then it will be bound as a collection:

```cs
var behaviour = MemberBindingBehaviour.For<MyClass>()
    .Bind(o => o.CollectionMember)
    .BuildBehaviour();
```

As described earlier, if a collection property is declared as read/write, then collection binding doesn't happen -
the container will instead attempt to resolve an instance of the collection type and write the result to the member.

However, the fluent API allows you to explicitly set a bound member as requiring collection injection, through the
`.AsCollection` overload.  This also provides additional customisation options for the collection binding - in that
you can explicitly provide individual element types to be resolved, or even supply the actual @Rezolver.ITarget
targets that you want to use to create each element when binding occurs.

So, first, let's re-do part of the previous `CustomCollection` example and show how you can inject a specific
set of values instead of relying on those which are registered in the container:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example11)]

As you'll no doubt realise, being able to explicitly provide these targets without having to just rely on what's
registered in the container means you have immense flexibility at your disposal.