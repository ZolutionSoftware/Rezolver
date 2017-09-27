# Decorating with delegates

If you've read about Rezolver's support for the [decorator pattern](../decorators.md) (including
[collection decoration](collections.md)) then you'll know that
you have most of the functionality you could ever need to implement the decorator design pattern.

There are some types, however, whose registrations cannot be decorated by a constructor-injected instance - 
in particular:
- Value types (since you cannot inherit from them)
- Arrays
- Delegate types

Plus, there are some types - e.g. `IEnumerable<T>` - where a more natural method of decoration is method-based
instead of class-based.

For this, Rezolver provides the ability to register delegates that decorate a specific concrete (i.e. 
*non*-generic) type.  The delegate will be executed, passing the undecorated result as an argument, and 
its return value used in place of the original result.

# Decorating an `int`

Our first example simply shows how decorate all `int` objects produced by the container so that they are 
doubled.  It's not a particularly real-world example - but it's a simple way to show how this works.

[!code-csharp[DecoratorExamples.cs](../../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example10)]

> [!TIP]
> The @Rezolver.DecoratorTargetContainerExtensions.RegisterDecorator* is overloaded for the delegate types 
> @System.Func`2 and @System.Delegate - the second of which allows any delegate to be registered as a 
> decorator, subject to parameter and return type checks.
> ***
> Additional generic overloads will be added in the future to simplify the registration of delegates of any
> arity, e.g. @System.Func`5, but for now you just have to create them directly - e.g:
> 
> `new Func<A, B, C, D>((a, b, c, d) => a.foo(b, c, d))`;

## Integration with `IEnumerable<T>`

[As shown in the enumerables documentation](../enumerables.md#decorators-and-enumerables), enumerables of a 
given type will honour any and all decorators which are registered for that type when they are created, so if we
extend the above example and produce an `IEnumerable<int>` from multiple `int` registrations, then each one
will be doubled in the enumerable that's created:

[!code-csharp[DecoratorExamples.cs](../../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example11)]

# Decorating a delegate

If you are injecting delegates in your application, then you can stack decorator delegates to implement the 
chain of responsibility pattern.

This example implements a number classifier as a single delegate (`Action<int>`) composed of many others.
As numbers are passed to the delegate, they are placed into one of four sets.

One of the decorator delegates accepts both the original delegate *and* an extra dependency (`IPrimeChecker`),
thus showing how decorator delegates support the same argument injection that's supported by Rezolver's standard
[delegate registrations](../delegates.md). Here's the code for `IPrimeChecker` and a basic implementation:

[!code-csharp[IPrimeChecker.cs](../../../../../test/Rezolver.Tests.Examples/types/IPrimeChecker.cs#example)]

And here's the test:

[!code-csharp[DecoratorExamples.cs](../../../../../test/Rezolver.Tests.Examples/DecoratorExamples.cs#example12)]