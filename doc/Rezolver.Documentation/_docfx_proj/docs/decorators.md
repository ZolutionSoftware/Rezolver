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