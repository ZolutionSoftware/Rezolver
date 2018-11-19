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

Delegates are a useful tool in the context of IOC containers as support for them gives you the opportunity to perform much more complex
tasks in order to resolve an object than the functionality offered by the standard targets in Rezolver.

Whether you *should* perform excessively complex logic in these factories is a topic of debate.  Our view is that
you should be able to if you want, or need, to, so Rezolver's support for delegates is extensive, including the ability to 
inject arguments to your delegate from the container, or resolve additional services inside your delegate through @Rezolver.IResolveContext.

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

## Argument injection

As mentioned in the introduction, Rezolver can inject arguments into your factory delegates just like they were constructors bound by 
@Rezolver.Targets.ConstructorTarget or @Rezolver.Targets.GenericConstructorTarget.

[!code-csharp[DelegateExamples.cs](../../../../test/Rezolver.Tests.Examples/DelegateExamples.cs#example10)]

**_Any_ delegate type, with any number of parameters, is supported, so long as it has a return type.**

## Injecting @Rezolver.IResolveContext

All @Rezolver.IContainer.Resolve* operations have an @Rezolver.IResolveContext at their heart.  Through the context, you can get the 
@Rezolver.IResolveContext.Container that originally received the call, the @Rezolver.IResolveContext.Scope and the 
@Rezolver.IResolveContext.RequestedType. It can also be used to create a new child scope (through its implementation of the 
@Rezolver.IScopeFactory interface).

If you need the context to be passed to your delegate - just make sure to declare a parameter of that type, most commonly
you'll probably use the single parameter specialisation of the @Rezolver.DelegateTargetContainerExtensions.RegisterDelegate* or
@Rezolver.Target.ForDelegate* methods, but ultimately the parameter can appear anywhere in the delegate's signature and it will
be injected.

This example shows the `Func<IRezolveContext, TResult>` overload in action:

[!code-csharp[DelegateExamples.cs](../../../../test/Rezolver.Tests.Examples/DelegateExamples.cs#example11)]

## Resolving inside a delegate

Building on the above, here's an example which injects the @Rezolver.IResolveContext in order to perform a late-bound 
@Rezolver.IResolveContext.Resolve* operation to inject a different dependency based on some ambient information about a user (which
is also injected into the delegate).

This is quite a long example which, admittedly, can be solved in a few different ways.  We're not saying this is the only way :)

[!code-csharp[AppPrincipal.cs](../../../../test/Rezolver.Tests.Examples/Types/AppPrincipal.cs#example)]

[!code-csharp[UserActionsServices.cs](../../../../test/Rezolver.Tests.Examples/Types/UserActionsServices.cs#example)]

The goal is to create a `UserControlPanel` which is correct for the current User (identified by a static property `CurrentPrincipal`),
so that it has access to a list of actions the user can perform based on their role.  The `UserControlPanel` class requires an
`IUserActionsService` which provides access to that list of actions, and we have three separate implementations of that interface
which we could use, based on the user's role.

We will use the @RezolveContext.Resolve* operation to create the instance we want to inject after deciding which type to resolve
based on the user's role:

[!code-csharp[DelegateExamples.cs](../../../../test/Rezolver.Tests.Examples/DelegateExamples.cs#example12)]

# Next Steps

That's about it for delegate registrations.  There are plans to add functionality to decorate instances via delegate, but in the meantime
feel free to explore the table of contents or [head back to the main service registration overview](service-registration.md) to explore 
more features of Rezolver.
