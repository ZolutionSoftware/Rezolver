# Creating and using a container

Using Rezolver is the same as with all other IOC containers:

- Create and configure the root container
- Register the services which your application needs
- Resolve services from the container

All of this setup is ultimately conducted through just a few primary types, which we'll take a brief look at
now.

## Core types

For all the built-in container types, Rezolver splits registration and resolving responsibilities between 
two primary interfaces:

### @Rezolver.ITargetContainer

This interface describes a registry of @Rezolver.ITarget objects, keyed by type, describing the type of object 
that is to be created (and how) when a given type is requested.

It is through this interface that you setup your container with registrations, which are then later used when 
resolving objects.

You might also see @Rezolver.ITargetContainerOwner - which is a special type required by some of the more complex
registration mechanisms (decorators, for example).

> [!TIP]
> The primary implementation of this interface that you will use in your application is @Rezolver.TargetContainer.

### @Rezolver.IContainer

This is the interface through which we resolve objects.  The interface does not expose any registration 
mechanisms at all (even if the 'standard' implementations of those classes all do) - only the ability
to request objects from the container.

This interface does not mandate that a container has an `ITargetContainer`, it's simply the case
that all the provided implementations which we will discuss in the rest of this documentation do
use that interface as the source of their service registrations.

> [!TIP]
> The primary implementation of this interface is @Rezolver.ContainerBase, an abstract class which also 
> implements the @Rezolver.ITargetContainer interface by wrapping around the @Rezolver.ContainerBase.Targets 
> property that it exposes.
> 
> As a consumer of Rezolver, however, you will be using the @Rezolver.Container and @Rezolver.ScopedContainer 
> classes most of the time - which derive most of their functionality from this abstract class.

## Creating a container

There are numerous ways to create an @Rezolver.IContainer instance.  The easiest, from the point of view registering
services and then resolving them, is to create an instance of the @Rezolver.Container type.  Or, if you want your root
container to act as a global lifetime scope for explicitly scoped objects (see <xref:Rezolver.Targets.ScopedTarget>) then you
can also use @Rezolver.ScopedContainer:

```cs
void Foo()
{
    //create a standard non-scoping container
    var container = new Container();
    //or create a scoped container:
    var container = new ScopedContainer();
```

*All code samples assume you have added a `using` statement (`imports` in VB) for the `Rezolver` namespace.*

Once you have a local reference to either of these classes, you can start registering services in the container,
and resolving objects from it.

## Registering services

As mentioned above, with our default implementations of @Rezolver.IContainer, registration of services ultimately 
means adding targets to a container's @Rezolver.ContainerBase.Targets target container, associating them with service 
types which we will later resolve.

The core registration method for this is the @Rezolver.ITargetContainer.Register*
method, which accepts an @Rezolver.ITarget and an optional type against which the registration is to be made.

There's too much to cover in a few sub headings here for this - 
[the service registration topic](service-registration.md) has more detail, and links to the different types of
registration you can perform.

## Resolving services

Resolving objects from your container is done through the @Rezolver.IContainer.Resolve* method which, you'll
notice, accepts a @Rezolver.ResolveContext as its single parameter, and returns an `Object`.  Again, if 
you're familiar with IOC containers then you're probably wondering where your strongly-typed `Resolve<TService>()` 
method is!

Well, fear not.  As with the many extension methods available on @Rezolver.ITargetContainer, the @Rezolver.IContainer
interface (through which all resolving of objects is done) has extension methods to provide a more traditional
IOC API, and these are found on the @Rezolver.ContainerResolveExtensions static class.

The one you'll use most frequently, of course, is the @Rezolver.ContainerResolveExtensions.Resolve``1(Rezolver.IContainer)
method, which provides the aforementioned `Resolve<TService>()` API.

So, assuming we have the `container` that we've been using up till now, we would resolve an instance of `MyService`
simply by doing one of the following:

```cs
MyService service = container.Resolve<MyService>();
//or
MyServiceBase service = container.Resolve<MyServiceBase>();
//or
IMyService service = container.Resolve<IMyService>();
```

> [!NOTE]
> The @Rezolver.ResolveContext class is used to capture the context of a call to the `Resolve` method, tracking the
> container on which the call is originally made, whether there is an @Rezolver.IContainerScope active for the call,
> and other things besides.  You will rarely use it directly in application code unless you are extending Rezolver.
>
> These `Resolve` extension methods create the `ResolveContext` for a given operation on your behalf, so you never
> have to worry about it.

Assuming the container can locate the service registration for the type you request, it will fetch/produce an
instance of the associated object type according to the behaviour described by the @Rezolver.ITarget which we 
previously registered.

If no registration is found, then an `InvalidOperationException` is raised by the container.

> [!TIP]
> @Rezolver.IContainer also implements the `System.IServiceProvider` interface from the .Net framework, which requires
> that missing services yield a `null` result instead of throwing an exception.

### CanResolve/TryResolve

Sometimes you might want to *attempt* to resolve an object from the container, but not have an exception raised
if it cannot be found.  In this case you can use the @Rezolver.IContainer.TryResolve* method, which returns the
object via an `out` parameter and returns a `bool` indicating whether the operation succeeds.

Naturally, as with the `Resolve` operation, this method has a generic overload
(<xref:Rezolver.ContainerResolveExtensions.TryResolve``1(Rezolver.IContainer,``0@)>) to avoid the need for a 
temporary `Object` reference:

```cs
MyService result;
bool success = container.TryResolve(out result);
//success == true or false depending on registrations.
```

Similarly we can also introspect a container to find out if it *can* resolve an instance of a given type
by using the @Rezolver.IContainer.CanResolve* method.  This method also has friendly overloads via extension
methods (e.g. <xref:Rezolver.ContainerResolveExtensions.CanResolve``1(Rezolver.IContainer)>)):

```cs
bool canResolve = container.CanResolve<MyService>();
```

* * *

# Next Steps

  - Now read up on how to [register services in the container](service-registration.md).

<!--Alternatively, you might be interested in Rezolver's understanding of [object lifetimes](object-lifetimes.md).-->