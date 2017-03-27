# Rezolver Dev Guide

Welcome to the Rezolver developer guide!

This site is always under development, and right now we're doing everything we can to get all high-level
documentation in place so that even novice users of DI containers can get up and running with Rezolver.

Something missing?  Please open an issue [over on Github](http://github.com/zolutionsoftware/rezolver)
with your question.

# Getting started

As with many open source .Net projects, there are two primary ways to get Rezolver integrated into your project.

- The easiest is via the [Nuget packages](nuget-packages/index.md)
  - By using the Nuget packages, you ensure that you're using the latest (hopefully stable!) releases.  Pre-release packages will also
be made available for in-development features.
- Fork or download the [Rezolver source from Github](https://github.com/ZolutionSoftware/Rezolver)
  - Use this if you can't integrate nuget into your build pipeline, or if you'd like to customise or contribute to the project.
  - If you're looking to customise, though, then there _should_ be enough extensibility points in the framework to allow you to do so 
without having to change core types.  If that's not the case, post an issue on the Github project so we can get it added in and
make it better for everyone!

* * *

# Tests as Examples

Most of the example code shown here is drawn directly from the project `Rezolver.Tests.Examples`, which can be found
in the `test` folder under the root of the repo.  If there's a filename shown at the top of a code example, then that
*should* equal the filename where you can find that code in the examples project.

Please note that any example code containing type declarations will be found in the `Types` folder of that project, with all the 
individual test files in the root.

We're using [xunit](https://xunit.github.io/) for all our tests, hence all the examples are written for it too.

## Matching an example to a test

We do omit the test function declaration in these examples - but if you look down a tests file you'll see comments inside
each test method with an XML tag.  Here's the body of the `ObjectExamples.cs` file, which contains all the examples
for the ['Objects as Services' documentation](objects.md):

[!code-csharp[ObjectExamples.cs](../../../../test/Rezolver.Tests.Examples/ObjectExamples.cs)]

You'll notice that the body of each test method looks like this:

```cs
//<example_n>

... (code) ...

//</example_n>
```

> [!NOTE]
> This is a facet of the code snippet feature of [docfx](https://dotnet.github.io/docfx/)

Each example is numbered sequentially, so it shouldn't be too hard to marry up the example in the documentation to the 
test in the tests file.

# Next Steps

- Asp.Net Core developers should read how to 
[integrate Rezolver into the Asp.Net Core hosting pipeline](nuget-packages/rezolver.microsoft.aspnetcore.hosting.md).
- Learn how to [create and use a container](create-and-use-a-container.md).