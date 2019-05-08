# Using the Fluent API to customise member bindings

All of the [standard binding behaviours](index.md#standard-behaviours), except the 
@Rezolver.MemberBindingBehaviour.BindNone behaviour of course, bind groups of members based on blanket
rules - i.e. either or both publicly writeable properties and fields.

But Rezolver also provides a convenient way to customise both which members are injected, but also *how*.

Rezolver has another behaviour implementation for this - the @Rezolver.BindSpecificMembersBehaviour class - which 
accepts either [an enumerable of members](xref:Rezolver.BindSpecificMembersBehaviour.%23ctor(System.Collections.Generic.IEnumerable{System.Reflection.MemberInfo})) 
to be auto-injected, or 
[a set of member bindings](xref:Rezolver.BindSpecificMembersBehaviour.%23ctor(System.Collections.Generic.IEnumerable{Rezolver.MemberBinding}))
(i.e. instances of the @Rezolver.MemberBinding class) containing the members to be bound, along with the bindings that are to be applied to them.

This behaviour adopts an entirely opt-in approach, meaning that only the properties or fields you tell it about will be
bound.

So, if you had a type with two properties, but you only wanted one of them to be bound, you just pass
that property's @System.Reflection.PropertyInfo to the behaviour instance when you build it, and away you go.

The problem, however, is that all that reflection can be very untidy, slow, and ultimately not terribly
quick or efficient for you, the developer.

This is where the fluent member bindings API comes in, which provides an easy way to build a custom 
@Rezolver.IMemberBindingBehaviour (which will be an instance of the aforementioned @Rezolver.BindSpecificMembersBehaviour
class) for any (concrete) type, which can then be provided when registering or when creating a @Rezolver.Targets.ConstructorTarget.  

## The Behaviour Builder

The backbone of this approach is the interface @Rezolver.IMemberBindingBehaviourBuilder`1 (and its default implementation 
<xref:Rezolver.MemberBindingBehaviourBuilder`1>) - which provides a way to create and configure an @Rezolver.IMemberBindingBehaviour 
(specifically an instance of <xref:Rezolver.BindSpecificMembersBehaviour>) through its two methods:

1) The @Rezolver.IMemberBindingBehaviourBuilder`1.Bind* method lets you create per-member instructions for the type 
whose members are to be bound.  This method returns an @Rezolver.MemberBindingBuilder`2 instance that can then be used
to customise *how* that member is bound.

2) The @Rezolver.IMemberBindingBehaviourBuilder`1.BuildBehaviour then creates an instance of the behaviour which 
binds the properties/fields of an instance as previously configured.  This instance can be reused for multiple
targets and registrations.

### Creating a Behaviour Builder

Since the constructor for the <xref:Rezolver.MemberBindingBehaviourBuilder`1> is internal, you have to use the 
@Rezolver.MemberBindingBehaviour.For* factory method to create one:

```cs
var behaviourBuilder = MemberBindingBehaviour.For<MyClass>();
```

### Adding a Member Binding

As mentioned above, once created, you can then start adding properties/fields to be bound, by calling the `Bind` method:

```cs
// adds an instruction to the behaviour builder to ensure that the member 'BindableMember'
// is bound when an instance is created.  
behaviourBuilder.Bind(o => o.BindableMember);
```

This configures the member to be injected via the equivalent of a @Rezolver.Container.Resolve* call for 
the member's type from the container that is producing the enclosing instance.

### Configuring a Member Binding

The `Bind` method's result - the @Rezolver.MemberBindingBuilder`2 class - allows you to customise how a member 
will be bound, through a set of instance methods:

```cs
// Explicitly binds to the type 'SomeOtherType' or 'AnotherType'
// The generic version is strongly-typed - if the specified type is not 
// reference compatible with the member's type, it won't compile.
behaviourBuilder.Bind(o => o.BindableMember)
    .ToType<SomeOtherType>();
// Or...
behaviourBuilder.Bind(o => o.BindableMember)
    .ToType(typeof(AnotherType));

// Explicitly binds it to a constant service
behaviourBuilder.Bind(o => o.BindableMember)
    .ToObject(new SomeOtherType());

// Explicitly binds to *any* target provided by the Rezolver framework,
// shown here as a delegate which requires an instance of 'HiddenService' 
// to be auto-injected injected by the container
behaviourBuilder.Bind(o => o.BindableMember)
    .ToTarget(Target.ForDelegate((HiddenService s) => new ComplexType(s)));
```

### Putting it all together

Since this sold as a fluent API, you can chain all of this together to customise and create a new 
customised behaviour in one line:

```cs
var behaviour = MemberBindingBehaviour.For<MyClass>()
    .Bind(o => o.BindableMember)
    .Bind(o => o.AnotherMember)
        .ToType<SpecificType>()
    .Bind(o => o.CollectionMember)
        .AsCollection()
    .BuildBehaviour();
```

> [!WARNING]
> If you specify the same member multiple times, an exception is thrown.

> [!TIP]
> The `.AsCollection()` call above is covered in the [collection member binding topic](collections.md).  Briefly,
> you will only need to explicitly mark a member as requiring collection binding if it's exposed through a
> writeable property.  Compliant read-only collection members will automatically be treated as requiring 
> a collection binding.  Read [the topic](collections.md) for more.

* * *

## More Examples

We've covered pretty much everything you need to know about using the fluent API, however Rezolver has a few
other tricks up its sleeve for when you are either creating @Rezolver.Targets.ConstructorTarget or registering 
one.  On many of the methods which accept an `IMemberBindingBehaviour` you will also find an overload 
which accepts an `Action<IMemberBindingBehaviourBuilder<T>>`.  This is a callback you can provide to perform
inline configuration of a dedicated member binding for that target or registration - without having to create
the behaviour builder yourself.

Here are some formal examples in keeping with the rest of the documentation on the site - in the form of XUnit 
tests that you can run yourself, if you clone the repo.

Consider the `Has2InjectableMembers` type that we've used in the [options](options.md) and 
[per-registration](per-registration.md) examples, which has two writeable properties:

[!code-csharp[Has2InjectableMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/Has2InjectableMembers.cs#example)]

First, we'll configure just one of the members for injection using the techniques shown above, but as we
register the type with the @Rezolver.TargetContainerExtensions.RegisterType``1(Rezolver.ITargetContainer,System.Action{Rezolver.IMemberBindingBehaviourBuilder{``0}})
extension method:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example6)]

A similar approach is also possible when using the 
@Rezolver.Target.ForType``1(System.Action{Rezolver.IMemberBindingBehaviourBuilder{``0}}) static method to create
a new target:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example7)]

### Customised member bindings

As shown earlier, we can also redirect the binding of a member to a different type.  So, consider this seemingly 
unfriendly type:

[!code-csharp[Has2IdenticalMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/Has2IdenticalMembers.cs#example)]

Clearly, if we bind these two members as we've done previously, both members will receive the same injected
service - so we need to override how those members will be injected:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example8)]

### Explicit collection binding

As intimated earlier, the `.AsCollection` overload can be used to control collection member injection.  A thorough 
example of this can be found [over in the collection binding topic](collections.md#explicit-injection-fluent-api).

* * *

# Next steps

- It might be that this fluent API isn't enough for what you need, so you might want to check out 
[how to create your own member binding behaviour](custom.md).
- Check out [how collection member binding works](collections.md) if you haven't already - it's supported both
by the [standard behaviours](index.md#standard-behaviours) and by the fluent API.