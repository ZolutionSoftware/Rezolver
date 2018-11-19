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

And then we have a generic type which has one of these as a member and which, by default, always adds a default 
instance of type `T` to that collection (so all our tests will verify that this item is still present after 
injecting):

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
@Rezolver.MemberBindingBuilder`2.AsCollection* overload:

1. @Rezolver.MemberBindingBuilder`2.AsCollection tells Rezolver to use collection injection with no customisation
2. @Rezolver.MemberBindingBuilder`2.AsCollection(System.Type) lets you specify the element type of the enumerable to resolve
3. @Rezolver.MemberBindingBuilder`2.AsCollection(System.Type[]) lets you explicitly provide the types to resolve for each element
4. @Rezolver.MemberBindingBuilder`2.AsCollection(Rezolver.ITarget[]) lets you provide individual targets whose results will be used as elements

So let's re-do part of the previous `CustomCollection` example and show how you to use 4) to inject a specific
set of values instead of relying on those which are registered in the container:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example11)]

> [!TIP]
> Remember that manually creating targets doesn't necessarily mean that you have to step outside of normal container 
> operation.  For example, if you provide a @Rezolver.Targets.DelegateTarget which wraps a delegate that has one or 
> more parameters, then Rezolver will automatically inject arguments to that delegate!

As intimated earlier, however, one of the primary reasons for using the @Rezolver.MemberBindingBuilder`2.AsCollection*
method is to instruct Rezolver to use collection injection even when the property is writable.  Whether it's a good
idea for a type to have a writable property exposing a collection that it also creates and initialises by default
is outside the scope of this documentation.  The point is, it's *possible*, and it might well apply to you.

So, here's a slight reworking of the `HasCustomCollection<T>` type that makes its `List` member writable:

[!code-csharp[HasCustomCollection.cs](../../../../../test/Rezolver.Tests.Examples/Types/HasCustomCollection.cs#example2)]

And here we can see that, by default, this type can no longer be created by the container, when member binding is
enabled, without a registration for the collection type used by the `List` member, as the `Assert.ThrowsAny(...)` 
call proves:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example12)]

But this is easily rectified with the fluent API's `AsCollection()` method:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example13)]

## Explicit (`ListMemberBinding`)

If you're writing your own `IMemberBindingBehaviour` implementation, as shown in the 
['custom behaviours'](custom.md) topic, then you can still leverage collection injection.

Simply create an instance of the @Rezolver.ListMemberBinding class - providing:

- @System.Reflection.MemberInfo of the member to be bound
- An @Rezolver.ITarget representing the enumerable value whose elements are to be added to the collection
- A @System.Type representing the element type of the underlying enumerable
- A @System.Reflection.MethodInfo of the instance method that is to be called to add items to the collection

And Rezolver will do the rest.

We'll add an example to cover this scenario in the future, however, if you're in the position where you need to
use this API, then you probably don't it, as you're quite a long way down the rezolver rabbit hole already!

* * *

Now you know the support that Rezolver has for collection injection, it's time to look at 
[to build custom member binding behaviours with the fluent API](fluent-api.md).