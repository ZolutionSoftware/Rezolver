# Rezolver.Microsoft.Extensions.DependencyInjection Package

[See package page on nuget](https://www.nuget.org/packages/Rezolver.Microsoft.Extensions.DependencyInjection).

> [!TIP]
> It's recommended that you use the [Asp.Net Core Hosting integration package](rezolver.microsoft.aspnetcore.hosting.md) 
> on top of this one to enable integration of Rezolver into your Asp.Net core website at an earlier stage of 
> its lifetime.

This package provides Rezolver's implementation of the Microsoft DI Container which drives the new Asp.Net Core stack.

After adding the package, configuring your Asp.Net website to use Rezolver as the DI container is simple:

Replace the default `ConfigureServices` function in your application's `Startup.cs` file with
this one:

[!code-csharp[Startup.cs](../../../../../Examples/Rezolver.Examples.AspnetCore.1.1/Startup.cs#example)]

This returns a new service provider to the Asp.Net Core stack - thus replacing the default service provider
that's already built.

As the comment in the snippet suggests, you will likely want to perform additional registration operations
on the @Rezolver.IContainer object that's returned by the `CreateRezolverContainer` function called here,
since the Rezolver container supports more functionality (e.g. expressions, decorators, custom targets, and more) 
than the MS DI container does through its `ServiceRegistrations` class.