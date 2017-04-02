# Decorators

The [decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern) is a key part of many software projects, 
and Rezolver offers first-class support for implementing it, but only if you have a reference to the 
@Rezolver.ITargetContainerOwner interface - which you will, if you use the @Rezolver.TargetContainer class directly 
to register your targets instead of simply creating a new @Rezolver.Container instance.

> [!NOTE]
> This limitation will be removed in 1.2 - in which [issue #25](https://github.com/ZolutionSoftware/Rezolver/issues/25)
> will be addressed - so that the extension methods shown in these examples will be available on the 
> @Rezolver.ITargetContainer interface instead, with a @System.NotSupportedException being thrown if the target 
> container on which you call the method is not also a @Rezolver.ITargetContainerOwner.

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

Now, as mentioned in the introduction, you cannot *currently* register a decorator directly into a
@Rezolver.IContainer -  because it does not (and will never) support the interface required by the
extension methods.  As a result, the examples here do not follow the same pattern that you should already
be familiar with:

```cs
var container = new Container();

// Add registrations to the container through its implementation of
// Rezolver.ITargetContainer.

var ifoo = container.Resolve<some_type>();
```

Instead, until v1.2 we have to create a @Rezolver.TargetContainer separately on which we will perform our
registrations, and then construct a new @Rezolver.Container, passing those targets as
a constructor argument:

```cs
var targets = new TargetContainer();
targets.RegisterType<Foo, IFoo>(); 
targets.RegisterDecorator<FooDecorator, IFoo>();

var container = new Container(targets);

var result = container.Resolve<IFoo>();
```

Although this is somewhat inconvenient, it should be said that if you are looking to use Rezolver's
[integration with Asp.Net Core](nuget-packages/rezolver.microsoft.aspnetcore.hosting.md), you almost certainly
*will* be working with a target container when performing registrations anyway.

# Examples

For the first few examples, we'll be using these decorators for the interface `IMyService`, which 
has also been used in many of the other examples.  If you've already looked at the 
[enumerable examples](enumerables.md) then you'll be familiar with them:

[!code-csharp[MyServiceDecorators.cs](../../../../test/Rezolver.Tests.Examples/Types/MyServiceDecorators.cs#example)]

## Single decorator, single service

Here, we simply register an `IMyService` implementation and one of our decorators.  We then resolve an 
instance and we should get an instance of the decorator, with the original `IMyService` as its `Inner`.

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example1)]

## Two decorators, single service

You can also 'stack' decorators for a given type, meaning you can decorate the decorators:

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example2)]

As the test shows, the decorators are applied in the order they are registered for the given type.

## Two decorators, enumerable

Decorators are also applied to elements of an enumerable:

[!code-csharp[DecoratorExamples.cs](../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example3)]

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

## Specialised Generic Decorator

> [!NOTE]
> This section is intended to show how you can apply decorators to specific closed variants of an open generic, however,
> whilst preparing the example, [an issue](https://github.com/ZolutionSoftware/Rezolver/issues/27) was discovered in the 
> implementation.
> 
> Not in the *least* bit embarrassing! :)
> 
> A fix is planned for v1.2, and is right at the top of the priority list - which you can see on the 
> [project board](http://waffle.io/ZolutionSoftware/Rezolver)