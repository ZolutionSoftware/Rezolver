# Automatic List Injection

There are three primary types related to generic lists that are commonly used in 
.Net projects:

- @System.Collections.Generic.List`1
- @System.Collections.Generic.IList`1
- @System.Collections.Generic.IReadOnlyList`1

In addition to being able to add explicit registrations for specific closed generic list 
types, Rezolver supports the automatic creation and injection of these types in the same way that
[enumerables](../enumerables.md) are also supported.

> [!NOTE]
> For all three types, Rezolver creates/injects an instance of `List<T>` by 
> binding to the
> @System.Collections.Generic.List`1.%23ctor(System.Collections.Generic.IEnumerable{`0})
> constructor - which means that [enumerable injection](../enumerables.md) must be enabled
> unless you intend to take control of all of your `IEnumerable<T>` registrations.
>
> Unfortunately, this also means that an instance of `IReadOnlyList<T>` can be cross-cast 
> to `List<T>` or `IList<T>` in order to make modifications to it - this will be changed
> in a future release.

# Examples

Because all the automatic `IEnumerable<T>` functionality supported 
by Rezolver also flows through to these list types (e.g. open generics etc), not many
examples are required - we'll just cover the basics (as is the case in the [arrays](arrays.md)
documentation), and then the rest you can learn from the 
[enumerables documentation](../enumerables.md).

So - an empty container can automatically inject an empty list instance:

[!code-csharp[ListExamples.cs](../../../../../test/Rezolver.Tests.Examples/ListExamples.cs#example1)]

And, after adding three registrations for `IMyService`, any list that is created will contain
three instances created from those registrations:

[!code-csharp[ListExamples.cs](../../../../../test/Rezolver.Tests.Examples/ListExamples.cs#example2)]

# Disabling Automatic List Injection

First, you should look at how [to disable array injection](arrays.md#disabling-automatic-array-injection) as 
the process to do it for lists is exactly the same - except it's a different option:

[!code-csharp[ListExamples.cs](../../../../../test/Rezolver.Tests.Examples/ListExamples.cs#example3)]

# See Also

- [Enumerables](../enumerables.md)
- [Arrays](arrays.md)
- [Collections](collections.md)