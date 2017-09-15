# Automatic creation/injection of arrays

Arrays of objects can be injected automatically by Rezolver just the same as `IEnumerable<T>`
instances.  Therefore, if you read through the [enumerables documentation](../enumerables.md),
all of the examples there can be repeated with the corresponding array type - e.g. `int[]`
instead of `IEnumerable<int>`.

By default, also, an empty container (or one which has not had any specific registrations
for the array's element type) will automatically build an empty array:

[!code-csharp[ArrayExamples.cs](../../../../../test/Rezolver.Tests.Examples/ArrayExamples.cs#example1)]

And, as you would expect, as soon as you start registering objects of a particular type,
you can inject an array of that type:

[!code-csharp[ArrayExamples.cs](../../../../../test/Rezolver.Tests.Examples/ArrayExamples.cs#example2)]

All the other enumerable behaviour - such as [open generics](../enumerables/generics.md#open-generics) 
etc also applies to arrays.

# Decorating arrays

> [!TIP]
> Decoration of array _**elements**_ is exactly the same as it is for enumerables - if you register
> a decorator for a particular type, then it will decorate every instance of that type when
> an enumerable is produced - see ['decorators and enumerables'](../enumerables.md#decorators-and-enumerables)
> for more on this.

If you want to decorate an array instance (e.g. to pad an array with certain values, perhaps), you cannot
to it using a decorator class - for the obvious reason that you cannot inherit from .Net's array type.

Instead, you will need to use [decorator delegates](../decorators/delegates.md) - a delegate that 
will be executed every time an instance of a given type is produced by the container - which will
be passed the undecorated instance as an argument and which returns a (potentially different) instance 
to be used in its place.

# Disabling Array Injection

As with many other Rezolver behaviours, the automatic injection of arrays can be enabled (the default)
and disabled via an option - however, it is an option that must be applied when the container's 
@Rezolver.ITargetContainer is created, and before some of the core configurations have been applied
to it.

To do this, you need to supply an instance of @Rezolver.ITargetContainer (typically 
<xref:Rezolver.TargetContainer>) to your @Rezolver.Container
(or <xref:Rezolver.ScopedContainer>) to which you pass a specific @Rezolver.ITargetContainerConfig
in which you add a configuration object that will set the @Rezolver.Options.EnableArrayInjection
option to `false`.

> [!TIP]
> You can of course supply an empty @Rezolver.ITargetContainerConfig to the @Rezolver.TargetContainer
> on construction, but this will also disable other configurations such as enumerable injection, 
> contravariance and so on.

The easiest way to do this, then, is to use the @Rezolver.TargetContainer.DefaultConfig collection - 
either manipulate it directly (which will change the options for all @Rezolver.TargetContainer instances) 
or use it as a starting point for a custom configuration - which what this example shows, using the 
`Clone()` method, followed by the `ConfigureOption<T>` extension method:

[!code-csharp[ArrayExamples.cs](../../../../../test/Rezolver.Tests.Examples/ArrayExamples.cs#example3)]

> [!TIP]
> Another option is to remove the @Rezolver.Configuration.InjectArrays configuration from the 
> @Rezolver.TargetContainer.DefaultConfig or a clone of it.

***

# See Also

- [Enumerables](../enumerables.md)
- [Lists](lists.md)
- [Collections](collections.md)