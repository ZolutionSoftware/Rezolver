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