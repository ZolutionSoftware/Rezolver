# Specific Constructors for Open Generics

As described in the [main open generic constructor injection topic](generics.md), Rezolver is able to bind
dynamically to the best-matched constructor for a particular closed generic when built from an open generic
registration.

However, it's also possible (starting from 1.3.2) to instruct Rezolver to use a *specific* constructor when
creating an instance from an open generic registration.

Use cases for this are, of course, pretty niche - especially if your own classes typically stick to the standard
pattern of only having a single constructor (as is enforced by some IOC containers).  But the reality is there will
always be types which legitimately have multiple constructors (take a lot of .Net collection types, for example)
which you might also want to create via constructor injection.

## When you might need to use this

The most likely scenario for needing to be able to provide a specific constructor to be used when creating an
instance from an open generic is to disambiguate between constructors when a generic type argument might 
can cause a conflict with the concrete type of an argument on one or more constructors.

For example, consider the `List<T>` type, which has the following constructors:

- `List<T>()`
- `List<T>(IEnumerable<T>)`
- `List<T>(int)`

> [!TIP]
> `List<T>` is a type that, by default, Rezolver [can build automatically for you](../arrays-lists-collections/lists.md)
> by extending its own support for [enumerables](../enumerables.md).
> ***
> In fact, the scenario described here is taken specifically from how Rezolver handles the `List<T>` type.

Now, if you were to explicitly register the `List<T>` type for constructor injection (after disabling Rezolver's
own support for it of course), Rezolver will happily bind to the `IEnumerable<T>` constructor for all types of `T`
except one: `int`.

When you request a `List<int>`, Rezolver immediately hits a problem - unless you've not got any `int` registrations
(which presumably you would have if you're requesting a list of `int`!) - because it sees *two* constructors that 
could be satisfied.  It doesn't *know* that you could only ever want the `IEnumerable<T>` one - how could it? - so
instead of getting a shiny new list to play with, you get an exception.

So how do we get around it?

What we need to be able to do, of course, is to instruct Rezolver to use the `IEnumerable<T>` constructor for all 
`T`.

Thankfully, there are a couple of ways to do that.

## The `RegisterGenericConstructor` overload

When registering targets into the @Rezolver.IRootTargetContainer, you can use one of these two methods:

- @Rezolver.TargetContainerRegistrationExtensions.RegisterGenericConstructor``1(Rezolver.ITargetContainer,System.Linq.Expressions.Expression{System.Func{``0}},Rezolver.IMemberBindingBehaviour)
- @Rezolver.TargetContainerRegistrationExtensions.RegisterGenericConstructor``2(Rezolver.ITargetContainer,System.Linq.Expressions.Expression{System.Func{``0}},Rezolver.IMemberBindingBehaviour)

Both functions expect an expression in which you provide a 'model' constructor call to the generic type whose
constructor you want to bind.  The actual constructor arguments you pass are not important - only that the
expression binds to the correct constructor.

Now, clearly, if you try to do this over the *open* generic, your code won't compile - e.g:

```cs
class MyGeneric<T, U> { 
    public MyGeneric(T t) { }
    public MyGeneric(T t, U u) { }
    public MyGeneric(T t, U u, string s) { }
}

// ...

targets.RegisterGenericConstructor(() => new MyGeneric<,>(null, null));
```

Because using the `MyGeneric<>` type in this way requires the type arguments to be provided.

Instead, you stub out the type arguments with any types you want, and Rezolver will match the constructor with its
definition on the open generic type:

```cs
targets.RegisterGenericConstructor(() => new MyGeneric<object, object>(null, null))
```

This will create a registration in the container which, for any closed `MyGeneric<,>` will explicitly bind to 
the two-parameter constructor, even if registrations exist which satisfy the `s` parameter on the 3-parameter 
version.

### Example 1

Here's a tested example with a type which extends the `List<T>` scenario:

[!code-csharp[HasAmbiguousGenericCtor.cs](../../../../../test/Rezolver.Tests.Examples/Types/HasAmbiguousGenericCtor.cs#example)]

In this case, if we want to resolve an `HasAmbiguousGenericCtor<string>`, Rezolver will, by default, use the
[best-match rule](index.md#best-match-examples), meaning that the last `string` we registered will be used both
as the last item in the enumerable but also as the argument for the single `string` parameter.  

But that's not a problem with the `RegisterGenericConstructor` function:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example200)]

### Example 2 - Different 'service' type

When we register a type for constructor injection normally, we can also tell the container to create an instance
of one type when another type is requested - e.g:

```cs
// construct a Foo whenever we want an IFoo
container.RegisterType<Foo, IFoo>();

// construct a Foo<T> whenever we want an IFoo<T>
container.RegisterType(typeof(Foo<>), typeof(IFoo<>));
```

We can do the same with the other `RegisterGenericConstructor` overload which accepts two type arguments.  In 
this case, you'll have noticed that our `HasAmbiguousGenericCtor<T>` generic also implements `IGeneric<T>` - so
let's bind a specific constructor *and* register it to be used for a generic service type:

[!code-csharp[ConstructorExamples.cs](../../../../../test/Rezolver.Tests.Examples/ConstructorExamples.cs#example201)]

As the comment says, it's somewhat unfortunate that we have to use specify those generic arguments - just remember
that the `RegisterGenericConstructor` method is specifically looking to bind an open generic.

## Alternatives

The `RegisterGenericConstructor` overload is merely a wrapper for one of the many overloads
of the @Rezolver.Target.ForConstructor* factory method, which creates targets from a @System.Reflection.ConstructorInfo.

This overload will create a @Rezolver.Targets.GenericConstructorTarget if the `ConstructorInfo` passed belongs to 
an open generic type.  After creating the target, you can then register it directly into the container using the
normal @Rezolver.ITargetContainer.Register* function or one of its overloads.

The same is also true of the @Rezolver.TargetContainerRegistrationExtensions.RegisterConstructor* overload.

Be aware, though - in all cases, it's not possible to provide named constructor argument bindings, either by dictionary
or as an object, because of the inherent difficulty of providing a single target for one or more arguments whose type
comes from a generic type argument.