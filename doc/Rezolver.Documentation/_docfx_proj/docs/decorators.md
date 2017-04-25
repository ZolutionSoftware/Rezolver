# Decorators

The [decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern) is a key part of many software projects, 
and Rezolver offers first-class support for implementing it either non-generically, or generically.

## Registering decorators in Rezolver

Registration of Decorators in Rezolver is done through the overloaded extension method 
@Rezolver.DecoratorTargetContainerExtensions.RegisterDecorator*.  There is a generic and non-generic version
(as is typical for nearly all Rezolver registration functions), but, unlike many other target types,
there is no factory method in the @Rezolver.Target static class to create one on its own.

The sharp-eyed developer will be confused by this omission, especially since the type 
@Rezolver.Targets.DecoratorTarget is a key part of Rezolver's implementation of decorators - but if 
you read the summary notes about that class, it should be apparent why: Decoration isn't *solely* 
implemented through an @Rezolver.ITarget implementation, it also needs a special target container 
type in which that target will be registered -  
@Rezolver.DecoratingTargetContainer, the creation of which is handled automatically by the 
@Rezolver.DecoratorTargetContainerExtensions.RegisterDecorator* methods.

# Examples

For the first few examples, we'll be using these decorators for the interface `IMyService`, which 
has also been used in many of the other examples.  If you've already looked at the 
[enumerable examples](enumerables.md) then you'll be familiar with them:

[!code-csharp[MyServiceDecorators.cs](../../../../test/Rezolver.Tests.Examples/Types/MyServiceDecorators.cs#example)]

***

## Single decorator, single service

Here, we simply register an `IMyService` implementation and one of our decorators.  We then resolve an 
instance and we should get an instance of the decorator, with the original `IMyService` as its `Inner`.

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example1)]

***

## Two decorators, single service

You can also 'stack' decorators for a given type, meaning you can decorate the decorators:

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example2)]

As the test shows, the decorators are applied in the order they are registered for the given type.

***

## Two decorators, enumerable

Decorators are also applied to elements of an enumerable:

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example3)]

***

## Open Generic Decorator

You can also use decorators with open generics - register them just like [normal generic types](constructor-injection/generics.md), except
via the @Rezolver.DecoratorTargetContainerExtensions.RegisterDecorator* method.

For this, our basic generic decorator is this:

[!code-csharp[MyServiceDecorators.cs](../../../../test/Rezolver.Tests.Examples/Types/UsesAnyServiceDecorator.cs#example)]

And the code to register it shouldn't be too much of a surprise:

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example4)]

> [!TIP]
> Enumerables of open generics can also be decorated in a similar way to the previous example. Just register multiple 
> `IService<>` registrations, then one or more open generic decorators as normal - then an `IEnumerable<IService<Foo>>` 
> will produce an enumerable with each element correctly decorated.

***

## Mixing Open Generic and Closed Decorators

> [!NOTE]
> This is new in v1.2 after [this issue](https://github.com/ZolutionSoftware/Rezolver/issues/27) was fixed.

Given a type `Foo<T> : IFoo<T>` which will be resolved by interface, e.g: `IFoo<Bar>`, there are two ways in which
we can decorate it:

- By registering a decorator for `IFoo<Bar>`
- By registering a decorator for the open generic `IFoo<>`

Resolver allows you to mix both approaches, so that you can have one or more catch-all decorators for the open generic, as well
as decorators for specific closed versions of that generic.  When Resolver produces the underlying instance, it will be decorated
with each decorator that applies for the type:

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example5)]

In the above example, we create a registration for the interface `IUsesAnyService<>`, and then _*two*_ decorators: the first 
also for `IUsesAnyService<>` and a second which only applies to `IUsesAnyService<MyService2>`.

We then resolve two instances - an `IUsesAnyService<MyService2>`, which is decorated twice, and an instance of 
`IUsesAnyService<MyService1>`, which is decorated only once.

### Ordering of Open and Closed Generic Decorators

When mixing open and closed generic decorators, the decorators are always applied in the order they are registered, regardless of
whether the decorator is registered against the closed or open generic.

So if you register decorators for the following types:

1. `IFoo<>`
2. `IFoo<Bar>`
3. `IFoo<>`

An instance of `IFoo<Bar>` will be decorated by all three in that same order, whereas an instance of `IFoo<Baz>` will only 
be decorated by decorators 1 and 3, in that order.

The following example demonstrates the `IFoo<Bar>` scenario, for completeness:

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example6)]