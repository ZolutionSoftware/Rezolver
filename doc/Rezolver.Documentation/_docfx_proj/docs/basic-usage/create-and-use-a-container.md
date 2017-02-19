The simplest way to create an @Rezolver.IContainer, registering targets in it and resolving objects from it 
couldn't be easier.

> [!NOTE]
> In Rezolver, registering and resolving services are operations which are handled by two separate interfaces 
> (<xref:Rezolver.ITargetContainer> and <xref:Rezolver.IContainer>).  The two most-used standard container classes (<xref:Rezolver.Container> and
> <xref:Rezolver.ScopedContainer>) implement both of these interfaces, however, in order to provide the
> most convenient developer experience.
> 
> So, in order for your application to be able to both register targets in, *and* resolve objects from, a
> container, you will need a reference to one of those two container classes.

> [!NOTE]
> A ***target*** (<xref:Rezolver.ITarget>) is an object stored by an @Rezolver.ITargetContainer which contains 
> information about how an object is to be created or retrieved when a container's @Rezolver.IContainer.Resolve* 
> operation is called.

> ![NOTE]
> The core implementation of @Rezolver.IContainer is @Rezolver.ContainerBase, which is shared by both 
> @Rezolver.Container and @Rezolver.ScopedContainer - and it is this class which provides the implementations of
> both @Rezolver.IContainer and @Rezolver.ITargetContainer.

For the majority of these code samples, we'll be using the many extension methods for both registration and 
resolving objects which are provided by numerous extension classes:

- Registration:
  - @Rezolver.AliasTargetContainerExtensions
  - @Rezolver.DecoratorTargetContainerExtensions
  - @Rezolver.DelegateTargetContainerExtensions
  - @Rezolver.ExpressionTargetContainerExtensions
  - @Rezolver.MultipleTargetContainerExtensions
  - @Rezolver.ObjectTargetContainerExtensions
  - @Rezolver.RegisterTypeTargetContainerExtensions
  - @Rezolver.ScopedTargetContainerExtensions
  - @Reaolver.SingletonTargetContainerExtensions
- Resolving:
  - @Rezolver.ContainerResolveExtensions

## Simple registration

The simplest thing we can do is to register a type for which we want an instance to be created
when request that type.

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/SimpleExamples.cs#MyServiceDirect)]

As you can see - we create a @Rezolver.Container, register a single type (as itself)