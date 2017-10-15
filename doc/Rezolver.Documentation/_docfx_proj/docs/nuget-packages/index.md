# Rezolver Nuget Packages

Rezolver is primarily distributed as a suite of Nuget packages whose functionality stacks one on top of another.
Depending on the level of functionality you require, there should be a Nuget package which contains only the code you need,
with the main package ([Rezolver](rezolver.md)) being the root dependency for all.

All the core nuget packages support the .NetStandard 1.1 profile (don't know what this is? 
[Learn more](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md)), 
.Net 4.5.1 and .Net 4.6.

> [!NOTE]
> Where a package has a dependency on a third party package (such as Newtonsoft's Json.Net or .Net Core's Microsoft.Extensions.DependencyInjection packages),
> then we try to maintain full support for the same frameworks and profiles that those packages support.

## List of packages

- [Rezolver](rezolver.md)
- [Rezolver.Microsoft.Extensions.DependencyInjection](rezolver.microsoft.extensions.dependencyinjection.md)
- [Rezolver.Microsoft.AspNetCore.Hosting](rezolver.microsoft.aspnetcore.hosting.md)
