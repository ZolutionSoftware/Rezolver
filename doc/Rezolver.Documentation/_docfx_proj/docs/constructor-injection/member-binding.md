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

This is all done via implementations of the @Rezolver.IMemberBindingBehaviour interface - of which there is currently one
implementation - @Rezolver.DefaultMemberBindingBehaviour.

Almost all of the ways in which you can create @Rezolver.Targets.ConstructorTarget
(factory methods, registration methods, constructors etc) will also accept a @Rezolver.IMemberBindingBehaviour with which to enable 
member injection if you want to.

> [!WARNING]
> @Rezolver.DefaultMemberBindingBehaviour will be renamed to `AllMembersBindingBehaviour` in 1.2, and might also
> go into a child namespace - e.g. `Rezolver.MemberBindingBehaviours`.

@Rezolver.DefaultMemberBindingBehaviour automatically binds all ***publicly writeable*** properties and fields on the type being constructed
with values from the container when resolved - *regardless* of whether the required services exist in the container.

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

*It's likely that two extra 'default' behaviours - `PropertiesOnlyBindingBehaviour` and `FieldsOnlyBindingBehaviour`, which
will only bind publicly writeable properties __or__ public fields, respectively -
will be added to the framework in 1.2*

* * *

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