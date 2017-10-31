# Generic Variance in Rezolver

Rezolver, as of 1.3.2, supports both [covariant](covariance.md) and [contravariant](contravariance.md) type 
parameters in generic types.

It supports these transparently - i.e. you don't need to do anything special to enable the support (but it can 
be disabled) except to request types which have type arguments specified for parameters declared as either
`in` or `out` as per normal generic variance rules.

Follow the links above or below to see examples of variance in action in Rezolver.

- [Contravariance](contravariance.md)
- [Covariance](covariance.md)

> [!TIP]
> If you don't know anything about generic variance in .Net then you should read through
> the [MSDN topic 'Covariance and Contravariance in Generics'](https://docs.microsoft.com/en-us/dotnet/standard/generics/covariance-and-contravariance)
> as it explains these concepts in far greater detail than we can!

# Mixed Variance Not (*yet*) Supported

At present, mixed variance - that is, combinations of contravariance and covariance in the same generic type
is not yet supported.

This means that while .Net allows you to do this:

```cs
Func<string, object> f
  = new Func<object, string>(o => o.ToString());
```

Rezolver will not be able to resolve a `Func<string, object>` from a `Func<object, string>` registration.

You *can*, however, resolve a `Func<object, object>` in this scenario or, indeed a `Func<string, string>` - it's
types which mix combinations of both covariant and contravariant type arguments which won't find a match (so - 
all-covariant or all-contravariant combinations also work).

There are some failing tests in the current codebase which track this functionality, and we'll look at getting
it implemented soon.