# Rolling your own `IMemberBindingBehaviour`

If you find you need more control over which properties and/or fields you want bound on an instance over and
above that provided via the [standard behaviours](index.md#standard-behaviours) and [fluent API](fluent-api.md), 
then you can of course implement your own binding behaviour by implementing the @Rezolver.IMemberBindingBehaviour
type.

This interface has one method to implement - @Rezolver.IMemberBindingBehaviour.GetMemberBindings* - which is expected
to return an array of @Rezolver.MemberBinding objects.  This is automatically called by the 
@Rezolver.Targets.ConstructorTarget.Bind* method, which is used when Rezolver compiles a 
@Rezolver.Targets.ConstructorTarget to service a request for a type from the container.

The @Rezolver.MemberBinding class describes a member as being bound to the result of another @Rezolver.ITarget, 
which means that it's not tied to simply resolving an instance from the container: any of the target 
implementations can be used and Rezolver will correctly bind it to the result of that target.

Now you've got an overview of what to do, let's get on with our example.

## Attribute-based member injection

A popular implementation of member injection in some IOC containers is to use an attribute on the properties/fields 
which should be injected.  This is not something that Rezolver supports out of the box - however it's a trivial
thing to implement yourself.

To do this, we will:

- Add a new attribute `InjectAttribute` which we will use to mark the properties that we want injected
- Implement a custom `IMemberBindingBehaviour` (`AttributeBindingBehaviour`) to bind only properties which have this attribute applied
  - We're inheriting from the @Rezolver.BindAllMembersBehaviour for this
- Decorate one or more members on a type with the `InjectAttribute`
- Pass the new `AttributeBindingBehaviour` when we register the type, or create the `ConstructorTarget`

First, the `InjectAttribute`:

[!code-csharp[InjectAttribute.cs](../../../../../test/Rezolver.Tests.Examples/Types/InjectAttribute.cs#example)]

Then our type which uses it:

[!code-csharp[HasAttributeInjectedMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/HasAttributeInjectedMembers.cs#example)]

Finally, our binding behaviour - which extends the aforementioned @Rezolver.BindAllMembersBehaviour as that class has plenty of
extension points, and already has the logic built in to walk all properties and fields for a type and its bases.

[!code-csharp[AttributeBindingBehaviour.cs](../../../../../test/Rezolver.Tests.Examples/Types/AttributeBindingBehaviour.cs#example)]

With that in place, we can then test:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example5)]

> [!TIP]
> You can, of course, then enable member injection for all constructor-injected types (including open generics) simply by registering 
> this behaviour as a global option, [as shown earlier in this topic](#global-option).

Hopefully this example will inspire you to create your own custom binding behaviour :)