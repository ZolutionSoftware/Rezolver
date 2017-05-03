# Member Injection

When executing a type's constructor you also have the option to inject services into the new instance's properties
and/or fields.

So - the equivalent of:

```cs
var service = new ServiceWithFields()
{
    Property = foo,
    Field = bar
}
```

This is all done via implementations of the @Rezolver.IMemberBindingBehaviour interface - of which there are a few implementations
supplied by Rezolver, which can be found in the @Rezolver.MemberBindingBehaviour static class:

1. **@Rezolver.MemberBindingBehaviour.BindNone**: prevents binding of any field or properties (implemented by <xref:Rezolver.BindNoMembersBehaviour>)
2. **@Rezolver.MemberBindingBehaviour.BindAll**: binds all publicly writeable properties and fields to values from the container, even
if the container doesn't have a registration for the member's type.  Implemented by an instance of @Rezolver.BindAllMembersBindingBehaviour.
3. **@Rezolver.MemberBindingBehaviour.BindProperties**: binds only publicly writeable properties to values from the container.  Fields are
ignored.  Implemented by an instance of @Rezolver.BindPublicPropertiesBehaviour.
4. **@Rezolver.MemberBindingBehaviour.BindFields**: binds only public fields to values from the container.  Properties are
ignored.  Implemented by an instance of @Rezolver.BindPublicFieldsBehaviour.

Almost all of the ways in which you can create @Rezolver.Targets.ConstructorTarget
(factory methods, registration methods, constructors etc) will also accept a @Rezolver.IMemberBindingBehaviour with which to enable 
member injection if you want to.


* * *

## Example - Injecting all members

Given this type:

[!code-csharp[Has2InjectableMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/Has2InjectableMembers.cs#example)]

We simply setup the container to build all three types and make sure to pass the binding behaviour when registering the
`Has2InjectableMembers` target:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example1)]

> [!WARNING]
> As intimated above, when using this behaviour, if one of the services required by any of the writable properties or 
> fields is missing, then the resolve operation will fail.
> 
> The behaviour is *deliberately* primitive - because unlike
> with constructors, where it's reasonable to assume all parameters are to be injected, there are no simple rules
> that can always applied to an object to determine which properties and/or fields should be auto-injected.

* * *

## TODO: Example using @Rezolver.GlobalBehaviours

## Example - Custom binding behaviour

If you find you need more control over which properties and/or fields you want bound on an instance, then you can, 
of course, implement your own binding behaviour.

A popular implementation of member injection in some IOC containers is to use an attribute on the properties/fields 
which should be injected.  This is, of course, not something that Rezolver supports out of the box - however it's a trivial
thing to implement yourself, and that's what this example does.

We will:

- Add a new attribute `InjectAttribute` which we will use to mark the properties that we want injected
- Implement a custom `IMemberBindingBehaviour` (`AttributeBindingBehaviour`) to bind only properties which have this attribute applied
- Decorate one or more members on a type with the `InjectAttribute`
- Pass the new `AttributeBindingBehaviour` when we register the type, or create the `ConstructorTarget`

First, the `InjectAttribute`:

[!code-csharp[InjectAttribute.cs](../../../../../test/Rezolver.Tests.Examples/Types/InjectAttribute.cs#example)]

Then our type which uses it:

[!code-csharp[HasAttributeInjectedMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/HasAttributeInjectedMembers.cs#example)]

Finally, our binding behaviour - which extends the aforementioned @Rezolver.DefaultMemberBindingBehaviour as that class has plenty of
extension points:

[!code-csharp[AttributeBindingBehaviour.cs](../../../../../test/Rezolver.Tests.Examples/Types/AttributeBindingBehaviour.cs#example)]

With that in place, we can then test:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example2)]


Hopefully this example will inspire you to create your own custom binding behaviour :)

* * *

# Next steps

- Head back to the [Constructor topic](index.md)
- Or, see how [constructor injection works with generic types](generics.md)