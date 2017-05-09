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
if the container doesn't have a registration for the member's type.  Implemented by an instance of @Rezolver.BindAllMembersBehaviour.
3. **@Rezolver.MemberBindingBehaviour.BindProperties**: binds only publicly writeable properties to values from the container.  Fields are
ignored.  Implemented by an instance of @Rezolver.BindPublicPropertiesBehaviour.
4. **@Rezolver.MemberBindingBehaviour.BindFields**: binds only public fields to values from the container.  Properties are
ignored.  Implemented by an instance of @Rezolver.BindPublicFieldsBehaviour.

# Enabling member injection

## Via Container behaviours

Container behaviours were introduced in 1.2, and provide a way to configure additional services used by any @Rezolver.ContainerBase derived
object (that's all @Rezolver.IContainer implementations you're likely to use) created by your application.

Container behaviours can be supplied to these container classes on construction (for example, the 
@Rezolver.Container.%23ctor(Rezolver.ITargetContainer,Rezolver.IContainerBehaviour) @Rezolver.Container constructor) or, if
not provided, then a default will be used from the @Rezolver.GlobalBehaviours class.  This class has two global behaviours for
@Rezolver.IContainer objects:

1. **@Rezolver.GlobalBehaviours.ContainerBehaviour** - Behaviour to be used by the majority of containers
2. **@Rezolver.GlobalBehaviours.OverridingContainerBehaviour** - Behaviour to be used by @Rezolver.OverridingContainer

The @Rezolver.MemberBindingBehaviour.BindNone @Rezolver.IMemberBindingBehaviour (see above) is configured as
the default in the first of these two container behaviours - which means that it becomes the default for all 
@Rezolver.IContainer.Resolve* operations (including those of @Rezolver.OverridingContainer objects, since they
fall back to their overriden container if unable to resolve a particular type).

If you want to change this default for containers in your application (e.g.
to the @Rezolver.MemberBindingBehaviour.BindAll behaviour), you can use the 
@Rezolver.ContainerBehaviourCollectionExtensions.UseMemberBindingBehaviour* extension method:

```cs
using Rezolver;

namespace MyApp
{
    public static void Main()
    {
        // GlobalBehaviours sits directly in the Rezolver namespace
        GlobalBehaviours.ContainerBehaviour
            .UseMemberBindingBehaviour(MemberBindingBehaviour.BindAll);
            
        // all containers will now bind all members by default, unless
        // a different IMemberBindingBehaviour is passed to a ConstructorTarget
        // (or any of the other methods/types which create it)
    }
}

```

Whilst this is the simplest way to do it, your application might have multiple containers, and you therefore
might wish to control their behaviour independently.  If so, then you can also supply a specific set of behaviours to your 
container when you create it.

> [!NOTE]
> If you do this, then it's best to clone the existing global default one first, and then
> reconfigure it, because there are other services configured in a container behaviour collection which are required,
> such as an `IContainerBehaviour<ITargetCompiler>`.

You'll see how to use custom container behaviours in an example further down this page.

## Via Explicit @Rezolver.IMemberBindingBehaviour 

Almost all of the ways in which you register types for construction by the container
(be it via @Rezolver.ITarget factory methods like @Rezolver.Target.ForType*, registration extension methods such as those in 
@Rezolver.RegisterTypeTargetContainerExtensions, or any of the constructors for @Rezolver.Targets.ConstructorTarget or 
@Rezolver.Targets.GenericConstructorTarget constructors etc) will accept an @Rezolver.IMemberBindingBehaviour which is 
specific to the target being created - overriding any behaviour configured by container behaviors.

It is this method of passing @Rezolver.IMemberBindingBehaviour that most of the examples here use.

* * *

# Examples

## Injecting all members

Given this type:

[!code-csharp[Has2InjectableMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/Has2InjectableMembers.cs#example)]

We simply setup the container to build all three types and make sure to pass the binding behaviour when registering the
`Has2InjectableMembers` target.  We'll see how to do this using both an explicit @Rezolver.IMemberBindingBehaviour, but
also via an @Rezolver.IContainerBehaviour.

### Explicit @Rezolver.IMemberBindingBehaviour

Here we'll register the `Has2InjectableMembers` type in the container with the @Rezolver.MemberBindingBehaviour.BindAll behaviour
passed with the registration.  Doing this overrides the container's default behaviour, thus guaranteeing that the members are
always bound.

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example1)]

> [!WARNING]
> When using the @Rezolver.MemberBindingBehaviour.BindAll behaviour, if one of the services required by any of the writable 
> properties or fields is not present in the container, then the resolve operation will fail.
> 
> The behaviour is *deliberately* primitive - because unlike
> with constructors, where it's reasonable to assume all parameters are to be injected, there are no simple rules
> that can always applied to an object to determine which properties and/or fields should be auto-injected.

### @Rezolver.IContainerBehaviour (Global)

In this version of the example, we'll change the container behaviour defined in @Rezolver.GlobalBehaviours.ContainerBehaviour
so that binding all members becomes the default for all containers.

> [!NOTE]
> If you're running the examples test project in Visual Studio, then it's recommended you disable parallel execution, because 
> it could cause other tests to fail when this one runs!

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example2)]

### @Rezolver.IContainerBehaviour (Explicit)

Our final demonstration of @Rezolver.IContainerBehaviour - here we clone the current global behaviour and then replace
whatever the default @Rezolver.IMemberBindingBehaviour is with the @Rezolver.MemberBindingBehaviour.BindAll behaviour.

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example3)]

* * *

## Custom binding behaviour

> [!TIP]
> The @Rezolver.IContainerBehaviour approach shown in the previous example will work also with the the custom binding behaviour
> shown in this example.

If you find you need more control over which properties and/or fields you want bound on an instance, then you can, 
of course, implement your own binding behaviour.

A popular implementation of member injection in some IOC containers is to use an attribute on the properties/fields 
which should be injected.  This is not something that Rezolver supports out of the box - however it's a trivial
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

Finally, our binding behaviour - which extends the aforementioned @Rezolver.BindAllMembersBehaviour as that class has plenty of
extension points:

[!code-csharp[AttributeBindingBehaviour.cs](../../../../../test/Rezolver.Tests.Examples/Types/AttributeBindingBehaviour.cs#example)]

With that in place, we can then test:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example5)]

Hopefully this example will inspire you to create your own custom binding behaviour :)

* * *

# Next steps

- Head back to the [Constructor topic](index.md)
- Or, see how [constructor injection works with generic types](generics.md)