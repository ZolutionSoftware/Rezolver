# Rezolver.Microsoft.Extensions.Hosting Package

[See package page on nuget](https://www.nuget.org/packages/Rezolver.Microsoft.Extensions.Hosting).

Rezolver also offers integration with the .Net Generic Host.  Setup and configuration is very similar to 
when [integrating Rezolver with Asp.Net Core](rezolver.microsoft.aspnetcore.hosting.md) except whereas Asp.Net Core
supports a convention-based approach to configuring the container, the generic host does not.

Here is a complete example which uses Rezolver's decorator functionality to add logging on startup and shutdown of
all @Microsoft.Extensions.Hosting.IHostedService services:

[!code-csharp[Program.cs](../../../../../Examples/Rezolver.Examples.GenericHost/program.cs)]