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


Here's just a few example types and the types against which we could register them:

Type | Can be registered as
--- | ---
`int` | `object`
`int` | `IFormattable`
`string` | `IEnumerable<char>`
`MyService : IService` | `IService`
`MyService<T>` | `MyService<T>`
`MyService<T>` | `MyService<string>`
`MyService<T> : IService<OtherService, T>` | `IService<OtherService, int>`

The built-in target container implementations (specifically <xref:Rezolver.TargetContainer>)
auto-detect 
Rezolver's 
