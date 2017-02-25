# Constructor Injection

The most basic, fundamental feature of any IOC container is the ability to create instances of services through a 
constructor, automatically injecting arguments into that constructor from the services which have been registered in
the container.

The default behaviour for our constructor injection is to find the constructor you want bound when the container has
to materialise an instance for you - you tell Rezolver the type you want created, and it'll do the rest.

> [!TIP]
> Constructor injection is achieved in Rezolver through the targets @Rezolver.Targets.ConstructorTarget and
> @Rezolver.Targets.GenericConstructorTarget (for open generic types).  These classes can be built by hand in your code for
> registration in the container, or you can use the extensions defined in 
> @Rezolver.RegisterTypeTargetContainerExtensions, which provides shortcuts for the most common constructor injection
> scenarios.
> 
> Finally, there's also @Rezolver.SingletonTargetContainerExtensions and @Rezolver.ScopedTargetContainerExtensions,
> which provide shortcuts for singleton and explicitly scoped constructor-injected registrations.
>
> In the examples shown throughout, we use both the extension methods and target constructors.

So, given these types:

[!code-csharp[MyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/MyService.cs#example)]

and

[!code-csharp[RequiresMyService.cs](../../../../../test/Rezolver.Tests.Examples/Types/RequiresMyService.cs#example)]

> [!NOTE]
> The rather silly explicit implementation and argument checking in the second constructor is purely for 
> illustrative purposes!

In order to build `RequiresMyService` we need an instance of `MyService` or `IMyService`, so let's try injecting a
`MyService`, and resolve:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example1)]

*Notice that the container happily binds to one of these two constructors*

Now, obviously a key facet of dependency injection is that we can depend upon an *abstraction* of a service instead of a concrete
implementation - so most of the time your constructors will request an interface instead of a concrete type.  To do this,
we simply register `MyService` against the type `IMyService`:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example2)]

'So what?' you say, 'it's doing exactly what I want it to!'  Yes, but there's more going on here than you'd think: the 
@Rezolver.Targets.ConstructorTarget is selecting the best-matched constructor based on the service registrations present 
in the container when it's asked to @Rezolver.IContainer.Resolve* the object.

To prove that it's the `IMyService` constructor we bound there, and not the other one - let's try registering a different
implementation of `IMyService` - one which the class will not support because it'll fail the argument type-check in the
second constructor:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example3)]

> [!NOTE]
> You can see these tests, and run them yourself, if you grab the 
> [Rezolver source from Github](https://github.com/zolutionsoftware/rezolver), open the main solution and run
> the tests in the 'Rezolver.Tests.Examples' project.

So, then, let's take a deeper dive into how Rezolver binds constructors.

# Types of constructor binding

Some IOC containers restrict you to types which have a single constructor.  Rezolver's constructor injection, implemented by
the types @Rezolver.Targets.ConstructorTarget and @Rezolver.Targets.GenericConstructorTarget, are capable of binding to types
which have multiple constructors.

The `ConstructorTarget` actually has two modes:

- Find the best matching constructor
- Explicitly-supplied constructor (where you supply a `ConstructorInfo` on creation)

## Best match constructor

The best-match algorithm is similar to how a compiler matches a method overload.  The rules aren't necessarily exactly 
the same as, say, the C# spec, but they're close.

Based on the services available in the container at the time the @Rezolver.IContainer.Resolve* call is made, the algorithm
will first attempt to select the greediest constructor whose parameters are *completely*
satisfied by those registered services.  Thus, if a class has a constructor with one parameter and one with ten; if the single
parameter can be successfully injected but only nine of the others can be, then the single parameter constructor wins:



> [!NOTE]
> Parameters with default values are treated as being 'satisfied' even if the service is not registered, so if our ten-parameter
> constructor overload had nine optional parameters with defaults, and the same first parameter, then it would win.
> 
> If there's a tie on the number of satisfied arguments between two constructors which have one or more optional parameters 
> with default values, then the one which will require the use of the fewest default values wins.

If, however, none of the constructors can be completely satisfied, then we look for the greediest constructor with the most
number of resolved arguments.  If there's a clear winner, then we proceed with that constructor anyway even though one or
more required services are missing.  This is to allow child containers (documentation pending) to plug the gap for these
other services.

### Named argument binding

When using best-match, you can also supply a dictionary of named argument bindings (as a dictionary of <xref:Rezolver.ITarget>s)
which can be used to provide a hint as to your preferred constructor.  You don't need to provide all named arguments, just those
for which 

## Simple scenarios



