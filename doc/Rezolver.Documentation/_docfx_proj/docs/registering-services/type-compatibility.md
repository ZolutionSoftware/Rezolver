# Type Compatibility Checks When Registering

The only way to register a target for our default containers is ultimately to call the
@Rezolver.ITargetContainer.Register* method or, (less commonly) 
@Rezolver.ITargetContainerOwner.RegisterContainer* method.

It's the target container's job to ensure that, when a target is registered, it is actually
capable of building an instance of the type against which the registration is being made.

Luckily, the @Rezolver.ITarget interface supplies the @Rezolver.ITarget.SupportsType* method,
which enables the target container to query the target to ensure that it can support the type
you provide when registering.

Via the @Rezolver.ITarget.DeclaredType property, also, the
target container can fall back to the target's static type should you not explicitly provide
a type for registration.  So, if a @Rezolver.Targets.ConstructorTarget is bound to the type
`MyService`, then its @Rezolver.ITarget.DeclaredType property will return 
`typeof(MyService)`.

## TL;DR

In short, the type against which you can register a target depends on the 
@Rezolver.ITarget.DeclaredType of the target you're registering, but ultimately on its
implementation of @Rezolver.ITarget.SupportsType*, too.

Here's just a few example types and the types against which we could register them:

ITarget.DeclaredType | Can be registered as | With Targets
--- | --- | ---
`int` | `object` | Any
`int` | `int?` | Any
`int` | `IFormattable` | Any
`string` | `IEnumerable<char>` | Any
`MyService : IService` | `IService` | Any
`MyService<T>` | `MyService<T>` | @Rezolver.Targets.GenericConstructorTarget
`MyService<T>` | `MyService<string>` | @Rezolver.Targets.GenericConstructorTarget
`MyService<T> : IService<OtherService, T>` | `IService<OtherService, int>` | @Rezolver.Targets.GenericConstructorTarget

## Registration vs Resolving


