# Factory expressions

If you've read the [factory delegate](delegates.md) documentation, then you'll know that Rezolver can bind a delegate (with an unlimited number of non-`ref`, non-`out`
parameters) as a factory for a given type registration.

Rezolver can do the same with expression trees (derived from the <xref:System.Linq.Expressions.Expression> class) and has one or two additional tricks up its sleeve when doing
so.

To register expessions you can use one of the many @Rezolver.TargetContainerExtensions.RegisterExpression* extension methods for @Rezolver.ITargetContainer.

To create expression targets you can either:

- Manually create an instance of @Rezolver.Targets.ExpressionTarget through its constructor
- Use the @Rezolver.Target.ForExpression* overload, which, like the delegate helper function @Rezolver.Target.ForDelegate*, provides specialisations for lambda expressions whose
signatures conform to one of many `System.Func<>` generic delegate types.

Whilst using expressions might appear to be fundamentally the same as using delegates, there is a subtle difference:  A @Rezolver.Targets.DelegateTarget requires 
an entire function body, whilst an @Rezolver.Targets.ExpressionTarget supports any expression, from expression 'fragments' like @System.Linq.Expressions.ConstantExpression 
right up to the delegate-like @System.Linq.Expressions.LambdaExpression.

# Expression 'fragments'

As we've just mentioned, an expression fragment is one of the small, specialised, expression types from the @System.Linq.Expressions namespace which wrap a fundamental 
language expression, such as constants or method calls or whatever.  The @Rezolver.Targets.ExpressionTarget supports these through its 
[constructors](xref:Rezolver.Targets.ExpressionTarget.%23ctor*).

When creating an @Rezolver.Targets.ExpressionTarget this way, its @Rezolver.ITarget.DeclaredType (and therefore the default type under which it will be registered, unless
overridden at registration time) will, by default, be set to the @System.Linq.Expressions.Expression.Type of the expression you pass to the constructor, unless you pass a 
non-null type explicitly as the second argument.

## Using `ConstantExpression`

In this example, we'll bake a @System.Linq.Expressions.ConstantExpression into an @Rezolver.Targets.ExpressionTarget, which will then be used to provide a `string`:

[!code-csharp[ExpressionExamples.cs](../../../../test/Rezolver.Tests.Examples/ExpressionExamples.cs#example1)]

## Using `MethodCallExpression`

Here, we have an instance method declared in our test, which returns the value of a local field:

[!code-csharp[ExpressionExamples.cs](../../../../test/Rezolver.Tests.Examples/ExpressionExamples.cs#example2a)]

To instruct Rezolver to execute this method on `this` instance whenever a string is resolved, we simply do this:

[!code-csharp[ExpressionExamples.cs](../../../../test/Rezolver.Tests.Examples/ExpressionExamples.cs#example2b)]

> [!NOTE]
> Obviously, this can also easily be implemented as a @Rezolver.Targets.DelegateTarget - but the same reasons for why you might use expressions over delegates in
> any application (i.e. the ability to analyse and rewrite code or dynamically compose logic) hold for targets in your container.  The point of these examples is to show
> that Rezolver will happily deal with any expression you throw at it, *not* that you should use expressions where a delegate would be a better choice.

# Lambda Expressions

Lambda expressions can also be used to create or register an @Rezolver.Targets.ExpressionTarget, meaning you can take advantage of the compiler's translation of code 
into expression trees to simplify their construction.

> [!NOTE]
> When constructing an @Rezolver.Targets.ExpressionTarget from a lambda expression, the @Rezolver.ITarget.DeclaredType of the target will be set to the
> @System.Linq.Expressions.Expression.Type of the @System.Linq.Expressions.LambdaExpression.Body of the lambda, unless overridden by a type explicitly provided on construction.

Ultimately, Rezolver doesn't really care if you pass a lambda or a fragment like those in the previous section - the same process is followed - but one additional feature
that a lambda provides is parameters.

## Injecting Arguments

As with [factory delegates](delegates.md), Rezolver provides automatic argument injection for lambda expressions - so, taking the `MethodCallExpression` example from
above, we could instead do this:

[!code-csharp[ExpressionExamples.cs](../../../../test/Rezolver.Tests.Examples/ExpressionExamples.cs#example3)]

> [!NOTE]
> Look past the fact that the object being injected is the same instance on which the test is executed, it's the principle that matters!

## Injecting the `ResolveContext`

Just as with the [`DelegateTarget` example](delegates.md#injecting-rezolverresolvecontext), you can also inject the @Rezolver.ResolveContext into your expression in order
to perform late-bound service location within your expression.  As the comment at the start of the test states - this example is functionally identical to the `DelegateTarget`
example - but because of the limitations of the C# compiler and its ability to translate code into expression trees, the main expression is written as a series of stacked 
conditional expressions so that the whole thing is a single expression.

[!code-csharp[ExpressionExamples.cs](../../../../test/Rezolver.Tests.Examples/ExpressionExamples.cs#example4)]

> [!NOTE]
> Remember: any expression tree built by the compiler can be built 'by hand' through the factory methods in @System.Linq.Expressions.Expression, meaning that 
> you could also provide an expression in which the conditional statements where built dynamically (or, more likely, a dynamically built 
> <xref:System.Linq.Expressions.SwitchExpression>) and it would still work.

# Resolving without an `ResolveContext`

These advanced examples with lambda expressions show how you can inject an argument or perform late-bound service location within your expression body.  Sometimes, however,
you might not want to, or be able to, provide a parameterised lambda expression, or you might be passing an expression fragment which cannot accept
injected arguments.

For this purpose, Rezolver provides the @Rezolver.ExpressionFunctions static class.  If the compiler sees a @System.Linq.Expressions.MethodCallExpression bound to one of 
the @Rezolver.ExpressionFunctions.Resolve* static functions of this class, it will be converted into a call to the appropriate @Rezolver.ResolveContext.Resolve* overload of the 
@Rezolver.ResolveContext that is in scope when the container executes the code compiled from the expression.

> [!WARNING]
> These functions only work inside expressions - any attempt to execute them outside of an expression passed to a @Rezolver.Targets.ExpressionTarget will result in 
> a @System.NotImplementedException being thrown.

[!code-csharp[ExpressionExamples.cs](../../../../test/Rezolver.Tests.Examples/ExpressionExamples.cs#example5)]

> [!NOTE]
> The `RequiresMyService` and `MyService` types can be seen in the [construction injection documentation](constructor-injection/index.md#example---injected-class).

Admittedly, this way of dynamically resolving services inside an expression requires some heavy lifting (with reflection in particular) when compared with writing the lambda 
by hand, but it's not intended to be a direct alternative to doing that.  Using this approach is for those times when you simply can't alter how you build the expression
you want to register.

You can, of course, also use the `ExpressionFunctions` static class inside a parameterless lambda to achieve exactly the same result:

[!code-csharp[ExpressionExamples.cs](../../../../test/Rezolver.Tests.Examples/ExpressionExamples.cs#example6)]

# Next steps

- If you haven't already, then you should probably look at the aforementioned [factory delegate documentation](delegates.md).
- The next main topic after this covers [decorators](decorators.md).