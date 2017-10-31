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

Just as with all other registrations (except [contravariance](contravariance.md)), the registration that 
serves a request for a particular service type which is matched covariantly is the one that was registered 
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

Single-service scenarios like the `MyBase`/`MyDerived` example above are less common in the IOC world.  The
most common example is with `IEnumerable<T>` functionality - where an application has several registrations
for concrete types which all happen to share a common base or interface, and your application wants to be able
to resolve them all automatically by that base or interface (without requiring that they are registered 
against it).

Rezolver's automatic [enumerable handling](../enumerables.md) supports this without any effort from you:

