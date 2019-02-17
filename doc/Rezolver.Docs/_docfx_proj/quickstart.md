# Quickstart

New to Rezolver?  New to IOC/DI ('Inversion of Control', or Dependency Injection)?  Here's how to do some of the basics.

Before we start, you need to get a reference to the Rezolver library added to your project, to do that you have two options:

1. Add a reference to one of the [nuget packages](docs/nuget-packages/index.md)
2. Pull the [code from Github](https://github.com/ZolutionSoftware/Rezolver) and build it locally

Now, after adding a `using` for the `Rezolver` namespace, you're ready to go.

# Basics

In a [world of injected dependencies](https://en.wikipedia.org/wiki/Dependency_injection), your code will
typically never use the `new` operator to create instances of anything other than perhaps 
[DTOs](https://en.wikipedia.org/wiki/Data_transfer_object).  

For example, a method which updates a record in the database will receive not only the new values for that 
record, but also an instance of an object that will provide access to the database.  Typically, this database 
accessor will likely be provided as an instance of an interface, or perhaps abstract class:

```cs
public Task UpdateWidget(Widget newValues, IRepository<Widget> widgetRepo)
{
  // find widget, update and save
}
```

This type of architecture is very typical as it decouples the code that wants to update the database from the
code that knows how to access the database.  The challenge now becomes what type of object should be created when we 
need an `IRepository<Widget>`?  This is where an IOC/DI  **container** - such as Rezolver - comes in.

## 'Services'

In the DI world, our `IRepository<Widget>` is known as a *service*.  This is not to be confused with the services that
you might be writing as part of a Service Layer Architecture, or a web service.  A service in this case is just some object 
that provides services to other code - it could be accessing a database, as in the case of our repository, or it could be 
formatting numbers, providing dates/times, sending an email or almost anything else.

Once all of our methods/objects have to be provided their service dependencies as arguments, it now means we either have to 
create an instance of a type that satisfies that dependency, or pass one that we've been given by something else.

And that's where the IOC/DI container comes in - it takes care of matching an object's service dependencies to service
*implementations* which can satisfy those dependencies.

To do this, an application needs to register one or more services in the container so that, when it's asked for a service
of a given type it knows *what* to build and *how* to build it - so that's where our quickstart kicks off.

> [!NOTE]
> All of the quickstart examples shown here will work with the default @Rezolver.Container, @Rezolver.ScopedContainer and 
> @Rezolver.TargetContainer types with their default configurations.

---

# Registering and Resolving Services

## Concrete types

Initially we're just using this type:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#mytype)]

Here we'll create a container, register the type `MyType` and then get the container to create an instance of it.

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example1)]

There are a few other ways we can register `MyType` too:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example2)]

There's also other ways to resolve an instance:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example3)]

When we're registering a type like this, we're telling the container to give us an instance of `MyType` by
[Constructor Injection](docs/constructor-injection/index.md), as seen here - when we want to create an instance
of a type which doesn't have a default constructor:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#requiresmytype)]

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example4)]

## Decoupled services + implementations

Obviously, most of the time, your code will be depending upon *abstractions* instead of concrete types, for example:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#iabstraction)]

So here, when creating an instance of `RequiresAbstraction`, we also require an instance of `IAbstraction`.  This
requires registering `Implementation` against the service type `IAbstraction` so that the container knows that every
time an instance of `IAbstraction` is required, an instance of `Implementation` should be created:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example10)]

> [!TIP]
> Most of the container registration methods have overloads which allow you to specify the 'service type' 
> against which the registration is made.  This type must be compatible with the type of whatever will be produced by
> that registration.
> 
> Generally speaking - the container will only match a registration against a requested type if that registration
> was made against that **exact** type.  The exception to this is when registrations are matched
> [covariantly or contravariantly](docs/variance/index.md) for generics.

# Integration with the MS DI Container

Many applications written today will be built around Asp.Net Core or the .Net Core Generic Host, and therefore will
be creating registrations in the @Microsoft.Extensions.DependencyInjection.ServiceCollection.  The
[Rezolver.Microsoft.Extensions.DependencyInjection package](docs/nuget-packages/rezolver.microsoft.extensions.dependencyinjection.md)
provides Rezolver's integration with this abstraction, and can be used to populate a Rezolver container that's already
been created:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example20)]

> [!TIP]
> The @Rezolver.MSDIITargetContainerExtensions.Populate* method is available from the `Rezolver` namespace as
> soon as you reference the package.

You can also create a container directly from a `ServiceCollection` using the 
@Microsoft.Extensions.DependencyInjection.RezolverServiceCollectionExtensions.CreateRezolverContainer* extension method
(in the `Microsoft.Extensions.DependencyInjection` namespace), shown here being used to create a container with
the same registrations as the previous example:

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example21)]

The [Asp.Net Core integration](docs/nuget-packages/rezolver.microsoft.aspnetcore.hosting.md) and 
[Generic Host integration](docs/nuget-packages/rezolver.microsoft.extensions.hosting.md) packages provide even deeper
integration with the .Net Core DI stack, and should be used when you want to drive your application with Rezolver's
enhanced registrations.

# Further reading

This was just a brief flyover of Rezolver's functionality - and it is capable of so much more.

Refer to the [list of features on the homepage](index.md#features) to see all the different things that you can do with
Rezolver, and from there you'll find links to extensive walkthroughs on how to take advantage of them.