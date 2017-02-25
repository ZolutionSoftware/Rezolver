# Registering services

Let's look at the different types of registrations you can perform on an @Rezolver.ITargetContainer, which means looking
through all the different types of @Rezolver.ITarget implementations we have available, and how registrations are actually
performed.

> [!TIP]
> A ***target*** (<xref:Rezolver.ITarget>) is an object stored within an @Rezolver.ITargetContainer which contains 
> information about how an object is to be created or retrieved when a container's @Rezolver.IContainer.Resolve* 
> operation is called.

To start with, we'll take a look at the @Rezolver.ITargetContainer interface and our default implementation, 
@Rezolver.TargetContainer - which contains the core registration API, as well as some extra functionality which
might be useful to some.

## Registering directly via @Rezolver.ITargetContainer.Register*

It's the @Rezolver.ITargetContainer interface which supplies service registrations when using the default
container types @Rezolver.Container and @Rezolver.ScopedContainer.  All the target container does is to provide a means
to register and look up @Rezolver.ITarget instances which have been registered against specific types.

When you @Rezolver.IContainer.Resolve* an instance from a container which uses that target container, it performs this 
lookup (via the @Rezolver.TargetContainer.Fetch* method) and, if it's successful, compiles 
the target into an @Rezolver.ICompiledTarget, whose @Rezolver.ICompiledTarget.GetObject* method is then used for that type 
for the life of the container (when cached - via the shared base class <xref:Rezolver.CachingContainerBase>).

To add registrations, we ultimately use the @Rezolver.ITargetContainer.Register* method, which accepts an
@Rezolver.ITarget object and an optional `Type` against which that target is to be registered.

If the optional type is not provided, then the target's @Rezolver.ITarget.DeclaredType is used as the default
registration type:

> [!NOTE]
> Here, we're using the @Rezolver.Targets.ObjectTarget.  We have [more in-depth documentation about `ObjectTarget`](objects.md)
> if you want to know more.

[!code-csharp[MyService.cs](../../../../../test/Rezolver.Tests.Examples/TargetContainerExamples.cs#example1)]

In this previous example, the target container defaults to `System.String` as the registration type for the target
because that's its declared type.  We can also provide any base or interface of the target's type as a valid type, too:

> [!NOTE]
> This time we're creating a @Rezolver.Targets.ConstructorTarget via one of its static factory functions.
> Again, this target [is covered in more depth here](constructors.md).

[!code-csharp[MyService.cs](../../../../../test/Rezolver.Tests.Examples/TargetContainerExamples.cs#example2)]

*The class `MyService` implements the interface `IMyService` in this example*

If you attempt to register a target against a type which the target does not support (checked by the target's
@Rezolver.ITarget.SupportsType* function), then an exception will occur:

[!code-csharp[MyService.cs](../../../../../test/Rezolver.Tests.Examples/TargetContainerExamples.cs#example3)]

## Registering via extension methods

Rezolver, like most IOC containers, also provides additional registration extension methods that abstract away 
the @Rezolver.ITarget which will ultimately be created and stored.

The majority of the code samples shown on this site use these registration methods, which are documented in the API section
of this site:

- @Rezolver.AliasTargetContainerExtensions *- for registering aliases of one type to another (useful for reusing singletons for multiple types)*
- @Rezolver.DecoratorTargetContainerExtensions *- for registering decorators*
- @Rezolver.DelegateTargetContainerExtensions *- for registering factory delegates*
- @Rezolver.ExpressionTargetContainerExtensions *- for registering expression trees*
- @Rezolver.MultipleTargetContainerExtensions *- for batch registering multiple targets*
- @Rezolver.ObjectTargetContainerExtensions *- for registering object references/values*
- @Rezolver.RegisterTypeTargetContainerExtensions *- for registering constructor-injected types (plain and open-generic)*
- @Rezolver.ScopedTargetContainerExtensions *- for registering scoped constructor-injected types*
- @Rezolver.SingletonTargetContainerExtensions *- for registering singleton constructor-injected types*

These methods extend @Rezolver.ITargetContainer or @Rezolver.ITargetContainerOwner,
and they all ultimately will create one of the many target types in the @Rezolver.Targets namespace (check the other topics
in the table of contents to the left or above - most of these extension methods are demonstrated alongisde the target ).


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