# Member Injection via Options

As described in the [overview](index.md#via-container-options), you can use the Rezolver Options
API to configure member injection for an entire container or group of objects.

To do this, you need an @Rezolver.ITargetContainer, an interface which is implemented by all the standard container classes, in addition
to the primary implementation types @Rezolver.TargetContainer and @Rezolver.OverridingTargetContainer.

As with the [topic on per-registration member injection](per-registration.md), we'll start by using the type
`Has2InjectableMembers` defined in the examples project:

[!code-csharp[Has2InjectableMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/Has2InjectableMembers.cs#example)]

First, then, we'll set the @Rezolver.MemberBindingBehaviour.BindAll behaviour as a global option on 
the container itself.

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example2)]

What this does is to enable member injection for *every* type that is built by constructor injection - including
open generics.

You're probably not likely to want to enable injection for all fields and properties across the board
in your application, as it's highly unlikely that all the types you want your container to build will have
been written that way.

If, however, you want to use attributes, or something similar, to control which fields 
and/or properties are bound globally - then you will want to check out the 
[custom binding behaviour walkthrough](custom.md) and then add that as a global registration.

## Per-Service-Type

But what if we only want instances of one type to have their members injected?

To achieve this without resorting to [per-registration behaviours](per-registration.md), instead of setting 
the option globally, we can associate it with a particular service type.

In the following example, we're registering an additional type, `AlsoHas2InjectableMembers`, which is a clone 
of the `Has2InjectableMembers` type, but otherwise unrelated.  Then, when we set the option to enable the 
@Rezolver.MemberBindingBehaviour.BindAll behaviour, we set it only for the `Has2InjectableMembers` type.

As a result, when we resolve the two instances at the end, only one of them will have their members injected:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example3)]

## Per-Hierarchy

When you set an option for a service type, it also automatically applies for any of its derivative types, 
which, for member injection, means:

- If you set it for a class, then it also applies to any derived type being created by constructor injection
- If you set it for an interface, then it applies to any class or value type which *implements* that interface and
which is being created by constructor injection.

To demonstrate, this example sets the @Rezolver.MemberBindingBehaviour.BindAll behaviour on the 
`Has2InjectableMembers` service type again; but this time it registers and resolves an instance of the type
`Inherits2InjectableMembers` - which is an empty class that derives from `Has2InjectableMembers`:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example4)]

> [!TIP]
> You can still define specific behaviours for derived types, either through the options API or on a 
> [per-registration](per-registration.md) basis, to override behaviours which are set at the base-class/interface
> level.

## Open generics

When we create a registration for an open generic and pass an @Rezolver.IMemberBindingBehaviour, all instances 
that are then produced by that registration - for any closed generic - will then use that member binding behaviour.

This is also possible with container options - simply by passing a reference to the open 
generic type in the `serviceType` parameter of the `SetOption` method:

```cs
container.SetOption(MyBehaviour, typeof(MyGeneric<>));
```

The beauty of this approach being that this behaviour will be used by default even for registrations against 
closed variants of that generic type (so long as the registration uses constructor injection).

> [!NOTE]
> This also combines with the functionality described in the previous section - meaning that not only
> will the behaviour kick in for any variant of `MyGeneric<>`, but also any class derived from *any* closed or
> open variant of `MyGeneric<>`.  In the case of a generic interface, then it kicks in for any type which implements
> any variant of that interface.