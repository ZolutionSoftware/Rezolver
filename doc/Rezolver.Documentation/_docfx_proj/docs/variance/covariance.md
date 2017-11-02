# Generic Covariance

v1.3.2 of Rezolver sees the introduction of generic covariance, which is a much more commonly used type of 
generic variance in .Net, compared to [contravariance](contravariance.md), however it's also more rare in 
the IOC world.

Generic covariance allows a variable of type `Generic<Ta>` to be assigned to an instance of `Generic<Tb>` 
so long as the type `Tb` is reference compatible with `Ta`.

> [!WARNING]
> Note that this precludes `Ta` being `object` and `Tb` being
> a `struct` (a value type), because assignment of a value type to `object` requires a boxing conversion.

Here are some examples:

```cs
Func<object> f1 
    = new Func<string>(() => "Hello World");
Func<IEnumerable<object> f2 
    = new Func<IEnumerable<string>>(() => new string[] { "Hello World" });
Func<IEnumerable<object>> f3 
    = new Func<string[]>(() => new string[] { "Hello World" });
```

The first assignment is allowed because `string` is a reference type which, of course, inherits from `object` - 
therefore an instance of `Func<string>` can be assigned to a `Func<object>` reference.

The second assignment is allowed because `IEnumerable<out T>` is also a covariant generic type, which means that
an instance of `IEnumerable<string>` can be assigned to an `IEnumerable<object>` reference; which therefore also
means that the two `Func<out T>` types are also reference compatible.

The third assignment is allowed because of array covariance (which is, admittedly, slightly broken in .Net).
Again, a `string[]` instance can be assigned to an `object[]` reference; and since `IEnumerable<object>` is 
then an implemented interface of `object[]`; it means they, too, are reference compatible.

# In Rezolver

Rezolver's implementation of covariance relies only on the declaration of covariant type parameters on generic
types which are registered as services.

So, consider this:

```cs
interface ICovariant<out T>
{
  T Foo();
}

class MyBase { }

class MyDerived { }

class ProducesMyDerived : ICovariant<MyDerived>
{
  MyDerived Foo()
  {
    //get a MyDerived object and return it.
  }
}
```

Given an @Rezolver.IContainer called `container`, we can now do this:

```cs
container.RegisterType<ProducesMyDerived, ICovariant<MyDerived>();

var result = container.Resolve<ICovariant<MyBase>>();
```

Assuming no other registrations exist, `result` will be an instance of our `ProducesMyDerived` class in the
previous code block.

## Last-registered wins

> [!NOTE]
> If a registration exists for the exact type requested, then covariance is ignored in favour of the 
> most-recently registered exact match.

Just as with all other registrations (except [contravariance](contravariance.md)), the registration that 
serves a request for a particular service type *which is matched covariantly* is the one that was registered 
most recently.

So a `Func<MyBase>` registration will supersede a `Func<MyDerived>` registration for a 
request for a `Func<object>` service if the `Func<MyBase>` registration was made last.

# Examples

## Constant `Func<out T>` service

Here's an example, in unit test form, similar to the type of covariance we've just covered above, this time 
with delegates:

[!code-csharp[CovarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/CovarianceExamples.cs#example1)]

> [!NOTE]
> Notice that with delegates, you typically register the delegate directly against its actual type - the 
> 'service type' for the registration is exactly the same as the delegate type itself.

## Enumerables

Single-service scenarios like the `MyBase`/`MyDerived` example above are less common with covariance in the 
IOC world.  The most common example is with `IEnumerable<T>` functionality - where an application has several 
registrations for concrete types which all happen to share a common base or interface, and your application 
wants to be able to resolve them all automatically by that base or interface whilst still also needing to 
be able to resolve them by their concrete types.

Clearly, without covariance, this could be achieved by creating two separate registrations for the same type - 
one against the concrete type and one against the common base/interface.

However, since `IEnumerable<T>` is covariant, any registration whose type is reference compatible with the `T`
really should be *automatically* identified and included in the enumerable.

Rezolver's automatic [enumerable handling](../enumerables.md) supports this without any effort from you:

[!code-csharp[CovarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/CovarianceExamples.cs#example2)]

### Nested Generic Covariance

This can be extended even further when the element type of an enumerable is itself a generic which
contains one or more covariant type parameters.

This is similar to the above example, except this time we have a series of concrete `Func<out T>` registrations:

[!code-csharp[CovarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/CovarianceExamples.cs#example3)]

### Nested Generic Contravariance

You might be wondering why we'd have an example about contravariance in the section about covariance - well
an enumerable of `Action<T>`, for example, can still match covariantly:

[!code-csharp[CovarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/CovarianceExamples.cs#example4)]

> [!TIP]
> When you start combining different types of variance you very quickly encounter counter-intuitive scenarios.
> In this case, try not to be distracted by the fact that the types in the `Action<>` delegate appear to be
> going the 'wrong way' up or down a type hierarchy.
> 
> Remember that variance is all about _**reference-compatibility**_, not specifically about whether two types share
> a common base or interface.  So, since an instance of `Action<MyService>` is reference compatible with a
> variable of type `Action<IMyService>`, this means that an `IEnumerable<MyService>` can also contain an
> instance of `Action<IMyService>`.
> ***
> If you're already familiar with the term 'reference compatible' then this will be of no surprise to you
> after learning that Rezolver supports generic variance :wink: