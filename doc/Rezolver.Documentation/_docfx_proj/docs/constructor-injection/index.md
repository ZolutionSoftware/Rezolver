# Constructor Injection

The most basic, fundamental feature of any IOC container is the ability to create instances of services through a 
constructor, automatically injecting arguments into that constructor from the services which have been registered in
the container.

> [!TIP]
> Constructor injection is achieved in Rezolver through the targets @Rezolver.Targets.ConstructorTarget and
> @Rezolver.Targets.GenericConstructorTarget (for open generic types).  The examples here show how to create and register
> these directly and via some of the extension methods in @Rezolver.RegisterTypeTargetContainerExtensions, 
> @Rezolver.SingletonTargetContainerExtensions and @Rezolver.ScopedTargetContainerExtensions.
> 
> You can see the tests from which these examples are taken, and run them yourself, if you grab the 
> [Rezolver source from Github](https://github.com/zolutionsoftware/rezolver), open the main solution and run
> the tests in the 'Rezolver.Tests.Examples' project.

Some IOC containers restrict you to types with a single constructor.  In some cases this is because of the design of the container
itself - i.e. in order to achieve good performance - and in others it's to encourage good program design.  

Rezolver's constructor injection, implemented by the types @Rezolver.Targets.ConstructorTarget and @Rezolver.Targets.GenericConstructorTarget, 
supports binding to types which have multiple constructors.

The `ConstructorTarget` actually has two modes:

- Find the best matching constructor
- Explicitly-supplied constructor (where you supply a `ConstructorInfo` on creation)

* * *

## Example - Injected class

So, given these types:

[!code-csharp[MyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/MyService.cs#example)]

and

[!code-csharp[RequiresMyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/RequiresMyService.cs#example)]

> [!NOTE]
> The rather silly explicit implementation and argument checking in the second constructor is purely for 
> illustrative purposes!

In order to build `RequiresMyService` we need an instance of `MyService` or `IMyService`, so let's try injecting a
`MyService`, and resolve:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example1)]

* * *

## Example - Injected interface

Now, obviously a key facet of dependency injection is that we can depend upon an *abstraction* of a service instead of a concrete
implementation - so most of the time your constructors will request an interface instead of a concrete type.  To do this,
we simply register `MyService` against the type `IMyService`:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example2)]

'So what?' you say, 'it's doing exactly what I want it to!'  Yes, but there's more going on here than you'd think: the 
@Rezolver.Targets.ConstructorTarget is selecting the best-matched constructor based on the service registrations present 
in the container when it's asked to @Rezolver.IContainer.Resolve* the object.

* * *

## Best-match examples

Let's take a bit of deep dive into how Rezolver determines a 'best-match' for the constructor to be called by the container.

### Example - Best-match (proof)

First, to prove that it's the `IMyService` constructor we bound in the previous example, and not the other one - let's try registering 
a different implementation of `IMyService` - one which the class will not support because it'll fail that silly argument type-check 
in the second constructor of `RequiresMyService`:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example3)]

* * *

### Example - Best-match (fewest params)

The best-match algorithm is similar to how a compiler matches a method overload when writing code by hand.  The rules aren't 
necessarily exactly the same as, say, the C# spec, but they're close.

At the time the first @Rezolver.IContainer.Resolve* call is made, the algorithm will first attempt to select the greediest constructor 
whose parameters are *completely* satisfied by services available to the container.  Thus, if a class has two constructors - one with 
1 parameter and one with 3 parameters - if the single parameter version can be successfully injected, but only 2 of the other 
constructor's parameters can be, then the single-parameter constructor wins.

So, given this type:

[!code-csharp[RequiresMyServices.cs](../../../../../test/Rezolver.Tests.Examples/Types/RequiresMyServices.cs#example)]

If we register `Service1` & `Service2`, but not `Service3`, the single-parameter constructor will be used:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example4)]

* * *

### Example - Best-match (with defaults)

When a constructor's parameters have default values, the rules change slightly.

The algorithm treats parameters which have defaults as *always* satisfied, even if a service is not registered of the correct type.  So 
if we extend `RequiresMyServices` with a new class whose 3-parameter constructor specifies default values for parameters 2 & 3:

[!code-csharp[RequiresMyServicesWithDefaults.cs](../../../../../test/Rezolver.Tests.Examples/Types/RequiresMyServicesWithDefaults.cs#example)]

And then we swap `RequiresMyServices` for `RequiresMyServicesWithDefaults`, this time, the 3-parameter
constructor will be executed, with parameters 1 & 2 receiving injected arguments, and parameter 3 receiving the default instance from
the base:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example5)]

> [!TIP]
> The use of @Rezolver.Targets.ChangeTypeTarget (via the `As` extension method) there might be a little confusing.  In truth, we're only sneaking it in here to show
> some other parts of the framework :)
> 
> Think of it as being the same as an explicit cast from one type to another - you put a target inside it and tell it what type
> you want it to be.
> 
> In the example, we have to use this if we're determined to the use the @Rezolver.MultipleTargetContainerExtensions.RegisterAll* method(s),
> because they don't allow us to override the registration type for the targets that we're registering.

* * *

### Example - Best *partial* match 

If, however, none of the constructors can be completely satisfied, then we look for the greediest constructor with the most
number of successfully resolved arguments.  If there's a clear winner, then we proceed with that constructor anyway even though one or
more required services are missing.

> [!NOTE]
> You might wonder why we would allow binding even though we can't actually satisfy all the parameters of the constructor, well we'll
> get to that in a moment.

This time, we'll have a type which needs one or two services - either `MyService1` or ***both*** `MyService2` *and* `MyService3`:

[!code-csharp[Requires2MyServices.cs](../../../../../test/Rezolver.Tests.Examples/Types/Requires2MyServices.cs#example)]

To force the selection of the second constructor for the test, we'll only register `MyService2` in the container and, when we attempt to
resolve the instance, we should get an `InvalidOperationException` explaining that `MyService3` couldn't be resolved: 

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example6)]

* * *

## Why we allow partial matches

As mentioned in the intro to the last example - it's probably not obvious why we would want to allow binding to a constructor in a container
which can't actually fulfil that constructor's requirements!

Well, it's clearly not *normal*, but it's a valid use-case when you consider that Rezolver supports a concept that is typically referred
to as 'child containers', except Rezolver calls them 'overriding containers'.

A more complete discussion of this functionality will be added to the documentation soon, but in the meantime, let's see two ways in which 
we could 'fix' the container in the previous test so that it successfully builds an instance of `Requires2MyServices`.

### Example - @Rezolver.OverridingContainer

This is the solution which most closely matches the child container functionality provided by other IOC libraries - take a container 
which is already 'established' and override it with another container that has its own registrations.  The two will work together, sharing 
registrations when creating instances, so long as the overriding container's @Rezolver.IContainer.Resolve* implementation is used:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example6_1)]

> [!NOTE]
> If we were to try to resolve from `container` after creating `containerOverride`, it would still fail, because it doesn't know anything
> about the overriding container.

Hopefully this should all seem pretty logical.  If so, and you're happy simply to accept it, you can skip the next bit.

#### How it works

If you're interested in the inner workings when using overriding containers, the process is this:

- The `overridingContainer` receives the @Rezolver.IContainer.Resolve* call for `Requires2MyServices`
- It looks inside its @Rezolver.ContainerBase.Targets target container for a registration for `Requires2MyServices` and doesn't find one
- So the call is passed to the overidden `container`...
  - ... which finds its registration and binds to the 2-parameter constructor of `Requires2MyServices` as before
  - The bound constructor is executed
    - The `MyService2` parameter is fulfiled by the `container`'s own registration
    - But, for the `MyService3` parameter, `container` sees that it was not the container whose @Rezolver.IContainer.Resolve* method
was *originally* called (i.e. `overridingContainer`), so it forwards the call for `MyService3` to `overridingContainer`.
- `overridingContainer` receives the @Rezolver.IContainer.Resolve* call for `MyService3`
  - It finds its registration, and executes it to get the instance, passing it back to `container` - thus completing the constructor call 
for `Requires2MyServices`

* * *

### Example - @Rezolver.OverridingTargetContainer

This solution is similar to the previous one, except this time we're not overriding the container, but overriding the *target container* used 
by a container.

> [!NOTE]
> The choice of name 'OverridingTargetContainer' is deliberate, despite the overriding behaviour shown here, because it is more a parent-child
> relationship than in the previous example.

In order to do this, however, we have to change how we create our container.  Until now, we've simply been creating a new @Rezolver.Container
instance and registering targets via its own implementation of @Rezolver.ITargetContainer (which, as mentioned elsewhere, wraps its
@Rezolver.ContainerBase.Targets property).

This time, we're going to create a @Rezolver.TargetContainer directly, register `MyService2` and `Requires2MyServices` in it, then 
create a @Rezolver.OverridingTargetContainer on top of that with the other registration of `MyService3`.  This target container is then 
passed to the new container on construction.

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example6_2)]

#### Why do this?

This might seem a little confusing - why would you split target containers like this?

Firstly - it's quite an advanced concept, and truly understanding the difference between this and the last example requires a lot
of understanding about the internals of Rezolver.  Therefore it's more suited to developers who are extending the framework, rather
than the average developer who's simply looking to use it to drive their application.

The framework itself takes advantage of this functionality throughout in order to override behaviours, or to 
extend a user-configured container with additional transient targets.  The @Rezolver.Targets.DecoratorTarget wouldn't work without this
functionality, for example.

Also, if you delve into the compiler pipeline, you will be using this functionality all the time.

Providing full examples of how you'd leverage this functionality is outside the scope of this topic, but we'll add them to the guide
as soon as we've got the rest of the guide complete.

* * *

## Best-match (named args)

When using best-match, you can also supply a dictionary of named argument bindings (as a dictionary of <xref:Rezolver.ITarget>s)
which can be used to provide a hint as to your preferred constructor and, more crucially, override the default behaviour of resolving *every* argument
from container.

You don't need to provide all named arguments, the binder will use as many as it can and auto-bind the rest.

### Example - Supplying a `DateTime`

To demonstrate this, we'll have a new type which requires an `IMyService` and also accepts a `DateTime`.

`DateTime`s and other primitive
types (`string`s etc) are typically not great for use in IOC containers, because you can only have one of them registered (or one collection)
unless you start using overriding containers or child target containers, which means you typically can only allow one type to have them as
dependencies.

[!code-csharp[RequiresIMyServiceAndDateTime.cs](../../../../../test/Rezolver.Tests.Examples/Types/RequiresIMyServiceAndDateTime.cs#example)]

Note that the single parameter constructor defaults the `StartDate` to `DateTime.UtcNow`, but in our test, we'll explicitly provide a 
`DateTime` which is `DateTime.UtcNow.AddDays(1)` to create a date in the future:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example7)]

What happens is that the named arguments contribute to the argument binding process - thus allowing us to 'cheat' and promote a constructor to being
a better match than the one that would normally be.

> [!WARNING]
> Obviously - named argument binding is potentially very brittle - as if the parameter name changes, then the binding will no longer work.
>
> In the future, we will also add the ability to supply an @Rezolver.ITargetContainer to a @Rezolver.Targets.ConstructorTarget whose registrations
> will be used in preference to the main container - thus allowing us simply to register a `DateTime` in this example, removing the dependency on
> the parameter name.

When you supply @Rezolver.ITarget instances up-front to another target in this way, you can use any of the targets in the @Rezolver.Targets
namespace to supply a value, and they will work as if they were registered in the container.

* * *

## With a `ConstructorInfo`

Instead of relying on the best match algorithm, you can also specify the constructor you want bound up-front, *and* you can supply parameter
bindings too.  To illustrate, we'll have a type with a default constructor and one which accepts a service:

[!code-csharp[AcceptsOptionalIMyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/AcceptsOptionalIMyService.cs#example)]

* * *

### Example - No parameter bindings

The first test ignores the registered services and forcibly targets the default constructor:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example100)]

* * *

### Example - Pre-bound parameters

We can also explicitly bind the parameters of a particular constructor by providing an array of @Rezolver.ParameterBinding objects along with 
a `ConstructorInfo`.  

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example101)]

As you can see - using explicitly bound parameters is a little verbose, given the need to find the constructor and then setup those
@Rezolver.ParameterBinding objects with the correct `ParameterInfo`s; but it's guaranteed to target the constructor you choose.

* * *

# Next steps

- See how [constructor injection works with generic types](generics.md)
- Learn how to [enable member injection](member-injection.md) (injecting services into properties and/or fields)