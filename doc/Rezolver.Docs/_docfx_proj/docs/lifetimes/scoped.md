# Scoped Objects

If you've been reading through [this section](index.md) in order, then you're probably already familiar with the purpose of a scoped object.

If you haven't already, then its suggested you read the [singletons topic](singleton.md) (because the way you create scoped registrations is fundamentally
identical), then the [container scopes topic](container-scopes.md) so that you know how to create a scope and what you can expect from it.

- A scoped object is a singleton enforced at the scope level
- The @Rezolver.ITarget.ScopeBehaviour of an @Rezolver.ITarget that creates a scoped object will be set to @Rezolver.ScopeBehaviour.Explicit.
- Although often used with objects that support the @System.IDisposable interface; any object can be registered as 'scoped'
- Any target can be made to produce a scoped object by wrapping it with a @Rezolver.Targets.ScopedTarget (either through its constructor or via the
@Rezolver.Target.Scoped* extension method)
- You can also register scoped types (i.e. a shortcut for creating a @Rezolver.Targets.ConstructorTarget or @Rezolver.Targets.GenericConstructorTarget
which is then wrapped in a <xref:Rezolver.Targets.ScopedTarget>) with the @Rezolver.TargetContainerExtensions.RegisterScoped* extension methods.

Apart from the fact that a scoped object limits itself to one-per-scope (and one registration can, therefore, produce multiple instances across multiple
scopes), the big difference between it and a singleton is that a scope must be available when you resolve an instance.

# Examples

We don't need many examples to show scoped objects in action.

You can apply the @Rezolver.Targets.ScopedTarget to all the same targets to which 
the @Rezolver.Targets.SingletonTarget can be applied, so some of the more exotic examples from the [singletons topic](singleton.md) are not repeated
here, but with the examples we do have, you should have enough to adapt those into creating scoped objects.

Finally, all the behaviour shown regarding `IDisposables` and [scopes](container-scopes.md) works the same for these explicitly scoped objects as
they do for transient object.

> [!NOTE]
> In all these examples, we will use the @Rezolver.ScopedContainer so that we always have a scope available even if we don't create one.

## Basic example

[!code-csharp[ExplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ExplicitScopeExamples.cs#example1)]

## Different scope = a different object

This example shows how different instances are returned for different scopes for the same registration:

[!code-csharp[ExplicitScopeExamples.cs](../../../../../test/Rezolver.Tests.Examples/ExplicitScopeExamples.cs#example2)]

# Wrapping up

And that's it for lifetimes, [return to the topic homepage](index.md) if you need more information.