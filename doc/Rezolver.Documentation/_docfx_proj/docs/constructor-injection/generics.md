# Generic Constructor Injection

> [!TIP]
> Before reading this you should read through the section on [construction injection](index.md)
>
> Also, generic types - especially in IOC-world - are an advanced concept that's easy to get very confused over, so if this is entirely new to you
> and you find you don't understand it fully, then don't worry: this isn't easy!
>
> It's probably a good idea to read the MSDN documentation about the @System.Type.IsGenericType property on @System.Type for more about 'open' 
> generics.

In Rezolver, binding open generic types, such as `IFoo<T>` or `IBar<T, U, V>`, to constructors of generics, e.g. `Foo<T>` or `Bar<T, U, V>`
is done via the @Rezolver.Targets.GenericConstructorTarget type.

This class knows how to map to a closed generic - i.e.
`Foo<IMyService>` or `IBar<MyService1, MyService2, MyService3>` - from an open generic and then subsequently bind
to the constructor of the closed version of that generic when requested from the container.

## 'Best-match' only for now

The [best-match algorithm](index.md#best-match-examples) described in the [constructor injection](index.md) is the only option currently available
to you when using @Rezolver.Targets.GenericConstructorTarget.  Theoretically, it's possible to specify the `ConstructorInfo` for the constructor you
want bound on the concrete generic type that Rezolver eventually decides to build for you, but we have not implemented that functionality yet.

You also cannot provide named arguments to help in the best-match search - to do so would be incredibly difficult, since if the type of a constructor 
argument is dependant upon a type argument, then there's no way you could supply a binding for it up-front to satisfy all possible types that 
might be passed to it.

## Creating/registering <xref:Rezolver.Targets.GenericConstructorTarget>s

You can, of course, simply use the constructors to build a new instance of this target - e.g: `new GenericConstructorTarget(typeof(Foo<>))`.

However, the non-generic versions of the @Rezolver.RegisterTypeTargetContainerExtensions.RegisterType* method that we've been using elsewhere to 
register 'simple' types in our target containers are also generic type-aware.  So, if you specify an open generic type as the implementing type, 
then those functions automatically create a @Rezolver.Targets.GenericConstructorTarget for you and register it.  

So, `container.RegisterType(typeof(MyGeneric<>))` will register `MyGeneric<>` for all variants of itself, and 
`container.RegisterType(typeof(MyGeneric<>), typeof(IMyGeneric<>))` will register `MyGeneric<>` for all variants of `IMyGeneric<>`.

Lets get on with some examples, the first few of which use these types:

[!code-csharp[IDataFormatter`1.cs](../../../../../test/Rezolver.Tests.Examples/Types/IDataFormatter`1.cs#example)]

[!code-csharp[DataFormatter`1.cs](../../../../../test/Rezolver.Tests.Examples/Types/DataFormatter`1.cs#example)]


* * *

# Basic examples

## Direct (no base/interface)

Similar to the examples for 'normal' constructor injection, we'll start by looking at registering without worrying about bases or interfaces, 
registering `DataFormatter<>` and fetching a few different closed variants of that generic:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/GenericConstructorExamples.cs#example1)]

* * *

## By Interface

And again, except this time we'll register and resolve against the `IDataFormatter<>` interface:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/GenericConstructorExamples.cs#example2)]

* * *

## Generic Dependency

This time, we have a generic type that has a dependency on another generic type whose generic argument is derived from the dependant's
generic argument:

[!code-csharp[RequiresIDataFormatter`1.cs](../../../../../test/Rezolver.Tests.Examples/Types/RequiresIDataFormatter`1.cs#example)]

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/GenericConstructorExamples.cs#example3)]

* * *

# Advanced - Complex Hierarchies

The previous examples showed how we can resolve an instance when the type requested is simply a closed version of the open generic against which
we registered a target; or when it's a direct base or interface of it.

Naturally, real-world type hierarchies do not always work like that - and sometimes you'll want to implement a type where the implementation is perhaps
more 'distant', in inheritance terms, from the base or interface.  With generics, this can get particularly complicated as the type parameters do not
always remain consistent - consider the following hierarchy:

[!code-csharp[ComplexGenerics.cs](../../../../../test/Rezolver.Tests.Examples/Types/ComplexGenerics.cs#example)]

Notice how on the three first generic types, the order of the type arguments is changing as we move through `BaseGeneric<,,>`, `MidGeneric<,,>` and
`FinalGeneric<,,>`.

The last generic actually fixes one of the type parameters passed to `FinalGeneric<,,>` - which introduces a different problem for our container, which
we'll get to after looking at the others.

*We apologise in advance for the complexity of this argument jumbling - but we accept no liability for any headaches you suffer!*

* * *

## Base of a Base with Jumbled Args

So, yes - this is a deeply contrived example, but it's worth seeing:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/GenericConstructorExamples.cs#example10)]

In order to do this, the container must walk the inheritance chain of the @Rezolver.ITarget.DeclaredType of the @Rezolver.Targets.GenericConstructorTarget
to create a map of the type arguments which must be fed to `FinalGeneric<,,>` in order to create an instance of `BaseGeneric<,,>`.

* * *

## Partially Closed Generic

> [!NOTE]
> The term 'Partially Closed' is a term only used here - it merely describes a generic type where *some* of the type arguments supplied to
> a generic type's parameters are concrete types.

Now we take a look at the `ClosingGeneric<T, U>` type - it's here we start getting into one of the murkier areas of generic types in 
Rezolver and, indeed, in general.

In this case, we now have a type which can *only ever* be used
to implement a *subset* of all the possible variants of `BaseGeneric<T, U, V>`, because, whatever its own `T` and `U` arguments are, its base will
always be `BaseGeneric<string, T, U>`.

Rezolver is happy to work with this, as the next test shows:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/GenericConstructorExamples.cs#example11)]

> [!NOTE]
> If we were to try to resolve `BaseGeneric<T1, T2, T3>` as we did before, then the container will *correctly* tell us that it cannot - and
> that the registered target is not compatible with that type.  Which leads us on to our problem...

### The problem with partially closed generics

With our container configured as above, it is now *impossible* to resolve `BaseGeneric<T1, T2, T3>` because the only implementation we have for 
`BaseGeneric<,,>` is one where the first type argument ***must*** be `string`.  If we were to register another, more general, type for 
`BaseGeneric<,,>` it would overwrite our specialised version, wiping it out.

It's also impossible for us to specify the type `BaseGeneric<string,,>` either in a `typeof`, or via the @System.Type.MakeGenericType* API, so we're
stuck.

### We're working on it!

This is a limitation of Rezolver right now, but there are plans to implement partially specialised registrations, so that the container would, when
`ClosingGeneric<T, U>` is registered, realise that it should *only* be used if the first type parameter on `BaseGeneric<,,>` is `string`.

This would then allow the container to accept a more general registration against `BaseGeneric<,,>` to be registered side-by-side and used for 
all other variants of `BaseGeneric<,,>`.

Implementation of this feature would also simplify the scenarios covered in the next section.

* * *

# Advanced - Nested Generics

Rezolver also supports generics where type arguments are passed to an interface or base nested within other generics - take these types:

[!code-csharp[NestedGenerics.cs](../../../../../test/Rezolver.Tests.Examples/Types/NestedGenerics.cs#example)]

## Singly nested generic argument

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/GenericConstructorExamples.cs#example20)]

> [!NOTE]
> As with our `ClosingGeneric<,>` from before, we *could* perform
> this registration against `IGenericService<>`, but if we do, then we wipe out
> the possibility of being able to register other services - so, we
> register against `IGenericService<IEnumerable<>>`, which requires some 
> @System.Type.MakeGenericType* jiggery pokery.  When we've got partially closed generics
> working, you will be able to register against `IGenericService<>`.

* * *

## Doubly nested generic argument

Rezolver doesn't care how far it has to go to work out how a type argument on a base is nested from the implementing type:

[!code-csharp[Example.cs](../../../../../test/Rezolver.Tests.Examples/GenericConstructorExamples.cs#example21)]

* * *

# Using Member Bindings

The same techniques that are shown in our [member binding documentation](member-binding.md) also work for generic types - if you supply an
@Rezolver.IMemberBindingBehaviour to the @Rezolver.Targets.GenericConstructorTarget when it is created (either by the constructors or the 
aforementioned factory methods), then that behaviour will be used when the constructor is bound.

Refer to the topic for detailed examples on how to use member binding.

* * *

# Next Steps

Assuming you've read the previous topics on [constructor injection](index.md) and [member binding](member-binding.md),
then that's it for constructor injection for now.

> [!NOTE] A separate topic will be added in the future regarding Generic Specialisation in the container, but that covers all target types, 
> not just the @Rezolver.Targets.GenericConstructorTarget - so that will go elsewhere.

Feel free to explore the table of contents or [head back to the main service registration overview](../service-registration.md) to explore 
more features of Rezolver.