# Creating and using a container

Using Rezolver is the same as with all other IOC containers:

- Create and configure the root container
- Register the services which your application needs
- Resolve services from the container

> [!NOTE]
> Rezolver does also support child containers and lifetime scopes, but we won't cover those here.

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

> [!NOTE]
> All code samples assume you have added a `using` statement (`imports` in VB) for the `Rezolver` namespace.

Once you have a local reference to either of these classes, you can start registering services in the container,
and resolving objects from it.

> [!TIP]
> The core implementation of @Rezolver.IContainer is @Rezolver.ContainerBase, which is shared by both 
> @Rezolver.Container and @Rezolver.ScopedContainer - and it is this class which provides the implementations of
> both @Rezolver.IContainer and @Rezolver.ITargetContainer through which you can both register services and resolve
> them.

## Registering services

With our default implementations of @Rezolver.IContainer, registration of services ultimately requires adding 
'targets' to an @Rezolver.ITargetContainer (the container's <xref:Rezolver.ContainerBase.Targets> property) and 
associating them with a service type which we will later resolve.

> [!NOTE]
> The @Rezolver.ContainerBase class implements the @Rezolver.ITargetContainer interface by forwarding all the
> calls it receives through that interface to its @Rezolver.ContainerBase.Targets property.

The core registration method for this is the @Rezolver.ITargetContainer.Register*
method, which accepts an @Rezolver.ITarget and an optional type against which the registration is to be made.

> [!TIP]
> A ***target*** (<xref:Rezolver.ITarget>) is an object stored within an @Rezolver.ITargetContainer which contains 
> information about how an object is to be created or retrieved when a container's @Rezolver.IContainer.Resolve* 
> operation is called.

Rezolver, like most IOC containers, also provides additional registration extension methods which abstract away 
the @Rezolver.ITarget which will ultimately be created and stored.

The majority of the code samples here use these registration methods, which are documented in the API section
of this site:

- @Rezolver.AliasTargetContainerExtensions
- @Rezolver.DecoratorTargetContainerExtensions
- @Rezolver.DelegateTargetContainerExtensions
- @Rezolver.ExpressionTargetContainerExtensions
- @Rezolver.MultipleTargetContainerExtensions
- @Rezolver.ObjectTargetContainerExtensions
- @Rezolver.RegisterTypeTargetContainerExtensions
- @Rezolver.ScopedTargetContainerExtensions
- @Rezolver.SingletonTargetContainerExtensions

> [!NOTE]
> All the registration extension methods require an instance of @Rezolver.ITargetContainer or
> @Rezolver.ITargetContainerOwner

So, then - if we wanted to tell the container that we want the type `MyService` to be available - we could use 
one of the many @Rezolver.RegisterTypeTargetContainerExtensions.RegisterType* methods, such as the generic
@Rezolver.RegisterTypeTargetContainerExtensions.RegisterType``1(Rezolver.ITargetContainer,Rezolver.IMemberBindingBehaviour)
method:

```cs
container.RegisterType<MyService>();
```

This tells the container to construct an instance of `MyService` when we resolve that type.

Alternatively, we could register the type `MyService` against one of its bases or interfaces:

```cs
//assume MyService inherits from MyServiceBase
container.RegisterType<MyService, MyServiceBase>();

//assume MyService implements IMyService
container.RegisterType<MyService, IMyService>();
```

So, here, the container will construct an instance of `MyService` when either `MyServiceBase` or
`IMyService` are requested.

All of our generic extension methods like those shown above will also have non-generic variants.  Typically this is
just good manners, but it's also more essential with IOC containers because you sometimes need to register against
types which cannot expressed as generic type arguments, for example:

```cs
container.RegisterType(
  typeof(IMyGeneric).MakeGenericType(
    typeof(IEnumerable<>)
  )
);
// because it's impossible to do:
// container.RegisterType<IMyGeneric<IEnumerable<>>();
```

> [!NOTE]
> This overview has only covered setting up a registration for a constructor-injected type, there are 
> many other types of registration that can be created - in different ways - and constructor injection
> itself has multiple alternative behaviours associated with it, too.  These will be introduced as we
> progress further through the documentation.

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

> [!NOTE]
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

## Examples

### Register and resolve by same type

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/SimpleExamples.cs#MyServiceDirect)]

