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
> Most .Net developers will already be familiar with the idea of 'realising' an enumerable to force it to become eager, using the
> @System.Linq.Enumerable.ToArray* or @System.Linq.Enumerable.ToList* extension methods.  These functions simply enumerate their
> input enumerable once, storing the results into an array/list or similar structure.