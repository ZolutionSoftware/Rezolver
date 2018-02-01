# Member Injection

When executing a type's constructor you also have the option to inject services into the new instance's properties
and/or fields.

In other words, imagine writing code like this:

```cs
var service = new ServiceWithFields()
{
    Property = foo,
    Field = bar,
    CollectionProperty = {
        1,
        2,
        3
    }
}
```

> [!NOTE]
> Collection initialisation is new in 1.3.2

This is all done via implementations of the @Rezolver.IMemberBindingBehaviour interface, and this section and its subsections
cover all the ways in which we can enable its use in order to assign values to members of a new object created by a Rezolver
container.

There are two main topics to cover:
- The different types of member binding behaviour
- How to instruct Rezolver to use a member binding behaviour when creating an instance

# Different types of Member Binding Behaviour

## Standard behaviours

Rezolver comes with four basic behaviours which cover the most basic of scenarios, all of which are available
from the @Rezolver.MemberBindingBehaviour static class as static properties:

1. **@Rezolver.MemberBindingBehaviour.BindNone**: prevents binding of any field or properties (implemented by <xref:Rezolver.BindNoMembersBehaviour>).
This is typically used to prevent member binding of certain types when it has been enabled for a whole group of types (perhaps with a common base)
via [container options](options.md).
2. **@Rezolver.MemberBindingBehaviour.BindAll**: binds all publicly writeable properties and fields to values from the container, even
if the container doesn't have a registration for the member's type.  Implemented by an instance of @Rezolver.BindAllMembersBehaviour.
3. **@Rezolver.MemberBindingBehaviour.BindProperties**: binds only publicly writeable properties to values from the container.  Fields are
ignored.  Implemented by an instance of @Rezolver.BindPublicPropertiesBehaviour.
4. **@Rezolver.MemberBindingBehaviour.BindFields**: binds only public fields to values from the container.  Properties are
ignored.  Implemented by an instance of @Rezolver.BindPublicFieldsBehaviour.

## Custom (via the Fluent API)

For fine-grained control over the members you want bound on a new instance of a given type, you can build a custom member binding behaviour 
through the fluent API that's exposed by the @Rezolver.MemberBindingBehaviour.For* static method.  This uses an opt-in approach to member binding, 
whereby only those properties you specify will be bound.

By default, the member values are then resolved from the container in the same way as the standard behaviours listed above, but you can also
override that behaviour to provide your own targets.

Collection initialisation can also be customised through this API.

For more, go to the [Fluent API topic](fluent-api.md) in this section.

## Custom (roll your own)

You can also provide your own implementation of the @Rezolver.IMemberBindingBehaviour interface for a completely custom member binding behaviour.

Many implementations start off by inheriting from the @Rezolver.BindAllMembersBehaviour - and we have a 
[step-by-step guide on how to do this](custom.md), which shows how to implement a member binding behaviour which
uses an attribute declared on fields or properties to select the members that should be bound (functionality which
is the default in some other containers).

# Enabling member injection

There are two ways to enable member injection in Rezolver, which are covered in two dedicated subsections.

## Via Container Options

The simplest way to configure member injection is through the Options API, which provides a way to define metadata globally or on a per-type
basis.  This approach is useful if you want the same member binding behaviour to be applied to all registrations
in your application, or to specific types of object (or subtypes), such as all instances of an open generic.

[Find out more about enabling member injection through the options API](options.md).

## Per-registration

When a @Rezolver.Targets.ConstructorTarget or @Rezolver.Targets.GenericConstructorTarget is created, you can pass
an @Rezolver.IMemberBindingBehaviour to its constructor.  This applies to most of the target creation and registration
APIs, too.

[Find out more about configuring member injection on a per-target/per-registration basis](per-registration.md).

* * *

With its comprehensive support for member injection, Rezolver should be able to support almost any scenario you 
need for your application.  Whilst it strictly violates the DI pattern, which lies at the heart of IOC-world,
the reality is that sometimes you just need to do it.

So now you know thee's no excuse: need to inject members?  Rezolver has you covered!