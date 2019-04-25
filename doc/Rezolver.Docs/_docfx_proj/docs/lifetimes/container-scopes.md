# Container Scopes

Before we start looking at [scoped objects](scoped.md), we should first look at how Rezolver's @Rezolver.ContainerScope
implements lifetime scoping.

> [!TIP]
> When using the Asp.Net Core integration, per-request lifetime scoping is automatically implemented by Asp.Net Core.

## What's the purpose of a scope?

A scope is a disposable object with two main purposes:

1) Tracking and disposing instances of any `IDisposable` objects produced by the container through that scope, as mentioned in the [introduction to this section](index.md).
2) Creating and managing 'scoped singletons' - i.e. where one instance of a given type is produced per scope instance, which might also be disposable.

This is necessary, or at least desirable, because the IOC container takes control of both how *and* when your objects are created and, 
more importantly, they are more often than not created purely to be passed as dependencies into other objects via dependency injection 
(via, for example, constructor parameters).  This means that the .Net developer's preferred pattern of working with disposable objects 
(at least in C#), namely:

```cs
using(var disposable = new MyDisposable())
{
    //use disposable
}
```

is no longer possible.  Not only that, but there's the issue of *who* is responsible for disposing of a given object, especially if it 
has been used as a dependency of more than one other object.

The scope offers a way to simplify this problem by taking ownership of those disposables for you.  All you need to do is to create the scope,
and then dispose of it when you no longer need the objects it produced.

## Creating a Container Scope

You can create a scope explicitly using these `CreateScope` methods:

- [Resolver.Container.CreateScope](xref:Rezolver.Container.CreateScope*)
- [Resolver.ContainerScope.CreateScope](xref:Rezolver.ContainerScope.CreateScope*)
- [Resolver.ResolveContext.CreateScope](xref:Rezolver.ResolveContext.CreateScope*)

In the example code, therefore, you will see a lot of this:

```cs
var container = new Container();

// perform registrations

using(var scope = container.CreateScope())
{
    
}

```

## Scope Behaviours

Different targets (see the @Rezolver.Targets namespace) exhibit different scoping behaviours - that is, how the object produced by the target
should be tracked within a scope when resolved.  At present<sup>*</sup>, this is determined by the @Rezolver.ScopeBehaviour enumeration, which 
has three values:

- @Rezolver.ScopeBehaviour.Implicit - The object should be passively tracked within an enclosing scope (if present) when it is created if, 
and *only* if, it is `IDisposable`.  Each and every instance will be tracked, and none of those instances will be reused.  When the scope is
disposed, all objects that were tracked will also be disposed.
- @Rezolver.ScopeBehaviour.Explicit - The object *requires* an enclosing scope to be present when created, and only one instance of the
object should be created per scope.  If the object is `IDisposable` then it, too, will be disposed when the scope is disposed.
- @Rezolver.ScopeBehaviour.None - The object does not interact with a scope and, if it's `IDisposable`, then it must be explicitly disposed
by application code.

<sup>*</sup> _In the future, the enumeration might be replaced with a type and static instances whose names are the same as those listed here,
so that scoping behaviour can be abstracted and, therefore, made more extensible_

The behaviour of an @Rezolver.ITarget can be read through its @Rezolver.ITarget.ScopeBehaviour property - which is read only and is, for most
targets, defaulted to @Rezolver.ScopeBehaviour.Implicit.

Indeed, most implementations of the interface do not allow you to control this behaviour.  One exception that does, however, is the 
@Rezolver.Targets.ObjectTarget - which defaults to @Rezolver.ScopeBehaviour.None because *you* supply the instance
when you create the target, therefore it's assumed *you* will also take responsibility for disposing it, if it's `IDisposable`.  If you want
Rezolver to dispose of it for you, then you can pass a different behaviour when you create the target.

> [!NOTE]
> The only other scope behaviour that you *should* pass to an @Rezolver.Targets.ObjectTarget is @Rezolver.ScopeBehaviour.Explicit, because
> there's only ever one instance.  If you use @Rezolver.ScopeBehaviour.Implicit, then a scope will end up tracking the same instance multiple
> times, creating unnecessary entries in its internal tracking arrays, and the object's `Dispose` method will end up being called multiple
> times.

## Using @Rezolver.ScopeBehaviour.Explicit

There is only one target whose @Rezolver.ITarget.ScopeBehaviour is set to @Rezolver.ScopeBehaviour.Explicit - the @Rezolver.Targets.ScopedTarget.

This target is used in the same way that we use the @Rezolver.Targets.SingletonTarget (as described in the 
[singletons documentation in this section](singleton.md)) - you wrap another target with it, and it then ensures that only one instance per-scope 
is ever produced from the target.

Explicitly scoped objects are covered exclusively in [the next topic in this section](scoped.md) but before we head on over to that topic,
we will go through a few examples which involve the @Rezolver.ScopeBehaviour.Implicit behaviour - i.e, the default for most types of
registration, to see how 'normal' objects interact with scopes.

* * *

# Examples

For these examples, we're using this type:

[!code-csharp[DisposableType.cs](../../../../../test/Rezolver.Tests.Examples/Types/DisposableType.cs#example)]

Note that the type throws an exception if attempts are made to dispose of it multiple times - this is a feature of the tests which drive the examples,
as we want to prevent unnecessary `Dispose()` calls as much as possible.

We're also using this trivial type for examples where we resolve the disposable as a dependency:

[!code-csharp[RequiresDisposableType.cs](../../../../../test/Rezolver.Tests.Examples/Types/RequiresDisposableType.cs#example)]

## Implicitly scoped transient disposable

First we look at the simplest case - we create a @Rezolver.Container from which we obtain a new scope through which we then resolve an instance 
of a disposable object.  When we dispose the scope, the object should also be disposed:

[!code-csharp[ImplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ImplicitScopeExamples.cs#example1)]

* * *

## Nested child scopes

This time, the container is building the same type, but we're using child scopes - checking that each child scope *only* disposes of the objects
resolved within it:

[!code-csharp[ImplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ImplicitScopeExamples.cs#example2)]

* * *

## Disposable Dependencies

All the examples so far demonstrate disposable objects being resolved directly via a scope.  What if a disposable object is instead created
as a dependency of another?

[!code-csharp[ImplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ImplicitScopeExamples.cs#example3)]

> [!TIP]
> Remember - the scope tracks all `IDisposable` objects it creates - so it doesn't matter whether an object is created directly for a Resolve
> operation or purely as a dependency for another object - it will be disposed by the scope that owns it.

* * *

## Disposable Singletons

On the subject of ownership - a singleton has special rules regarding the scope it wants to belong to.

The singleton could be constructed at any time, inside any scope, but as we know from the [singletons documentation](singleton.md), the object 
itself must remain available as long as the container itself is available.  As a result, singletons must ensure that they are disposed by the 
root-most scope (i.e. one which does not have a parent):

[!code-csharp[ImplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ImplicitScopeExamples.cs#example4)]

> [!IMPORTANT]
> As the example shows, if you are using disposable singletons, then you should *__always__* use @Rezolver.ScopedContainer as your 
> application's container instead of @Rezolver.Container.
> 
> It supports exactly the same functionality shown in all the examples on this site, but it also has it's own scope
> which will automatically become the default root for your application, thus ensuring that any singletons will be disposed only when you
> dispose of the @Rezolver.ScopedContainer.

* * *

## Mixing lifetimes and scopes

The previous example throws up a slight problem: what happens when a singleton depends on a non-singleton disposable; and vice versa?

### Transient depending on a Singleton

When a transient object depends on a singleton disposable, then (regardless of whether the transient is Disposable too) nothing changes, the
singleton is still tracked in the root-most scope, and is not disposed until that scope is disposed - there is no difference to the singleton 
example shown above.

### Singleton depending on a Transient

> [!NOTE]
> This didn't work properly prior to v1.2.  If you are still using v1.1, then
> consider upgrading to the latest package

If the roles are reversed and a singleton takes a dependency on a transient `IDisposable`, then that transient (and any other dependencies)
will also be tracked in the root scope, so that it remains usable for the lifetime of the singleton:

[!code-csharp[ImplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ImplicitScopeExamples.cs#example5)]

In short - an object's lifetime is determined by the *longest* lifetime of any object that 'owns' that object.

* * *

# Next steps

- Learn how to create per-scope singletons with [scoped objects](scoped.md).