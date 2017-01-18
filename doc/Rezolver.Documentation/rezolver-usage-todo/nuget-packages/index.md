# Rezolver Nuget Packages

Rezolver is primarily distributed as a suite of Nuget packages whose functionality stacks one on top of another.
Depending on the level of functionality you require, there should be a Nuget package which contains only the code you need.

All the core nuget packages support the new .NetStandard 1.3 profile (don't know what this is? [Learn more](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md)), .Net 4.5.1 and .Net 4.6.

> Where a package has a dependency on a third party package (such as Newtonsoft's Json.Net or .Net Core's Microsoft.Extensions.DependencyInjection packages),
> then we try to maintain full support for the same frameworks and profiles that those packages support.

## List of packages

- [Rezolver](rezolver.md).
- [Rezolver.Configuration](rezolver.configuration.md)
