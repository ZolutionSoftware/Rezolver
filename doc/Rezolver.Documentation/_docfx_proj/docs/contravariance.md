# Generic Contravariance

> [!TIP]
> If you don't know anything about generic variance in .Net then you should read through
> the [MSDN topic 'Covariance and Contravariance in Generics'](https://docs.microsoft.com/en-us/dotnet/standard/generics/covariance-and-contravariance)
> as it explains these concepts in far greater detail than we can!

When a generic type is requested from the container, Rezolver builds a search list of all 
the possible instances of that generic type which could satisfy that request, returning an 
instance produced by the registration that was made against the best-matching version of
that generic (and, in the case of [enumerables](enumerables.md), potentially more instances
in order of best-to-worst match).

The same is also true when the requested type has one or more contravariant type parameters - 
in which case bases or interfaces of the corresponding type argument are also sought.

# Examples

Contravariance is a tricky subject at the best of times and contriving examples for how
to take advantage of it when using a DI container is even trickier.  That said, you'll know
when *you* need it, so hopefully what's presented here will help you.

Hopefully, when you get to the stage of wanting to use it, you should just naturally
expect it to work, and it will! :wink:


## Ordered Enumerables

Let's say that in addition to Rezolver's own enumerables functionality, our application 
wants to be able to inject explicitly ordered enumerables.  For that, we'll need the
@System.Collections.Generic.IComparer`1 interface, whose type parameter is contravariant.

Now, the idea is that our application will be able to request an `IOrderedEnumerable<T>`
for _**any**_ type for which it could also request an `IEnumerable<T>` - this means that
we're going to have to have a default comparer that can be used, whilst allowing specific
comparers for known types to be registered in addition.

The most natural way to do this is to try to wrap the @System.Linq.Enumerable.OrderBy*
extension method, but Rezolver cannot (currently) bind to generic methods, but we can 
instead write a class which implements `IOrderedEnumerable<T>` by wrapping around 
`OrderBy`:

[!code-csharp[OrderedEnumerableWrapper.cs](../../../../test/Rezolver.Tests.Examples/Types/OrderedEnumerableWrapper.cs#example)]

The generic class accepts an enumerable (automatically injected by Rezolver) and an 
`IComparer<T>` - the default version of which will wrap around the `Comparer<T>.Default`
property:

[!code-csharp[DefaultComparerWrapper`1.cs](../../../../test/Rezolver.Tests.Examples/Types/DefaultComparerWrapper`1.cs#example)]

Now - if we configure the container correctly and resolve the correct types, getting an 
ordered enumerable is pretty simple:

[!code-csharp[ContravarianceExamples.cs](../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example1)]

So far, so 'open generic' - we can now get ordered enumerables which 'work' for any of the
built-in .Net types that `Comparer<T>.Default` also works for.  Now we need to extend it
for custom types.

Let's use a similar example to the one used on MSDN for the 
@System.Collections.Generic.Comparer`1 type - and introduce some types which represent 
2D geometries:

[!code-csharp[Shapes.cs](../../../../test/Rezolver.Tests.Examples/Types/Shapes.cs#example)]

And let's introduce an `IComparer<T>` which sorts these objects by their area:

[!code-csharp[ShapeAreaComparer.cs](../../../../test/Rezolver.Tests.Examples/Types/ShapeAreaComparer.cs#example)]

Clearly, with this in place it would be trivial to register shape instances as `I2DShape` 
and register the `ShapeAreaComparer` as the comparer for the type `IComparer<I2DShape>` -
thus allowing us to request an `IOrderedEnumerable<I2DShape>`, we don't need a
contravariance-aware DI container to be able to do that.

But what if we want to inject an `IOrderedEnumerable<T>` where `T` is one of the 
*concrete* shape types:

- `Rectangle`
- `Square`
- `Circle` 

Meaning that our `OrderedEnumerableWrapper<T>` will be looking for one of these, 
respectively:

- `IComparer<Rectangle>`
- `IComparer<Square>`
- `IComparer<Circle>`

We know that an instance of our `ShapeAreaComparer` can be assigned to variables of all 
these types, because `IComparer<T>` is contravariant, as demonstrated by this snippet:

```cs
// create an instance of the comparer, which implements IComparer<I2DShape>
var comparer = new ShapeAreaComparer(Comparer<double>.Default);

// can be used for all of these:
IComparer<Rectangle> a = comparer;
IComparer<Square> b = comparer;
IComparer<Circle> c = comparer;
```

And Rezolver knows this too - which means that all we have to do is to register the
type `ShapeAreaComparer` as `IComparer<I2DShape>`, and Rezolver will automatically use that comparer
whenever it is compatible with a requested type.

The following example breaks this into two demonstrations - one explicit set of 
assertions which verify the `ShapeAreaComparer` is being used for any compatible comparer
type, and another which verifies that ordered enumerables of concrete shape types are being
created correctly:

[!code-csharp[ContravarianceExamples.cs](../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example2)]

> [!NOTE]
> In the example, it would make sense for `ShapeAreaComparer` to be registered as a singleton,
> the idea being that one single instance is used for all `IComparer<I2DShape>`-compatible
> requests.  While this will work at the moment, Rezolver incorrectly creates one instance 
> per unique `IComparer<T>` type instead of one 'master' instance for all.
>
> This bug is being tracked on [issue #53](https://github.com/ZolutionSoftware/Rezolver/issues/53)

***

## Overriding Contravariant Registrations

As described in the opening paragraph above, Rezolver conducts searches for compatible 
registrations in the order of best-to-worst match which, for contravariant type parameters,
means that registrations which target a derived type are used in preference to a 
registration which targets a base or interface.

So, extending our above example, let's say we want to sort rectangles by area and 
*then by length* if their areas are the same, and *then by height* if their lengths 
are the same.

For this we need a new `RectangleComparer`:

[!code-csharp[RectangleComparer.cs](../../../../test/Rezolver.Tests.Examples/Types/RectangleComparer.cs#example)]

Note that it has a dependency on `IComparer<I2DShape>` - which explicitly targets our
catch-all `ShapeAreaComparer` as per the original setup.

Now we just add the registration (shown in the example being added *before* our catch-all
just to highlight the fact that the order in which these registrations are added doesn't 
affect the logic) and use it.

> [!NOTE]
> As the comments in the next example point out - the enumerable that's added in this 
> example is deliberately ordered such that a stable sort by area _**alone**_ would 
> yield the wrong result (as would be the case if we only used our original 
> `ShapeAreaComparer`)

[!code-csharp[ContravarianceExamples.cs](../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example3)]

> [!WARNING]
> Eager readers will have noticed that the `RectangleComparer` is effectively a 
> decorator and, while it's certainly possible to implement it and register it as such 
> (with some tweaks), there is currently a bug affecting contravariant Decorators 
> which prevents them being used for more derived types unless they are specifically
> registered for those types.
>
> The bug is being tracked on [issue #54](https://github.com/ZolutionSoftware/Rezolver/issues/54)

