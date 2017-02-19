# Getting started

As with many open source .Net projects, there are two primary ways to get Rezolver integrated into your project.

- The easiest is via the [Nuget packages](../nuget-packages/index.md)
  - By using the Nuget packages, you ensure that you're using the latest (hopefully stable!) releases.  Pre-release packages will also
be made available for in-development features.
- Fork or download the source from [Github](https://github.com/ZolutionSoftware/Rezolver)
  - Use this if you can't integrate nuget into your build pipeline, or if you'd like to customise or contribute to the project.
  - If you're looking to customise, though, then there _should_ be enough extensibility points in the framework to allow you to do so 
without having to change core types.  If that's not the case, post an issue on the Github project so we can get it added in and
make it better for everyone!

> [!TIP]
> If you are developing an Asp.Net Core website, then you'll also want to read how to 
> [integrate Rezolver into the Asp.Net Core hosting pipeline](../nuget-packages/rezolver.microsoft.aspnetcore.hosting.md).

* * *

Once you have the main assembly referenced (Rezolver.dll) - you're ready to move on to the next step - which is to 
[create and use a container](create-and-use-a-container.md).