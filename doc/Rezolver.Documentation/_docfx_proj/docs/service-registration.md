# Registering services

Registering services ultimately means adding @Rezolver.ITarget objects to an @Rezolver.ITargetContainer.

The different built-in implementations of @Rezolver.ITarget provide us with the ability to resolve objects
in different ways - and this section looks at those implementations in addition to how we actually perform
registrations.

> [!TIP]
> An <xref:Rezolver.ITarget> is an object stored within an @Rezolver.ITargetContainer which contains 
> information about how an object is to be created or retrieved when a container's @Rezolver.IContainer.Resolve* 
> operation is called.

This subject means delving into the @Rezolver.ITargetContainer interface and our default implementation, 
@Rezolver.TargetContainer - which contains the core registration API, as well as some extra functionality which
might be useful to some.

## What is @Rezolver.ITargetContainer?

The @Rezolver.ITargetContainer interface supplies service registrations when using the default
container types @Rezolver.Container and @Rezolver.ScopedContainer.  All the target container does is to provide a means
to register and look up @Rezolver.ITarget instances which have been registered against specific types.

## Registering via extension methods

The majority of your work with the Rezolver framework will use extension methods to create and add targets to
an @Rezolver.ITargetContainer which will ultimately drive the @Rezolver.IContainer that you will use to create instances.

These extension methods are more expressive than the core API (documented below), and greatly simplify your interaction
with the Rezolver framework.

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

> [!NOTE]
> Some of these methods extend @Rezolver.ITargetContainerOwner - an interface which extends @Rezolver.ITargetContainer
> to supply advanced functionality to the framework - such as decoration.

## Registering via @Rezolver.ITargetContainer.Register*

To add registrations to an @Rezolver.ITargetContainer directly, i.e. without extension methods, we ultimately use the 
@Rezolver.ITargetContainer.Register* method, which accepts an @Rezolver.ITarget object and an optional `Type` against 
which that target is to be registered.

If the optional type is not provided, then the target's @Rezolver.ITarget.DeclaredType is used as the default
registration type:

> [!NOTE]
> In the next example, we're using the @Rezolver.Targets.ObjectTarget.
> We have [more in-depth documentation about `ObjectTarget`](objects.md) if you want to know more.

[!code-csharp[TargetContainerExamples.cs](../../../../test/Rezolver.Tests.Examples/TargetContainerExamples.cs#example1)]

So, here, the target container defaults to using `System.String` as the registration type for the target
because that's its @Rezolver.ITarget.DeclaredType.  We can also provide any base or interface of the target's type
as a valid type, too:

> [!NOTE]
> This time we're creating a @Rezolver.Targets.ConstructorTarget via one of its static factory functions.
> Again, this target [is covered in more depth here](using-constructors/index.md).

[!code-csharp[TargetContainerExamples.cs](../../../../test/Rezolver.Tests.Examples/TargetContainerExamples.cs#example2)]

*The class `MyService` implements the interface `IMyService` in this example*

If you attempt to register a target against a type which the target does not support then an exception will occur:

[!code-csharp[TargetContainerExamples.cs](../../../../test/Rezolver.Tests.Examples/TargetContainerExamples.cs#example3)]

### How type compatiblity is verified

The type(s) against which you can register a target is dependant upon the @Rezolver.ITarget.DeclaredType of the target,
but, instead of performing the compatibility checks itself, the target container uses the @Rezolver.ITarget.SupportsType* 
method of the target being registered.

Here's just a few example types and the types against which we could register them, given the correct @Rezolver.ITarget 
implementation:

ITarget.DeclaredType | Can be registered as | With Targets
--- | --- | ---
`int` | `object` | Any
`int` | `int?` | Any
`int` | `IFormattable` | Any
`string` | `IEnumerable<char>` | Any
`MyService : IService` | `IService` | Any
`MyService<T>` | `MyService<T>` | @Rezolver.Targets.GenericConstructorTarget
`MyService<T>` | `MyService<string>` | @Rezolver.Targets.GenericConstructorTarget
`MyService<T> : IService<OtherService, T>` | `IService<OtherService, int>` | @Rezolver.Targets.GenericConstructorTarget

# Retrieving registrations

As illustrated by earlier examples, you can interrogate the registrations in an @Rezolver.ITargetContainer through two methods:

- @Rezolver.ITargetContainer.Fetch* - retrieves the last-registered target for the type
- @Rezolver.ITargetContainer.FetchAll* - retrieves all targets that have been registered for the type, in the order they were registered.

These same methods are used by the standard container classes when determining how to resolve an instance (or instances) for
a given type.

# Target types

The different ways in which Rezolver can create/obtain objects for your application, then, are pretty much all handled 
through the @Rezolver.ITarget interface, and the different implementations that we have for that.

Whether you want to use [constructor injection](using-constructors/index.md), an [object you've built yourself](objects.md), an 
[expression tree](expressions.md) or a [factory delegate](delegates.md), or something else, there's lots of ways to get 
Rezolver to build the services you want to use in your application.

Use the table of contents to the left (or above if on a small screen) to select the type of registration you want to
learn more about.

All the targets used by default in Rezolver to create objects can be found in the @Rezolver.Targets namespace.

## Implementing targets

You can also implement @Rezolver.ITarget yourself if you're feeling adventurous - but you must provide a way for the
container to compile your target into an @Rezolver.ICompiledTarget that can be used at resolve-time.  Documentation 
on how to do this will be added to this guide in the future.

## Short-circuited targets

Rezolver containers also support short-circuited, 'direct' targets which bypass the compilation process when attempting 
to fulfil a @Rezolver.IContainer.Resolve* operation, specifically:

- If the target also supports the @Rezolver.ICompiledTarget interface, then its @Rezolver.ICompiledTarget.GetObject* method
will be used to get the object.
- If the target can be cast to the type originally requested through the @Rezolver.IContainer.Resolve* method, then target
will be returned as the object.

The framework exploits both of these techniques to use the container as the source of its own services and configuration.