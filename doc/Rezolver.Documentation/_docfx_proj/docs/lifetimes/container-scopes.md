# Container Scopes

Before we start looking at [scoped objects](scoped.md), we should first look at how Rezolver's @Rezolver.IContainerScope interface, and
its implementations, are used to implement lifetime scoping.

## What's the purpose of a scope?

A scope is a disposable object whose main purpose, as mentioned in the [introduction to this section](index.md), is to track and dispose 
any `IDisposable` objects produced by the container through that scope.

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

If you've used other IOC containers, then you'll probably already have guessed how to create a new scope.

Formally, the @Rezolver.IScopeFactory interface is how you create a scope - through its @Rezolver.IScopeFactory.CreateScope* method.

The interface is implemented by:

- @Rezolver.IContainer
- @Rezolver.IContainerScope (because scopes are hierarchical)
- @Rezolver.ResolveContext

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

## Implicitly scoped transient object

This is the simplest example possible:

[!code-csharp[ImplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ImplicitScopeExamples.cs#example1)]

