# Transient Objects

Most registrations performed on an @Rezolver.ITargetContainer will result in transient 
objects being created when the @Rezolver.IContainer.Resolve* method of an @Rezolver.IContainer 
is called.  Ultimately, the transience of an object is determined by the @Rezolver.ITarget that
is registered against the type that's requested from the container.

Some of these targets create inherently transient objects, whilst some inherit or expressly control the
lifetimee of other targets.

The following table summarises this:

@Rezolver.ITarget implementation | Transient? | Notes
--- | --- | ---
@Rezolver.Targets.ChangeTypeTarget | *Inherited* | Target doesn't create anything, only changes the type of another target's result, therefore it inherits that target's lifetime.
@Rezolver.Targets.ConstructorTarget | **Yes** |
<sup>*</sup>@Rezolver.Targets.DecoratorTarget | **Yes** | It's not currently possible to directly control the lifetime of a decorator - only the object it decorates.
@Rezolver.Targets.DefaultTarget | No | Default values are cached per-type, therefore even value type defaults are technically singletons.
@Rezolver.Targets.DelegateTarget | **Depends on the delegate** | The delegate is *always* executed, so the transience of the object it returns depends on the delegate's logic.
@Rezolver.Targets.ExpressionTarget | **Depends on the expression** | Like @Rezolver.Targets.DelegateTarget, the expression itself determines the lifetime of the object produced.
@Rezolver.Targets.GenericConstructorTarget | **Yes** | Same as @Rezolver.Targets.ConstructorTarget.
@Rezolver.Targets.ListTarget | **Yes** |
@Rezolver.Targets.ObjectTarget | No |
<sup>*</sup>@Rezolver.Targets.OptionalParameterTarget | No | Only used to bind optional constructor parameters to their default values
@Rezolver.Targets.ResolvedTarget | **Depends** | This target represents an explicit instruction to resolve an object/value from the container - therefore the lifetime of the object produced depends on the lifetime of the registration which ultimately gets resolved.
@Rezolver.Targets.ScopedTarget | No | This target is responsible for Rezolver's implementation of [scoped objects](scoped.md).
@Rezolver.Targets.SingletonTarget | No | This target is responsible for Rezolver's implementation of [singleton objects](singleton.md).
<sup>*</sup>@Rezolver.Targets.UnscopedTarget | *Inherited* | This target is used to strip explicit or implicit scoping behaviour from another target.

<sup>*</sup> *Denotes a target which is used indirectly by other registrations and not generally used directly by applications* 

There's no need for any further examples for transient objects, as most of the examples throughout this documentation
use the targets which are marked as transient in the above table.

* * *

# Next steps

- Head over to the [singletons topic](singleton.md) to read about how to use the @Rezolver.Targets.SingletonTarget.