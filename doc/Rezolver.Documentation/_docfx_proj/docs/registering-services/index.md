# Registering services (with targets)

Now that we've learned how to [create and use our container](../create-and-use-a-container.md), it's time to take a 
look at the different types of registrations you can perform on an @Rezolver.ITargetContainer, which means looking
through all the different types of @Rezolver.ITarget implementations we have available.

> [!TIP]
> It's the @Rezolver.ITargetContainer interface which supplies service registrations when using the default
> container types @Rezolver.Container and @Rezolver.ScopedContainer.  All the target container does is to provide a means
> to register and look up @Rezolver.ITarget instances against a type.  When you @Rezolver.IContainer.Resolve* an instance
> from the container, it performs this lookup and, if it's successful, compiles the target returned from the target container
> into an @Rezolver.ICompiledTarget, whose @Rezolver.ICompiledTarget.GetObject* method is then used for that type for the 
> life of the container (courtesy of the <xref:Rezolver.CachingContainerBase>)

Whether you want to use [constructor injection](constructors.md), an [object you've built yourself](objects.md), an 
[expression tree](expressions.md) or a [factory delegate](delegates.md), plus more, there's lots of ways to get 
Rezolver to build the services you want to use in your application.

Use the table of contents to the left (or above if on a small screen) to select the type of registration you want to
learn more about.

All the targets used by default in Rezolver to create objects can be found in the @Rezolver.Targets namespace.

## Implementing targets

You can also implement @Rezolver.ITarget yourself if you're feeling adventurous - but you must provide a way for the
container to compile your target into an @Rezolver.ICompiledTarget that can be used at resolve-time.  Documentation 
on how to do this will be added to this guide in the future.

## Short-circuited targets

Rezolver containers also support short-circuited, 'direct' targets which bypass the compilation process when returned
from the @Rezolver.ITargetContainer when attempting to fulfil a @Rezolver.IContainer.Resolve* operation.

Specifically:

- If the target also supports the @Rezolver.ICompiledTarget interface, then its @Rezolver.ICompiledTarget.GetObject* method
will be used to get the object.
- If the target can be cast to the type originally requested through the @Rezolver.IContainer.Resolve* method, then target
will be returned as the object.

The framework exploits both of these techniques to use the container as the source of its own services and configuration.