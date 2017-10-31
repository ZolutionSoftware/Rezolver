# Rezolver.Microsoft.AspNetCore.Hosting Package

This builds on the [Rezolver.Microsoft.Extensions.DependencyInjection](rezolver.microsoft.extensions.dependencyinjection.md)
package to enable the integration of Rezolver into your Asp.Net Core site somewhat earlier in its startup phase.

[See package page on nuget](https://www.nuget.org/packages/Rezolver.Microsoft.AspNetCore.Hosting).

Rezolver integration is available for both Asp.Net Core 1.1 and 2.0, with the Asp.Net Core 2.0 
integration offering more flexibility as it allows you to perform configuration on the container(s) that
are created before any registrations are made.

# Asp.Net Core 2.0

Version `2.0` of the package supports Asp.Net Core 2.0.

## Program.cs changes

After adding the package, change your `program.cs` file so it looks like this:

[!code-csharp[Example.cs](../../../../../Examples/Rezolver.Examples.AspNetCore.2.0/Program.cs#example)]

So, at the most basic level you just call the 
@Microsoft.AspNetCore.Hosting.RezolverServiceProviderWebHostBuilderExtensions.UseRezolver* method, and that's
enough.

If you've been reading some of the topics on this site - say, about [contravariance](../variance/contravariance.md)
or [lazy and eager enumerables](../enumerables/lazy-vs-eager.md) then you'll have seen how we can use options
to control complex behaviours in Rezolver containers.  That's what is shown in the alternative call
to `UseRezolver` in the commented code.  you can also pass a configuration callback that will be called
to modify the @Rezolver.RezolverOptions that will be used to create both the @Rezolver.IRootTargetContainer that
forms the basis of the registrations, and the @Rezolver.ScopedContainer that will ultimately create the objects
for the application.

> [!NOTE]
> Please note that lazy enumerables are currently switched off in Rezolver's integration with Asp.Net Core
> because of a mis-specification of a test by the Dependency Injection team.
> 
> The [specification test](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Specification.Tests/) is something
> which a DI container should pass if it's intending to be used with Asp.Net Core - and unfortunately there
> was one test that failed when Rezolver was using lazy enumerables - which is its default behaviour.
> [An issue was raised](https://github.com/aspnet/DependencyInjection/issues/589) on the Git repo and has 
> been fixed, but it'll take a few more days/weeks before we see it in the wild.
> 
> We can say that if you *need* lazy enumerables, then you should be able to re-enable them and your application
> will work.

## Startup.cs changes

Now we need to tell the Asp.Net Core stack that you want an @Rezolver.IRootTargetContainer to be created
in order to construct the @Rezolver.ScopedContainer that will be used as the DI container.

The *simplest* way to do this is to add a single method (it can be empty) called `ConfigureContainer`, which 
accepts a single parameter of the type <xref:Rezolver.IRootTargetContainer>.

> [!INFO]
> The @Rezolver.IRootTargetContainer interface was added in Rezolver 1.3.2 to accommodate covariance
> and is now the interface used for 'top-level' target containers.  You can still declare your method
> as `ConfigureContainer<ITargetContainer>` without losing any functionality - so projects which are
> upgrading from v2.0.x of the Hosting package will still work without any code changes.

[!code-csharp[Startup.cs](../../../../../Examples/Rezolver.Examples.AspNetCore.2.0/Startup.cs#example)]

> [!NOTE]
> Note the call to `AddOptions()` in the `ConfigureServices` method.  As the comment in the code says,
> you need this if you intend to use configuration callbacks in your `UseRezolver` method call in `program.cs`.

By adding that `ConfigureContainer` method, a new Rezolver @Rezolver.ScopedContainer will be created and populated
from the `IServiceCollection` that contains all the service registrations for the application.

As the comment states, inside that method you can also perform more configuration or registrations on the 
@Rezolver.IRootTargetContainer, such as decorators, expressions etc which are not supported by Microsoft's DI
abstractions, before the target container is passed to a new @Rezolver.ScopedContainer that will then be
used as the application's `IServiceProvider`.

And that's it - your application is configured.

# Asp.Net Core 1.1

Version `1.3` is the latest version of the package that supports Asp.Net Core 1.1.  The setup is fundamentally
identical, except the `UseRezolver` method does not accept any callbacks.

[!code-csharp[Example.cs](../../../../../Examples/Rezolver.Examples.AspNetCore.1.1/Program.cs#example)]

The `startup.cs` is similar to what's required for Asp.Net Core 2.0 - except your `ConfigureContainer`
method should accept an `ITargetContainer` - i.e. `ConfigureContainer(ITargetContainer)`
and then Rezolver will be used as the container for your application.

***

Clearly, we recommmend moving to Asp.Net Core 2.0 ASAP as it has more features, is faster, and is generally 
*way* cooler than 1.1! :wink:
