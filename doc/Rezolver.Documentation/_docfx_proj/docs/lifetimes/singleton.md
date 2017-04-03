# Singletons

Rezolver's implementation of singletons has three entry points:

- The @Rezolver.SingletonTargetContainerExtensions.RegisterSingle* overload, which offers a shortcut to registering a type via the 
@Rezolver.Target.ConstructorTarget or @Rezolver.Targets.GenericConstructorTarget (see the 
[construction injection topic](../constructor-injection/index.md) for more) as a singleton.
- The @Rezolver.Target.Singleton* extension method, which creates a new @Rezolver.Targets.SingletonTarget that wraps the target
on which it is called, converting it into a singleton.
- The @Rezolver.Targets.SingletonTarget.%23ctor(Rezolver.ITarget) constructor

The @Rezolver.Targets.SingletonTarget enforces a lock around its inner target so that its first result is cached and then returned
for each subsequent @Rezolver.IContainer.Resolve* operation.

> [!NOTE]
> At the moment, singletons apply across the whole `AppDomain`, but in the future (sometime in 1.2), they will apply to each container - 
> meaning that the same registration in two different containers would yield two different singletons.

## TODO: some examples