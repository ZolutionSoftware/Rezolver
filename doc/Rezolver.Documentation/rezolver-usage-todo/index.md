# 1. Getting started

As with many open source .Net projects, there are two primary ways to get Rezolver integrated into your project.

- The easiest is via the [Nuget packages](nuget-packages/)
  - By using the Nuget packages, you ensure that you're using the latest (hopefully stable!) releases.  Alpha packages will also
be made available for in-development features.
- Fork or download the source from [Github](https://github.com/LordZoltan/Rezolver)
  - Use this if you can't integrate nuget into your build pipeline, or if you'd like to customise or contribute to the project.
    - If you're looking to customise, though, then there _should_ be enough extensibility points in the framework to allow you to do so 
without having to change core types.  If that's not the case, post an issue on the Github project so we can get it added in and
make it better for everyone!

<hr class="soft" />

Once you've got the main assembly referenced (Rezolver.dll) - then you can create a @Rezolver.Container, register targets against its
internal @Rezolver.ITargetContainer target container, and then grab objects from it!

[!code-csharp[Example.cs](../../../test/Rezolver.Tests/TargetContainerTests.cs#example1)]

More code

[!code-csharp[Full-File.cs](../../../test/Rezolver.Tests/ConstructorTargetTests.cs)]