# Singletons

Rezolver's implementation of singletons has three entry points:

- The @Rezolver.SingletonTargetContainerExtensions.RegisterSingleton* overload, which offers a shortcut to registering a type via the 
@Rezolver.Targets.ConstructorTarget or @Rezolver.Targets.GenericConstructorTarget (see the 
[construction injection topic](../constructor-injection/index.md) for more) as a singleton.
- The @Rezolver.Target.Singleton* extension method, which creates a new @Rezolver.Targets.SingletonTarget that wraps the target
on which it is called, converting it into a singleton.
- The @Rezolver.Targets.SingletonTarget.%23ctor(Rezolver.ITarget) constructor

The @Rezolver.Targets.SingletonTarget enforces a lock around its inner target so that its first result is cached and then returned
for each subsequent @Rezolver.IContainer.Resolve* operation.

> [!NOTE]
> At the moment, the lifetime of a singleton is tied to the lifetime of the @Rezolver.Targets.SingletonTarget itself.
> If you only ever have one container, or if you have multiple containers but use different targets for singletons 
> of the same type, apply across the whole `AppDomain`, then you won't enounter any issues.  But if you create multiple
> containers from the same @Rezolver.ITargetContainer, then you will find that singletons will be shared between them.
> * * *
> In the future (v1.2), singletons will be unique to each container - 
> meaning that the same registration in two different containers would yield two different singletons.

* * *

# Examples

> [!TIP]
> As with many of the other examples throughout this guide, you'll find the code for these tests in the `test/Rezolver.Examples` tests
> project.

## Constructor Injection

If you are using the @Rezolver.RegisterTypeTargetContainerExtensions.RegisterType* overload, you can swap it for the 
@Rezolver.SingletonTargetContainerExtensions.RegisterSingleton* overload to register a singleton for constructor injection:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example1)]

* * *

## Constructor Injection (via @Rezolver.Target.Singleton* extension)

If you need to use some of the more advanced constructor injection functionality, such as 
[named argument binding](../constructor-injection/index.md#best-match-named-args), or 
[specific `ConstructorInfo` binding](../constructor-injection/index.md#with-a-constructorinfo), then you can't use the 
@Rezolver.SingletonTargetContainerExtensions.RegisterSingleton* overload - because that creates the @Rezolver.Targets.ConstructorTarget or
@Rezolver.Targets.GenericConstructorTarget for you.  Instead, you will create it yourself and then wrap a 
@Rezolver.Targets.SingletonTarget around it - using either its @Rezolver.Targets.SingletonTarget.%23ctor(Rezolver.ITarget) constructor, or 
the @Rezolver.Target.Singleton* @Rezolver.ITarget extension method.

> [!NOTE]
> The @Rezolver.Targets.SingletonTarget inherits its @Rezolver.ITarget.DeclaredType from its inner target - so a singleton will always be
> compatible with whatever type its inner target is compatible with.

This example is derived from the [named argument binding](../constructor-injection/index.md#best-match-named-args) constructor injection 
example mentioned above, except the `RequiresIMyServiceAndDateTime` type is registered as a singleton via the 
@Rezolver.Target.Singleton* @Rezolver.ITarget extension method:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example2)]

* * *

## Open Generics

When you register an open generic (via [generic constructor injection](../constructor-injection/generics.md)) as a singleton, then one 
singleton is created for each concrete generic type:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example3)]

* * *

## Factory Delegates

Building on the [factory delegate documentation](../delegates.md), we can also register delegates as singletons, too:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example4)]

> [!NOTE]
> Clearly this is not a real-world example - but intended to be as illustrative as possible of the effects of converting a delegate
> into a singleton: the delegate will be executed just the once.

* * *

## Expressions

If you're [registering expressions in your container](../expressions.md) then you won't be surprised to learn you can also register
these as singletons.  By now, you should pretty much already be able to guess how!

This example is similar to the delegate example above, in that we have a counter variable which is being (pre)incremented by an expression, 
with its result returned whenever we resolve an `int`.  In itself this poses an additional challenge because such an expression cannot be 
written as a compile-time lambda (assignments are not allowed).  So in this case we have a type which holds a mutable counter:

[!code-csharp[CounterHolder.cs](../../../../../test/Rezolver.Tests.Examples/Types/CounterHolder.cs#example)]

We register that as a singleton (so the counter is shared) and then build an expression by hand to return the result of pre-incrementing
it and writing the value back to the counter holder.  This example also shows another way to leverage Rezolver's ability to inject arguments 
into your expressions, by supplying a hand-built @System.Linq.Expressions.LambdaExpression with @System.Linq.Expressions.LambdaExpression.Parameters:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example5)]

* * *

## Mixing Transient and Singleton

Rezolver allows you to mix singletons and transients in a single object graph, since an individual registration controls its own lifetime.

First, we have a transient object with dependencies on objects registered as singletons (the most common case):

> [!NOTE]
> The `RequiresMyServices` type and other related types shown here are introduced in the [constructor injection](../constructor-injection/index.md)
> topic.

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example6)]

Then, the same types, but this time the `RequiresMyServices` type is registered as a singleton, with the three dependency types registered
as transient:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example7)]

* * *

## Singletons in Enumerables

Building on the examples shown in the [enumerables documentation](../enumerables.md), you can register a singleton among a collection
of services which are later resolved or injected as an enumerable of that service:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example8)]

> [!NOTE]
> At present, the enumerable itself is *always* a new instance, even if every element within it is a singleton.  There is currently
> no way to force an enumerable to be a singleton, however this ability might be added at a later date.  If, however, you
> inject the enumerable as a dependency of a singleton, then it will behave as you would expect.

* * *

## Decorating Singletons

The [decorator documentation](../decorators.md) show many permutations of Rezolver's support for the decorator pattern.  As the 
[documentation for transient objects](transient.md) mentions, it's not (currently) possible to control the lifetime of the decorators themselves,
because they are always transient.  However, as you would expect, if the original registration for the decorated type is a singleton, then 
each decorator instance that is created by the container will receive the same non-decorated service instance:

[!code-csharp[SingletonExamples.cs](../../../../../test/Rezolver.Tests.Examples/SingletonExamples.cs#example9)]

* * *

# Wrapping up (and next steps)

The key takeaway from this is that you can convert any of the targets in the @Rezolver.Targets namespace into a singleton simply by wrapping
a @Rezolver.Targets.SingletonTarget around it.  Clearly, there are some targets to which you *shouldn't* do this (the @Rezolver.Targets.ScopedTarget
and the SingletonTarget itself, for instance!) although, right now, Rezolver doesn't prevent you from doing so.

- You should now take a look at [how explicitly scoped objects](scoped.md) are supported by Rezolver.
- You might also want to see how singletons behave inside an @Rezolver.IContainerScope - which is part of the 
[container scopes](container-scopes.md) documentation.