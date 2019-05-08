# Per-registration Member Injection

When creating an instance of the main @Rezolver.ITarget implementations responsible for constructor injection -
i.e. @Rezolver.Targets.ConstructorTarget and @Rezolver.Targets.GenericConstructorTarget - you also have the ability
to provide an @Rezolver.IMemberBindingBehaviour that you want to be used to bind the members on each new instance.

You will rarely use these class' constructors directly.  Instead, most of the time, you will be using 
factory methods like @Rezolver.Target.ForType*, or the @Rezolver.TargetContainerExtensions.RegisterType*
extension methods.

Such explicitly provided member binding behaviours will *override* any which might otherwise be applied 
[via options](options.md).

As with the documentation on that topic our example also uses the `Has2InjectableMembers` demo type:

[!code-csharp[Has2InjectableMembers.cs](../../../../../test/Rezolver.Tests.Examples/Types/Has2InjectableMembers.cs#example)]

As in that topic, we make sure to register all three types, making sure that when we register our `Has2InjectableMembers`
type, that we pass the binding behaviour:

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example1)]

> [!WARNING]
> When using the @Rezolver.MemberBindingBehaviour.BindAll behaviour, if one of the services required by any of the writable 
> properties or fields is not present in the container, then the resolve operation will fail.
> 
> The behaviour is *deliberately* primitive - because unlike
> with constructors, where it's reasonable to assume all parameters are to be injected, there are no simple rules
> that can always be applied to an object to determine which properties and/or fields should be auto-injected.

You don't need to be creating a registration in order to setup member binding in this way, either.  In this example,
we're creating the target (which will be a @Rezolver.Targets.ConstructorTarget for the `Has2InjectableMembers` type)
and then registering it separately through the @Rezolver.ITargetContainer.Register* method

[!code-csharp[MemberBindingExamples.cs](../../../../../test/Rezolver.Tests.Examples/MemberBindingExamples.cs#example1b)]

* * *

You should now know how to [configure member injection via container options](options.md) as well as on a
per-registration or per-target basis, as shown here.

Now you might want to take a look at how [collection member injection works](collections.md).