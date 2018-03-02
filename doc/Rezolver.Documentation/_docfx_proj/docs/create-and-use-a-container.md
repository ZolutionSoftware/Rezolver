# Creating and using a container

Using Rezolver is the same as with all other IOC containers:

- Create and configure the root container
- Register the services which your application needs
- Resolve services from the container

All of this setup is ultimately conducted through just a few primary types, which we'll take a brief look at
now.

# Core types

For all the built-in container types, Rezolver splits registration and resolving responsibilities between 
two interfaces:

## @Rezolver.ITargetContainer

> [!NOTE]
> You'll notice that the constructors for the @Rezolver.Container and @Rezolver.ScopedContainer types (see next)
> actually accept an instance of @Rezolver.IRootTargetContainer.  This is a new interface added in v1.3.2 which
> marks a target container as a 'top-level' one - and supports functionality such as 
> [covariance](variance/covariance.md).  `IRootTargetContainer` also implies the `ITargetContainer` interface.

This interface describes a registry of @Rezolver.ITarget objects, keyed by type, describing the type of object 
that is to be created (and how) when a given type is requested.

It is through this interface that you setup your container with registrations, which are then later used when 
resolving objects.

> [!TIP]
> The primary implementation of this interface that you will use in your application is @Rezolver.TargetContainer - 
> either created implicitly by the framework or explicitly in your own code.

## @Rezolver.IContainer

This is the interface through which we resolve objects.  The interface does not expose any registration 
mechanisms at all (even if the classes providing the 'standard' implementations all do) - only the ability
to request objects from the container.

This interface does not mandate that a container has an `IRootTargetContainer`, it's simply the case
that all the provided implementations which we will discuss in the rest of this documentation do
use that interface as the source of their service registrations.

> [!TIP]
> The primary implementation of this interface is @Rezolver.ContainerBase, an abstract class which also 
> implements the @Rezolver.ITargetContainer interface by wrapping around the @Rezolver.ContainerBase.Targets 
> property that it exposes.
> 
> As a consumer of Rezolver, however, you will be using the @Rezolver.Container and @Rezolver.ScopedContainer 
> classes most of the time - which derive most of their functionality from this abstract class.

# Creating a container

There are numerous ways to create an @Rezolver.IContainer instance.  The easiest, from the point of view of registering
services and then resolving them, is to create an instance of the @Rezolver.Container type.  Or, if you want your root
container to act as a global lifetime scope for [explicitly scoped objects](lifetimes/scoped.md) then you
can also use @Rezolver.ScopedContainer:

```cs
// create a standard non-scoping container
var container = new Container();

// or create a scoped container:
var container = new ScopedContainer();
```

*All code samples assume you have added a `using` statement (`imports` in VB) for the `Rezolver` namespace.*

Once you have a local reference to either of these classes, you can start registering services via the 
container's @Rezolver.ITargetContainer implementation, and resolving objects from it.

## Registering services

As mentioned above, with our default implementations of @Rezolver.IContainer, registration of services ultimately 
means adding targets to the container's @Rezolver.ContainerBase.Targets target container, associating them with service 
types which we will later resolve.  

The core registration method for this is the @Rezolver.ITargetContainer.Register*
method, which accepts an @Rezolver.ITarget and an optional type against which the registration is to be made.

Here's an example where we register the type `Foo` to be created whenever an instance of `IFoo` is requested,
directly via a container's own implementation of @Rezolver.ITargetContainer:

```cs
var container = new Container();
container.RegisterType<Foo, IFoo>();
```

Or, if you create a dedicated @Rezolver.ITargetContainer that you specifically want the @Rezolver.Container to
use, only a small change is required:

```cs
var targets = new TargetContainer();
targets.RegisterType<Foo, IFoo>();

var container = new Container(targets);
```

> [!TIP]
> The @Rezolver.RegisterTypeTargetContainerExtensions.RegisterType* overload instructs the container to build
> an instance via [constructor injection](constructor-injection/index.md).

There's too many types of registrations to cover in a few sub headings here for this - 
[the service registration topic](service-registration.md) has more detail, with links to all the other 
different types of registration you can perform.

Moreover, you can browse the different topics from the table of contents on the left (or top on mobile).

## Resolving services

Resolving objects from your container is done through the @Rezolver.IContainer.Resolve* method which, you'll
notice, accepts an @Rezolver.IResolveContext as its single parameter, and returns an `Object`.  Again, if 
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
> The @Rezolver.IResolveContext interface captures the context of a call to the `Resolve` method, tracking the
> container on which the call is originally made, whether there is an @Rezolver.IContainerScope active for the call,
> and other things besides.  You will rarely use it directly in application code unless you are extending Rezolver.
>
> These `Resolve` extension methods create the `IResolveContext` for a given operation on your behalf, so you never
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

# Container Configurations and Options

For those looking to customise or extend Rezolver, many of the types are overridable.  However, 
the @Rezolver.ITargetContainer and @Rezolver.IContainer implementations mentioned above also use
another mechanism that provides extensibility without having to subclass them.

> [!NOTE]
> This is an advanced topic and not one that you should have to worry about most of the time.  The examples in
> this guide will highlight where you can use the functionality described below - this section is intended to be 
> a high-level overview only. 

There are two primary types of container configuration in Rezolver:

1. **Target container configuration** (via implementations of <xref:Rezolver.ITargetContainerConfig>)
2. **Container configuration** (via implementations of <xref:Rezolver.IContainerConfig>)

Both are very similar in that they define a method called `Configure` (see 
[ITargetContainerConfig.Configure](xref:Rezolver.ITargetContainerConfig.Configure*) and 
[IContainerConfig.Configure](xref:Rezolver.IContainerConfig.Configure*)) to 
which is passed an @Rezolver.ITargetContainer and, in the case of @Rezolver.IContainerConfig, also an
@Rezolver.IContainer.

Implementations of the interfaces can add/modify service registrations which are then used either directly by the 
container, or which provide more advanced registration functionality.

For example, Rezolver's [automatic enumerable injection](enumerables.md) is enabled by the 
@Rezolver.Configuration.InjectEnumerables configuration when it configures an @Rezolver.ITargetContainer.  This configuration
is actually applied to all instances of @Rezolver.TargetContainer by default (via the @Rezolver.TargetContainer.DefaultConfig
configuration collection) - but you can also control whether enumerable injection is enabled without having to remove the 
configuration from that collection, [as is shown in the last enumerable example](enumerables.md#disabling-enumerable-injection).

There is much more to be covered about configuration and options.  For now - use them where this guide shows you can
(e.g. to [control contravariance](variance/contravariance.md#disabling-contravariance-advanced) or 
[member binding behaviour](member-injection/index.md) etc), and if you want to be able to control
something else this way, and can't, then just open an issue on Github.

* * *

# Next Steps

- Read up on how to [register services in the container](service-registration.md).
- Alternatively, if you haven't already, take a look Rezolver's understanding of [object lifetimes](lifetimes/index.md).