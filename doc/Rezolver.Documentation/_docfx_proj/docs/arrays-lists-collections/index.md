# Injection other collection types

[As described in the enumerables section](../enumerables.md), Rezolver can automatically
build and inject instances of `IEnumerable<T>` of generic and non-generic types based 
purely off the registrations within the container.

Sometimes, however, you might also want to inject arrays, lists or collections (using 
such types such as `List<T>`, `IList<T>`, `T[]`, `Collection<T>` etc) whose contents are
also built automatically purely from the registrations of the type `T` present in the 
container.

> [!NOTE]
> The functionality described in this section does not supersede your ability to create
> explicit registrations for these types.

By default Rezolver has the ability to automatically build and inject the following types
which are commonly used to represent series of objects:

- Arrays
- <xref:System.Collections.Generic.List`1>, <xref:System.Collections.Generic.IList`1>, 
<xref:System.Collections.Generic.IReadOnlyList`1> - 

- @System.Collections.ObjectModel.Collection`1, @System.Collections.Generic.ICollection`1,
@System.Collections.ObjectModel.ReadOnlyCollection`1, @System.Collections.Generic.IReadOnlyCollection`1

## Requires `IEnumerable<T>` Injection

The key thing to note about Rezolver's implementation of these is that they all, in 
some way, rely upon the injection of `IEnumerable<T>` mentioned at the start of this topic
and, in some cases, rely on each other.

***

# Next steps

- [Array injection](arrays.md)
- [List injection](lists.md)
- [Collection injection](collections.md)