# Using the Fluent API to customise member bindings

All of the [standard binding behaviours](index.md#standard-behaviours), except the 
@Rezolver.MemberBindingBehaviour.BindNone behaviour of course, bind groups of members based on blanket
rules - i.e. either or both publicly writeable properties and fields.

But what if you want to register type for member injection but only inject *certain* members?

Rezolver has another behaviour implementation for this - the @Rezolver.BindSpecificMembersBehaviour class - which 
accepts either a series of members to be auto-injected, or a set of @Rezolver.MemberBinding instances containing 
the members to be bound, along with the bindings that are to be applied to them.

So, if you had a type with two properties, but you only wanted one of them to be bound, you just pass
that property's @System.Reflection.PropertyInfo to the behaviour instance when you build it, and away you go.

The problem, however, is that all that reflection can be very untidy, slow, and ultimately not terribly
quick or efficient.

This is where the fluent member bindings API comes in.