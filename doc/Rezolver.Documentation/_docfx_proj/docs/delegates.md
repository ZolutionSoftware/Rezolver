# Factory Delegates

You can register delegates in a Rezolver container so that, when the associated service type is resolved, your
delegate will be executed and its result returned as the instance.

The delegate can be of any type, subject to the following constraints:

- The delegate must have non-`void` return type
- It must not have any `ref` or `out` parameters

To register delegates you can use one of the many @Rezolver.DelegateTargetContainerExtensions.RegisterDelegate* extension methods for @Rezolver.ITargetContainer.

To create delegate targets you can either:

- Manually create an instance of @Rezolver.Targets.DelegateTarget through its constructor
- Use the @Rezolver.Target.ForDelegate* overload, which provides specialisations for variants of the `System.Func<>` generic delegate

# Basic Examples

Here are some straightforward examples

## `RegisterType` analogue

In this example, we simply register a delegate that fires a type's constructor:

[!code-csharp[DelegateExamples.cs](../../../../test/Rezolver.Tests.Examples/DelegateExamples.cs#example1)]

## As a Singleton

We can also use the @Rezolver.Target.Singleton* and the @Rezolver.Target.Scoped* extension methods to modify the lifetime of the
object produced by the delegate:

[!code-csharp[DelegateExamples.cs](../../../../test/Rezolver.Tests.Examples/DelegateExamples.cs#example2)]

## Honouring Scopes (<xref:Rezolver.IContainerScope>)

As you would expect, if the object is resolved through a scope, then it will be tracked and disposed by that scope when it is disposed:

[!code-csharp[DelegateExamples.cs](../../../../test/Rezolver.Tests.Examples/DelegateExamples.cs#example3)]

# Advanced Examples

## Explicit Service Location

Sometimes you might need to fetch a service inside your delegate.

> [!NOTE]
> Many people consider it bad practise to have any form of 'service location' (explicitly calling a container/factory to create an instance of something
> for you).
> 
> Excessive use of this pattern does indeed have problems - not least because there's no real way to determine an object's dependencies without looking at its code - but
> in some scenarios (e.g. general purpose frameworks etc) being able to use service location through your container can be vitally important.

TODO!

## Injecting `IScopeFactory`

The @Rezolver.IScopeFactory interface is implemented by @Rezolver.ContainerBase (base class of all containers), @Rezolver.ContainerScope and - more importantly -
the @Rezolver.ResolveContext class.