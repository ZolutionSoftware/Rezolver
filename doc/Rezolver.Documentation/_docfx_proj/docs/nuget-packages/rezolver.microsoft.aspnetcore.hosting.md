# Rezolver.Microsoft.AspNetCore.Hosting Package

[See package page on nuget](https://www.nuget.org/packages/Rezolver.Microsoft.AspNetCore.Hosting).

This builds on the [Rezolver.Microsoft.Extensions.DependencyInjection](rezolver.microsoft.extensions.dependencyinjection.md)
package to enable the integration of Rezolver into your Asp.Net Core site somewhwat earlier in its startup phase.

After adding the package, open your site's `program.cs` and add a call to the `UseRezolver()` extension
method that's now available, as shown:

[!code-csharp[Example.cs](../../../../../Examples/Rezolver.Examples.AspNetCore.1.1/Program.cs#example)]

Then we need to tell the Asp.Net Core stack that you want an @Rezolver.ITargetContainer to be created
in order to construct the @Rezolver.IContainer that will be used as the DI container.

We do this in your application's `startup.cs` - simply by declaring a single method:

[!code-csharp[Example.cs](../../../../../Examples/Rezolver.Examples.AspNetCore.1.1/Startup.cs#example)]

And that's it - your application is configured.
