# Automatic Lazy&lt;T&gt; Injection

This functionality builds on [Autofactory injection](autofactories.md) to provide the ability to inject @System.Lazy`1 instances where the container is used
to create the instance on-demand.

This is not something that's easily achievable via conventional registrations, because the constructors for `Lazy<T>` create an inherent ambiguity: 

[!code-csharp[AutoLazyExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoLazyExamples.cs#example0)]

If you try to do this, the `Resolve` call will throw an @System.Reflection.AmbiguousMatchException because Rezolver is unable to decide whether to bind to the 
<xref:System.Lazy%601.%23ctor(%600)?displayProperty=nameWithType> or <xref:System.Lazy%601.%23ctor(System.Func%7b%600%7d)?displayProperty=nameWithType>
constructor.

Whilst it is possible to [bind to a specific constructor on a generic type](/constructor-injection/generics-manual-constructor.md), the whole experience isn't
exactly friendly.

Instead, you can configure your container to automatically support `Lazy<T>` injection by getting it to automatically assume that you want it to be implemented
via the <xref:System.Lazy%601.%23ctor(System.Func%7b%600%7d)?displayProperty=nameWithType> constructor, with the `valueFactory` parameter being provided a 
delegate that's resolved from the container.

# Enabling

Enabling automatic lazy injection is similar to [how we enable automatic `Func<T>` injection](autofactories.md#automatic):

[!code-csharp[AutoLazyExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoLazyExamples.cs#example1a)]

With this in place, we can adjust our earlier attempt, and it'll work:

[!code-csharp[AutoLazyExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoLazyExamples.cs#example1b)]

## Simplifying

As you can see from that example, we still need to use the @Rezolver.RootTargetContainerExtensions.RegisterAutoFunc* method to make it possible
for the `Lazy<IFoo>` to be created (or we could register our own `Func<T>` to be used instead).

Instead, if we also [enable Automatic `Func<T>` injection](autofactories.md#automatic-functreturn-injection) using the @Rezolver.Options.EnableAutoFuncInjection 
option, then we can remove the need for the second registration:

[!code-csharp[AutoLazyExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoLazyExamples.cs#example2a)]

[!code-csharp[AutoLazyExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoLazyExamples.cs#example2b)]

# Interaction with scopes

As with Autofactories, Rezolver's automatically generated `Lazy<T>` instances will work with scopes correctly.  The scope that produces a Lazy instance will be
the scope to which its @System.Lazy`1.Value will be tied:

[!code-csharp[AutoLazyExamples.cs](../../../../test/Rezolver.Tests.Examples/AutoLazyExamples.cs#example3)]

> [!TIP]
> This is also true for [Singleton](lifetimes/singleton.md) objects, including disposable singletons (which are rooted to the 'outer' scope to prevent early
> disposal).