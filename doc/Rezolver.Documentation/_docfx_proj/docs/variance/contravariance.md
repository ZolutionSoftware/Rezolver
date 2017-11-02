# Generic Contravariance

Generic contravariance in .Net is where an instance of a generic type featuring a less derived type 
as a type argument can be assigned to a reference to a the same generic, but with a more derived
type argument.  Contravariant type parameters are declared using the `in` modifier in the generic
declaration.

Commonly used contravariant generics are any of the `System.Action<>` delegate derivatives, or interface
types such as the @System.Collections.Generic.IComparer`1 interface (used in the examples below).

Here are some examples of legal assignments which take advantage of contravariance:

```cs
Action<string> a
    = new Action<object>(o => Console.WriteLine(o));
IComparer<string> b
    = Comparer<object>.Default;
```

When a generic interface or delegate declares a contravariant type parameter, it restricts that
type to being used as a method *parameter* - i.e. it becomes an 'input' type, hence the `in` modifier.

# In Rezolver

When a generic type is requested from the container, Rezolver builds a search list of all 
the possible instances of that generic type which could satisfy that request, returning an 
instance produced by the registration that was made against the best-matching version of
that generic (and, in the case of [enumerables](../enumerables.md), potentially more instances
in registration order).

The same is also true when the requested type has one or more contravariant type parameters - 
in which case bases or interfaces of the corresponding type argument are also sought.

The important thing to note here is that, just as with [covariance](covariance.md), it's the 
type that an @Rezolver.ITarget is *registered* against that matters - not the implementing type.

So, given these types:

```cs
interface IContravariant<in T>
{
  void Foo(T t);
}

class MyBase {}

class MyDerived : MyBase {}

class AcceptsMyBase : IContravariant<MyBase>
{
  void Foo(MyBase t)
  {
    
  }
}
```

`AcceptsMyBase` will be used to create instances of `IContravariant<MyDerived>` if it is registered
against the contravariant interface type `IContravariant<MyBase>`:

```cs
container.RegisterType<AcceptsMyBase, IContravariant<MyBase>>();

// gets an instance of AcceptsMyBase
var result = container.Resolve<IContravariant<MyDerived>();
```

## Type Precedence is King

In nearly all cases (including for [covariance](covariance.md)), Rezolver uses the 
*most recently registered* target to provide the result for a particular service type.  

For requests for generic types which have one or more contravariant type parameters, however, 
this is not the case.

For these, Rezolver instead looks for an exact match and, if it doesn't find one,
it then walks 'up' the type hierarchies of each contravariant type parameter looking for a hit,
prioritising non-`object` base types over interfaces, and interfaces over the `object` base
type.

So, given a type hierarchy like this:

```cs
interface IBase
{
}

class Base : IBase
{
}

interface IDerived : IBase
{
}

class Derived : Base, IDerived
{
}
```

If a request is made for an `Action<Derived>`, Rezolver will search for registrations for these types:

- `Action<Derived>`
- `Action<Base>`
- `Action<IDerived>`
- `Action<IBase>`
- `Action<object>`
- `Action<>` *I.e. an 'open' generic registration*

The first `ITarget` to be retrieved from the container's `ITargetContainer` for one of these types will 'win'.

Also, generic interfaces are given precedence over non-generic interfaces; with consideration given 
to interfaces which 'inherit' other interfaces (not a technically correct phrase, but we all know what 
it means).

### Array Types

For generic types to which an array type is passed to a contravariant type argument, the picture gets more
complicated - as the dynamically built `Array` type for a given element type also brings with it several interfaces.

The search list for an array type, like the one above, then, starts getting very complex very quickly, e.g:

- `Action<Derived[]>`
- `Action<IList<Derived>>`
- `Action<IEnumerable<Derived>>`
- `...`
- `Action<Base[]>`
- `Action<IList<Base>>`
- `...`
- `Action<IEnumerable>`
- `Action<Array>`
- `Action<object>`

The above list is, by no means, exhaustive - but you should be able to get the picture.  Notice that the `Array` base
type, like the `object` type before, is included as one of the last types, since it is ubiquitous to all arrays.

# Examples

Contravariance is a tricky subject at the best of times and contriving examples for how
to take advantage of it when using a DI container is even trickier.  That said, you'll know
when *you* need it, so hopefully what's presented here will help you.

Hopefully, when you get to the stage of wanting to use it, you should just naturally
expect it to work, and it will! :wink:

## Injecting `IComparer<T>`

Let's say that in addition to Rezolver's own enumerables functionality, our application 
wants to be able to inject explicitly ordered enumerables.  For that, we'll need the
@System.Collections.Generic.IComparer`1 interface, whose type parameter is contravariant.

Now, the idea is that our application will be able to request an `IOrderedEnumerable<T>`
for _**any**_ type for which it could also request an `IEnumerable<T>` - this means that
we're going to have to have a default comparer that can be used, whilst allowing specific
comparers for known types to be registered in addition.

The most natural way to do this is to try to wrap the @System.Linq.Enumerable.OrderBy*
extension method, but Rezolver cannot (currently) bind to generic methods

Instead, we can quite easily write a class which implements `IOrderedEnumerable<T>` 
by wrapping around the `OrderBy` extension method:

[!code-csharp[OrderedEnumerableWrapper.cs](../../../../../test/Rezolver.Tests.Examples/Types/OrderedEnumerableWrapper.cs#example)]

The generic class accepts an enumerable (automatically injected by Rezolver) and an 
`IComparer<T>` - the default version of which will wrap around the `Comparer<T>.Default`
property:

[!code-csharp[DefaultComparerWrapper`1.cs](../../../../../test/Rezolver.Tests.Examples/Types/DefaultComparerWrapper`1.cs#example)]

Now - if we configure the container correctly and resolve the correct types, getting an 
ordered enumerable is pretty simple:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example1)]

So far, so 'open generic' - we can now get ordered enumerables which work for any of the
built-in .Net types that `Comparer<T>.Default` also works for.  Now we need to extend it
for custom types.

Let's use a similar example to the one used on MSDN for the 
@System.Collections.Generic.Comparer`1 type - and introduce some types which represent 
2D geometries:

[!code-csharp[Shapes.cs](../../../../../test/Rezolver.Tests.Examples/Types/Shapes.cs#example)]

And let's introduce an `IComparer<T>` which sorts these objects by their area:

[!code-csharp[ShapeAreaComparer.cs](../../../../../test/Rezolver.Tests.Examples/Types/ShapeAreaComparer.cs#example)]

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

Without a contravariance-aware container, this would cause us a problem, because we'd
need to register the `ShapeAreaComparer` type for each and every specialisation of 
`IComparer<T>` applicable for every `I2DShape`-implementation present in our application.

Thankfully, Rezolver is aware of the contravariance of `T` in `IComparer<T>`, which 
means that all we have to do is to register the type `ShapeAreaComparer` as 
`IComparer<I2DShape>`, and Rezolver will automatically use it whenever it is compatible 
with a given `T`.

The following example breaks this into two demonstrations - one explicit set of 
assertions which verify the `ShapeAreaComparer` is being used for any compatible comparer
type, and another which verifies that ordered enumerables of concrete shape types are being
created correctly:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example2)]

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

[!code-csharp[RectangleComparer.cs](../../../../../test/Rezolver.Tests.Examples/Types/RectangleComparer.cs#example)]

Note that it has a dependency on two other comparers: `IComparer<I2DShape>` - which explicitly targets our
catch-all `ShapeAreaComparer` as per the original setup - and `IComparer<double>`  so we can compare lengths
and highlights (which will use the 'catch-all' `DefaultComparerWrapper<T>`).

Now we just add the registration (shown in the example being added *before* our catch-all
just to highlight the fact that the order in which these registrations are added doesn't 
affect the logic) and use it.

> [!NOTE]
> As the comments in the next example point out - the enumerable that's added in this 
> example is deliberately ordered such that a stable sort by area _**alone**_ would 
> yield the wrong result (as would be the case if we only used our original 
> `ShapeAreaComparer`)

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example3)]

> [!WARNING]
> Eager readers will have noticed that the `RectangleComparer` is effectively a 
> decorator and, while it's certainly possible to implement it and register it as such 
> (with some tweaks), there is currently a bug affecting contravariant Decorators 
> which prevents them being used for more derived types unless they are specifically
> registered for those types.
>
> The bug is being tracked on [issue #54](https://github.com/ZolutionSoftware/Rezolver/issues/54)

So note that we are creating an explicit registration for `IComparer<Rectangle>` which then
supersedes our `IComparer<I2DShape>` registration for `Rectangle`, but it also kicks
in for `Square` as well - because `Rectangle` is 'closer' to `Square` than `I2DShape` is.

## Contravariance with Enumerables

If you have read through the [documentation on enumerables of generics](../enumerables/generics.md)
then you will already be aware of Rezolver's powerful auto-enumerable functionality, and you
can probably already guess how it works when requesting enumerables of contravariant generics.

For an enumerable of a generic with one or more contravariant type parameters, *every* registration 
that matches the given type will be returned - in registration order, as demonstrated by this example
which builds on our `I2DShape` types to configure the container to write shape information
to a `StringBuilder`:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example5)]

> [!TIP]
> `object` is always considered last in a contravariant search, after a type's other bases
> or interfaces, because `object` is ubiquitous and technically applies to all instances.
> 
> Also, while bases are walked in order of inheritance, from least to most derived, the 
> order that interfaces are considered is less well-defined.  Rezolver applies a trivial
> sort on the list of interfaces of the type such that generic interfaces appear before 
> non-generic interfaces; and any 'derived' interfaces before those that they 'inherit'.
> 
> Finally, for array types, the `Array` runtime type is also always considered after all other
> applicable types - except `object`.

## Disabling Contravariance (Advanced)

> [!WARNING]
> If you need to start playing around with how contravariance works naturally within the
> container to make things work for your application then, assuming it's not to avoid
> a bug in Rezolver, you should consider instead whether your design is correct.
> 
> Generally, if you've written a delegate or interface type with a contravariant parameter
> and are subsequently injecting instances of it, then the way that Rezolver will locate
> registrations for that type should be correct as per the registrations you add to
> the container.  So, in most cases, if you're getting unexpected results it's likely you've
> missed something in your registrations.

As with much of the rest of the built-in functionality in Rezolver, it's possible to disable
contravariance by setting an option on the @Rezolver.ITargetContainer which underpins your
@Rezolver.IContainer (which, if you're using either @Rezolver.Container or 
@Rezolver.ScopedContainer is the container itself).

The option is @Rezolver.Options.EnableContravariance, which has a 
@Rezolver.Options.EnableContravariance.Default value that is, unsurprisingly, equivalent 
to `true`.

You can set the option to `true` or `false`:

- Globally
- Against a specific closed generic, e.g. `Foo<Bar>`, to control contravariance for that 
specific type
- Against an open generic, e.g. `Foo<>`, to control contravariance for *any* generic type
based on it
- Against a non-generic type, e.g. `Bar`, to control contravariance when that type, or any
of its derivatives, is supplied as a type argument to *any* contravariant type parameter.

> [!NOTE]
> When you disable contravariance using any of the following methods, Rezolver will expect
> registrations for the associated concrete generic types that your application requests.

Disabling contravariance is something you might do when resolving 
multiple instances of a contravariant type, as in the previous delegate example, so we'll
build on that with additional delegates for the `Square` type.

We'll show five different ways to disable contravariance such that we only get one delegate
when we request an array of `Action<Square, StringBuilder>` (which simply reuses the container's
`IEnumerable<>` functionality.

**Note**: We're using this method in these examples:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example_registershapedelegates)]

***

### Disabling for generics

When you disable per-generic, if that type has one or more contravariant type parameters,
then you are instructing Rezolver to suspend contravariant searches for any of those type
parameters either for a specific closed version of that generic, or for any.

> [!TIP]
> If a generic type doesn't have contravariant parameters, and you disable 
> contravariance for it, you are telling Rezolver to suspend contravariance when that type
> is passed as an argument to *another generic's* contravariant type parameter - as
> shown in the [next section](#disabling-for-type-arguments).

In the first example, we'll disable contravariance for 
`Action<Square, StringBuilder>` - which means we only get one delegate in our array:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example6)]

We can also disable it for all `Action<,>` delegates:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example7)]

***

### Disabling for type arguments

The previous two examples show how we can control Rezolver's contravariance for
specific generic types.  But sometimes you might want to suspend contravariance for
a type, or any of its derivatives or implementations, for *any* generic to which they are
passed as type arguments.

So, the next example shows how we can disable contravariance for the `Square` type, which 
means that when *any* concrete generic is requested, if `Square` is passed as the type argument
to a contravariant type parameter, the contravariance for that parameter will be ignored:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example8)]

And we can also do the same for *any* type which has the `I2DShape` interface:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example9)]

### Disabling globally

Finally, the simplest of all: disabling contravariance for all types:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example10)]

### Caution: Changing to 'Opt-in'

As will have become apparent, Rezolver uses an opt-out model by default for contravariance
simply because a generic type parameter is either contravariant or it isn't; and if it 
_**is**_ then the container should adapt accordingly.

If you want *selective* contravariance for only certain types in your application,
then you will typically switch it off globally and then start setting the 
@Rezolver.Options.EnableContravariance option to `true` for those types you want to 
have opted in.

But, in almost all cases, you will need to set it to `true` for more types than you 
initially thought - as contravariance has been switched off not only for the generics
but also for all the types which are being passed as type arguments.

So, our final example shows how we can re-enable contravariance *only* for the 
`Action<Square, StringBuilder>` delegate type - which actually involves re-enabling it
for the `Square` type also:

[!code-csharp[ContravarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/ContravarianceExamples.cs#example11)]

# Summary

Rezolver's support for contravariance is comprehensive - including support for nesting 
contravariant types within others (examples not included because it gets very complicated
very quickly, but you can see the test cases for this in the standard Rezolver test suite).

So if you start working with contravariant types, then you should find Rezolver *just works*.