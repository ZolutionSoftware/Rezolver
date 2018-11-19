# Mixing Covariant and Contravariant parameters

All of our [covariant](covariance.md) and [contravariant](contravariance.md) examples have focused on variant
generic interfaces or delegates which have *all* covariant or *all* contravariant type parameters, but Rezolver 
is not limited to this.

Rezolver can also match registrations to generics whose type arguments don't match the requested types, but 
which are still compatible across *all* type arguments, regardless of whether the underlying type parameters 
are covariant or contravariant.

Consider the @System.Converter`2 type:

`public delegate TOutput Converter<in TInput, out TOutput>(TInput input);`

> [!TIP]
> These examples use the same shapes types that are used in the [contravariance](contravariance.md) examples.

In this example, we'll resolve a `Converter<Rectangle, IDictionary<string, string>>` - but neither type argument 
matches the types used in the registration (`Converter<object, Dictionary<string, string>>`).
However, because the first is contravariant and the second is covariant, Rezolver pulls out the registered 
converter, seeing that it is compatible:

[!code-csharp[MixedVarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/MixedVarianceExamples.cs#example1)]

And, of course, if you resolve an `IEnumerable<>` of a type containing a mixture of covariant and contravariant
type parameters - it works exactly as expected:

[!code-csharp[MixedVarianceExamples.cs](../../../../../test/Rezolver.Tests.Examples/MixedVarianceExamples.cs#example2)]

> [!NOTE]
> Remember that auto-generated enumerables always return objects in *registration* order.

Further examples for this functionality which don't also require lots of background and other classes are thin on 
the ground right now.  That said, generic variance is something you either *know* you need, or you don't.  So,
if you do, hopefully these examples - and the rest in this section - should be enough to satisfy you that Rezolver
will be able to support you in whatever you need.

Certainly, don't start marking all your generic parameters as `in` or `out` just because they can be - variant
interfaces and delegates are typically variant for a *specific reason*.  If you don't have a good reason, then 
don't make your types variant.