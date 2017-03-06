# Objects as Services

Sometimes you will want to create an object yourself and register that in the container for use as
a particular service type.

This is done either by creating an @Rezolver.Targets.ObjectTarget and registering it in a target 
container, or via the extension method(s) @Rezolver.ObjectTargetContainerExtensions.RegisterObject*.

This type of registration is often thought of as a singleton, but it's more accurate to think of
it as a 'constant' service - because, from the moment the registration is created, the object reference
is already known and doesn't change.  Singletons are different because the container
creates the instance for you.

We don't need many examples for this, first - this is how you'd register an object against its 
runtime type:

[!code-csharp[ObjectExamples.cs](../../../../test/Rezolver.Tests.Examples/ObjectExamples.cs#example1)]

And if you want to register it against a different type, the following example shows one way, with another shown in comments:

[!code-csharp[ObjectExamples.cs](../../../../test/Rezolver.Tests.Examples/ObjectExamples.cs#example2)]

That's all there is to it, really.  Although before we move on, we should take a quick look at how object-based services interact with scopes.

* * *

# Objects in Scopes

A scope (obtained either by using @Rezolver.ScopedContainer or through a @Rezolver.IContainerScope returned from a container's 
@Rezolver.IContainer.CreateScope* method) will by default dispose of any disposable objects it creates when it is disposed.

If those objects are obtained from an @Rezolver.Targets.ObjectTarget, however, then no scope will ever touch them - because *you* created them,
therefore you will also typically dispose them.

Here's an example, using the aforementioned @Rezolver.ScopedContainer:

[!code-csharp[ObjectExamples.cs](../../../../test/Rezolver.Tests.Examples/ObjectExamples.cs#example10)]

As the last assertion proves - the supplied object is left untouched by the scoped container.

You can change this behaviour, however, by providing a different @Rezolver.ScopeBehaviour when creating the target (directly registering with 
the `RegisterObject` method):

[!code-csharp[ObjectExamples.cs](../../../../test/Rezolver.Tests.Examples/ObjectExamples.cs#example11)]

Notice the `scopeBehaviour:` named argument on the 4th line.  To enable the correct disposal behaviour, you must use `Explicit`
because you need the scope to know that only one instance should ever be tracked per scope.

> [!WARNING]
> If you pass `Implicit` then disposal will still work, although there's a risk that the object will be tracked by the scope multiple times
> and, therefore, could be disposed multiple times.
> 
> This behaviour is unintentional and will be prevented in a future release by raising an exception if the `Implicit` 
> behaviour is set on an @Rezolver.Targets.ObjectTarget.

* * *

## Child scopes

For the seasoned IOC container-user the last phrase of the previous section - 'one instance should ever be tracked *per scope*' - will likely be 
setting alarm bells ringing.

As with Singletons, Rezolver knows that a service implemented by a constant object should only ever be tracked in the root-most scope so that the 
disposal of a child scope does not also cause the disposal of the object.

To demonstrate this, another test, which uses a @Rezolver.ScopedContainer as before, but which resolves the object only through a child scope which
is then immediately disposed.  The test shows that the object, however, is not disposed of until the root-most scope - the @Rezolver.ScopedContainer -
is disposed:

[!code-csharp[ObjectExamples.cs](../../../../test/Rezolver.Tests.Examples/ObjectExamples.cs#example11)]

* * *

# Next steps

Feel free to explore the table of contents or [head back to the main service registration overview](service-registration.md) to explore 
more features of Rezolver.