# Decorating Collections

As already discussed in the [main topic](../decorators.md), Rezolver's auto-generated
[enumerables](../enumerables.md), 
[arrays](../arrays-lists-collections/arrays.md),
[collections](../arrays-lists-collections/collections.md) and 
[lists](../arrays-lists-collections/lists.md) are all decorator aware - so if you register a decorator
for `IFoo` and then request any of the supported types, e.g:

- `IEnumerable<IFoo>`
- `IFoo[]`
- `ICollection<IFoo>`
- `IList<IFoo>`

Then any individual instances within those collection types will be correctly decorated.

However, it's also possible to decorate *the collection __instance__* too, using a decorator class - with one
notable exception (which we'll deal with later).

So let's say that we're not using, or can't use, scoping and we want an `IList<T>` into which we might 
be adding `IDisposable` objects.  We want those disposable objects to be disposed automatically when they 
are removed from the list - which can easily be implemented via decoration:

[!code-csharp[DisposableListDecorator.cs](../../../../../test/Rezolver.Tests.Examples/Types/DisposableListDecorator.cs#example)]

And then the test:

[!code-csharp[ListExamples.cs](../../../../../test/Rezolver.Tests.Examples/ListExamples.cs#example10)]

As with all decorators, they stack and they will honour the order in which they are registered.

# Decorating `IEnumerable<T>`

Decorating `IEnumerable<T>` can be done in a similar way to that shown in the example in the 
[contravariance topic in which `IOrderedEnumerable<T>` is configured](../variance/contravariance.md#injecting-icomparert) -
simply change the registration of the `OrderedEnumerableWrapper<T>` from this:

```cs
container.RegisterType(typeof(OrderedEnumerableWrapper<>), typeof(IOrderedEnumerable<>));
```

To this:

```cs
container.RegisterDecorator(typeof(OrderedEnumerableWrapper<>), typeof(IEnumerable<>));
```

And all enumerables will now be sorted by default using an injected `IComparer<T>`.

However, there is another way to decorate `IEnumerable<T>` that doesn't involve writing a new class - especially
if the intention is to use [one of the many Linq extension methods](xref:System.Linq.Enumerable) for a specific
`IEnumerable<T>` - and that's the same as the decoration strategy that's needed for arrays.

# Decorating arrays

Decorating arrays is *not* possible via a decorator class - for the obvious reason that arrays are neither
classes that can be created via inheritance, nor do they have an interface that's exclusive to the array
type.  So, that means Rezolver cannot create array decorators via constructor injection - therefore
it's not possible to *write* a class that can decorate an array.

Instead, Rezolver offers another decoration strategy - [decorator delegates](delegates.md) - which allow you to
alter the object that is produced by the container before it's returned to the caller (or injected into another
object's constructor or whatever).

