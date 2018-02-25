# Enumerable Projections

Projections provide a way to create an enumerable of one type from an enumerable of another type, through a single
registration in the container.

For example, let's say we have a rudimentary pricing object model, in which we have one type responsible for 
applying a price adjustment, whose parameters are controlled by a configuration object:

[!code-csharp[SimplePriceAdjustment.cs](../../../../../test/Rezolver.Tests.Examples/Types/SimplePriceAdjustment.cs#example)]

> [!NOTE]
> Of course, in a real-world application, we might split fixed/percentage rules into separate
> implementations.  Also, we might have an explicit order to apply to apply them in (e.g. tax rules last).
> 
> So, remember, this example isn't meant to be used as the template for a real-world pricing calculator!

An application, then, will register one or more configuration objects (or a way to obtain an enumerable 
programmatically) and then will, at some point, want to turn that into an enumerable of adjustment objects
when calculating a price for something.

Now, clearly, the application shouldn't be depending on those configs, it should be depending on the adjustments
themselves, in short:

[!code-csharp[SimplePriceCalculator.cs](../../../../../test/Rezolver.Tests.Examples/Types/SimplePriceCalculator.cs#example)]

What Rezolver's enumerable projections allow you to do is to register the configs either as individual entries,
as has already been shown in the [initial topic in this section](../enumerables.md), or as an explicitly 
registered enumerable, and then to create a single registration for a projection from 
`IEnumerable<SimplePriceAdjustmentConfig>` to `IEnumerable<SimplePriceAdjustment>`.

When rezolved, Rezolver iterates all items in the input sequence, executing the registered target each time,
setting the newly created instance into the output sequence.  In effect, it performs a Linq 
`.Select(input => output)` operation on the input sequence.

For simple cases like this, where the type we are projecting to is always the same and always created by constructor
injection, we can use the @Rezolver.RootTargetContainerExtensions.RegisterProjection``2(Rezolver.IRootTargetContainer)
extension method:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example100)]

> [!TIP]
> This form can also be used to reuse an existing registration, which is covered in the next section.

One of the key things to note about this is that, by registering a projection, we are creating a specific 
`IEnumerable<T>` registration in the container which is *tied* to the source enumerable.  After doing this, it's
no longer possible to create additional registrations for the projected type (i.e. `SimplePriceAdjustment` in this 
case) and have them included in the output enumerable.

# More advanced projections

The @Rezolver.RootTargetContainerExtensions.RegisterProjection* overload supports many more scenarios than just
automatically generating constructor-injected instances of the same type.

## Different service/implementation types

In the same way that we can register a type for constructor injection against a specific service type using the
form `container.RegisterType<Service, IService>()` to create an instance of `Service` whenever `IService` is required,
you can also split the service and implementation types for the output enumerable of a projection:

```cs
container.RegisterProjection<Foo, IBar, Bar>();
```

This will project an enumerable of `IEnumerable<IBar>` (the second type argument) by creating instances of `Bar`
for every input instance of `Foo`.

## Reusing an existing registration

When registering a projection, Rezolver will only auto-create a constructor-injected target for the implementation
type if a target isn't found in the container for the same type.  This means that we can configure the container 
to create instances of the output type in any way that we want, and that registration will be reused by the 
projection.

To demonstrate this, we'll start moving towards a more sensible design and take advantage of the `IPriceAdjustment`
interface that we've implemented on `SimplePriceAdjustment`.  Let's add a decorator which only applies an adjustment
if the result is greater than or equal to 50% of the original price:

[!code-csharp[SimplePriceAdjustment.cs](../../../../../test/Rezolver.Tests.Examples/Types/SimplePriceAdjustment.cs#example2)]

Pretty standard stuff.

And now let's define a new calculator type which depends upon an `IEnumerable<IPriceAdjustment>` instead of an
`IEnumerable<SimplePriceAdjustment>`:

[!code-csharp[SimplePriceCalculator.cs](../../../../../test/Rezolver.Tests.Examples/Types/SimplePriceCalculator.cs#example2)]

Now we can use Rezolver's [support for decorators](../decorators.md) to decorate every projected `IPriceAdjustment`
with our rule which ensures that no combination of price adjustments will ever result more than a 50% reduction:

[!code-csharp[EnumerableExamples.cs](../../../../../test/Rezolver.Tests.Examples/EnumerableExamples.cs#example101)]

This example is worthy of some extra explanation, so let's take a look at the code line-by-line:

> [!INFO]
> You can skip this part if you're happy to accept that it 'just works'.

- First, we're registering the `SimplePriceAdjustment` type for constructor injection when an `IPriceAdjustment`
is requested.
- Then we're registering a decorator for the `IPriceAdjustment` interface which will wrap all instances produced
for that type with an instance of the `NeverLessThanHalfPrice` decorator.
- Then we register the `PriceCalculator` type.
- And then we declare the projection from `SimplePriceAdjustmentConfig` to `IPriceAdjustment`.  This means that
when an `IEnumerable<IPriceAdjustment>` is requested, the container will project a new `IPriceAdjustment` instance
for each `SimplePriceAdjustmentConfig`.
- Finally, some price configs are added

When the container is building the instances of `IPriceAdjustment` for the enumerable, it does a couple of things
behind the scenes:
- Creates a temporary `ITargetContainer` into which it registers the `SimplePriceAdjustmentConfig` that it has
obtained from the input enumerable.  _**Note**_ - It's the @Rezolver.OverridingTargetContainer that is used for this.
- Locates the correct target to execute for the service type `IPriceAdjustment` (in this case, the decorator)
- When compiling the `IPriceAdjustment` target for each item in the loop, Rezolver passes the 'extra' 
`ITargetContainer` through.  Thus, when the `SimplePriceAdjustmentConfig` dependency is resolved for the 
`SimplePriceAdjustment` constructor, it resolves the latest one obtained from the input enumerable.

* * *

# Other advanced projections

We've not yet touched on the strongest features of projections - i.e:

- @Rezolver.RootTargetContainerExtensions.RegisterProjection(Rezolver.IRootTargetContainer,System.Type,System.Type,System.Func{Rezolver.IRootTargetContainer,Rezolver.ITarget,System.Type}) -
With this function you register a projection with a callback to select the implementation type to be resolved for 
each projected element (useful for dynamically chosing generics etc).  The callback is passed an `ITarget` which 
represents the value that is to be projected.  
- @Rezolver.RootTargetContainerExtensions.RegisterProjection(Rezolver.IRootTargetContainer,System.Type,System.Type,System.Func{Rezolver.IRootTargetContainer,Rezolver.ITarget,Rezolver.ITarget}) -
And with this function you provide a callback to build a specific target for each projected element.

As suggested above, these really come into their own for scenarios such as projections of generic types.

We're not providing in-depth examples for these as we're probably at example overload by now.  There are tests in the
main tests projects which you can look at.  We might also add some examples later if there's enough demand.

The chances are, though, that most people won't need this functionality and, if you do, you're probably of a 
sufficient level to be able to figure it out for yourself.