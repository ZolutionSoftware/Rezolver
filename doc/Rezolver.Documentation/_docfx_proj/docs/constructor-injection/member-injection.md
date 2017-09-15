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

## Via Container Options

The simplest way to configure member injection is through the Options API, which provides a way to define metadata globally or on a per-type
basis.  To do this, you need an @Rezolver.ITargetContainer, an interface which is implemented by all the standard container classes, in addition
to the primary implementation types @Rezolver.TargetContainer and @Rezolver.OverridingTargetContainer.

This short example shows how to set the default member binding behaviour for all constructed types to the @Rezolver.MemberBindingBehaviour.BindAll
behaviour, thus enabling member injection for all publicly writeable properties and fields:

```cs
using Rezolver;

namespace MyApp
{
    public static void Main()
    {
        var container = new Container();
        container.SetOption(MemberBindingBehaviour.BindAll);
        // objects created through constructor injection by this container will now also have their members injected

        // you can also set a specific behaviour for a type in this way
        container.SetOption(typeof(ClassWithProperties), MemberBindingBehaviour.BindProperties);
        // now, any instance of ClassWithProperties created by constructor injection will have only its properties injected

        // also works for open generics
        container.SetOption(typeof(GenericWithFields<>), MemberBindingBehaviour.BindFields);
        // any instance of any generic based on GenericWithFields<> will have only its fields injected
    }
}

```

## Via Explicit @Rezolver.IMemberBindingBehaviour 

Almost all of the ways in which you register types for construction by the container
(be it via @Rezolver.ITarget factory methods like @Rezolver.Target.ForType*, registration extension methods such as those in 
@Rezolver.RegisterTypeTargetContainerExtensions, or any of the constructors for @Rezolver.Targets.ConstructorTarget or 
@Rezolver.Targets.GenericConstructorTarget constructors etc) will accept an @Rezolver.IMemberBindingBehaviour which is 
specific to the target being created.

Such explicitly provided member binding behaviours will *override* any which might otherwise be applied via the Options
API (as shown in the last example).

```cs
    container.RegisterType<MyClass>(MemberBindingBehaviour.BindAll);
```

The rest of the examples in this topic will show both in action.

* * *

# Examples

## Injecting all members

Given this type:

[!code-csharp[Has2InjectableMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/Has2InjectableMembers.cs#example)]

We simply setup the container to build all three types and make sure to pass the binding behaviour when registering the
`Has2InjectableMembers` target.  We'll see how to do this using both an explicit @Rezolver.IMemberBindingBehaviour, but
also via the Options API.

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

### Global Option

In this version of the example, we'll set the @Rezolver.MemberBindingBehaviour.BindAll behaviour as a global option on 
the container itself.

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example2)]

### Service-specific option

But what if we only want instances of one type (or any of its derivatives) to have their members injected?

Well, instead of setting the option globally, we can associate it with a particular service type:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example3)]

Clearly, in this case we might just as well use an explicitly-provided member binding behaviour, as shown two examples 
above, however this facility is useful if you have a whole hierarchy of objects with a common interface or
base whose properties and/or fields should be auto-injected.

* * *

## Custom binding behaviour

> [!TIP]
> The options approach shown in the previous two examples will work also with the the custom binding behaviour
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

> [!TIP]
> You can, of course, then enable member injection for all constructor-injected types (including open generics) simply by registering 
> this behaviour as a global option, [as shown earlier in this topic](#global-option).

Hopefully this example will inspire you to create your own custom binding behaviour :)

* * *

# Next steps

- Head back to the [Constructor topic](index.md)
- Or, see how [constructor injection works with generic types](generics.md)