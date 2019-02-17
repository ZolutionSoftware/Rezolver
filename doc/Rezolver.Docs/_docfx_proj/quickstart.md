# Quickstart

New to Rezolver?  New to IOC?  Here's how to do some of the basics.

Before we start, you need to get a reference to the Rezolver library referenced, to do that you have two options:

1. Add a reference to one of the [nuget packages](docs/nuget-packages/index.md)
2. Pull the [code from Github](https://github.com/ZolutionSoftware/Rezolver) and build it locally

Now, after adding a `using` for the `Rezolver` namespace and you're ready to go.

# Basics

In general, you won't be using an IOC container directly to 'resolve' types.  

## Registering and resolving

[!code-csharp[QuickstartExamples.cs](../../../test/Rezolver.Tests.Examples/QuickStartExamples.cs#example1)]

