# Automatic Lazy&lt;T&gt; Injection

This was added at the same time as the [Autofactory injection](autofactories.md), in 1.4, as the two sets of functionality go hand
in hand.

Indeed - to really get the most out of this functionality, you'll probably want to enable [automatic `Func<T>` injection](autofactories.md#automatic)
as well as enable [automatic `Lazy<T>` injection](#automatic).

Normally, in order to be able to inject `Lazy<T>` objects which actually perform just-in-time initialisation of their value, you would 
have to explicitly register a `Func<T>` as an object (or via some custom @Rezolver.ITarget implementation); and then either register 
the open type `Lazy<T>` or a closed version of it whose value type matches the return type of that delegate.

That's because of the [best-matched behaviour](constructor-injection/generics.md#best-match-vs-specific) that's used by default, which
selects the best constructor in a type that's available, based on the registrations in the container.

You could also [explicitly target the required constructor on the `Lazy<T>` type](constructor-injection/generics-manual-constructor.md), 
but the problem is that you're going to have to jump through a few hoops in order to be able to use the container's own registrations
to build the instead for the Lazy.

Automatic Lazy Injection is a feature which takes away all this pain, whilst also transparently allowing the container to
be used in the proouction of lazily-initialised instance.

## How to enable

Automatic 