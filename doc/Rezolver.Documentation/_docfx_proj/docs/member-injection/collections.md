# Collection Initialisation

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

## In Rezolver

If you are binding members in Rezolver using one of the [standard behaviours](index.md#standard-behaviours),
for example the @Rezolver.BindAllMembersBehaviour behaviour, then the requirements for a member to be considered
for _**automatic**_ collection binding are as follows:

1. It must be a public read-only property
2. It's type must implement the <xref:System.Collections.Generic.IEnumerable`1> interface
3. It's type must have a publicly accessible `Add` method which accepts a single parameter whose type is equal to the element type of its `IEnumerable<T>` interface

Luckily, this means that all the most common collection types used in .Net applications (`List<T>`, `IList<T>`, `Collection<T>` etc)
are automatically supported.

But Rezolver is not limited only to 'recognised' collection types - if you write your own custom collection type,
then, so long as it satisfies requirements 2 & 3 above, Rezolver can also inject items into that as well.

### Resolving `IEnumerable<T>` is required

When Rezolver performs collection injection, it does it by resolving an `IEnumerable<T>` from the container to
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

### Why only read-only properties?

When a property is writable, Rezolver's standard behaviours will assume that you want that property to be injected
by *assignment* rather than by modifying it (i.e. by adding to it).  You can still inject them using the standard
behaviours - but only by ensuring there's a registration for that member's type instead.

> [TIP]
> Later in this topic, you'll see how you can use the [fluent API](fluent-api.md) to instruct Rezolver to bind
> a writable collection property using collection injection, instead of assignment injection.


