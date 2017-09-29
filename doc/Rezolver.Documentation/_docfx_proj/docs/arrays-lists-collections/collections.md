# Collection injection

There are four commonly used generic collection types in .Net, two classes and two interfaces:

- @System.Collections.ObjectModel.Collection`1 and the interface type
@System.Collections.Generic.ICollection`1
- @System.Collections.ObjectModel.ReadOnlyCollection`1 and the interface type
@System.Collections.Generic.IReadOnlyCollection`1

In addition to being able to [auto-inject `IEnumerable<T>`](../enumerables.md), Rezolver
can also automatically build instances of any of these types, with empty collections being 
created by default if no registrations are present.

> [!WARNING]
> Collection injection relies on [list injection](lists.md) because, behind the scenes, the
> container is using the `IList<T>` constructors 
> @System.Collections.ObjectModel.Collection`1.%23ctor(System.Collections.Generic.IList{`0})
> and @System.Collections.ObjectModel.ReadOnlyCollection`1.%23ctor(System.Collections.Generic.IList{`0})
> to create the collection instances.
>
> Therefore, if you disable automatic list injection, automatic collection injection will only 
> work if you specifically add registrations for the corresponding `IList<T>` types.

# Examples

The following two examples are the same as the first two for `IEnumerable<T>`, and identical 
to those used for [arrays](arrays.md) and [lists](lists.md).

Empty collections:

[!code-csharp[CollectionExamples.cs](../../../../../test/Rezolver.Tests.Examples/CollectionExamples.cs#example1)]

Collections after registrations have been added:

[!code-csharp[CollectionExamples.cs](../../../../../test/Rezolver.Tests.Examples/CollectionExamples.cs#example2)]

# Disabling Collection Injection

First, you should look at how [to disable array injection](arrays.md#disabling-automatic-array-injection) as the process
to do it for collections is exactly the same - except it's a different option:

[!code-csharp[CollectionExamples.cs](../../../../../test/Rezolver.Tests.Examples/CollectionExamples.cs#example3)]

# See Also

- [Enumerables](../enumerables.md)
- [Arrays](arrays.md)
- [Lists](lists.md)