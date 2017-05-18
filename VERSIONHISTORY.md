# Rezolver Version History

## In progress (1.2.vnext)

 - Renamed `IContainer.FetchCompiled` to `IContainer.GetCompiledTarget`
   - Changed `ContainerBase`'s implementation non-virtual
 - Moved `ContainerBase.MissingCompiledTarget` into the `Rezolver.Compilation` namespace
   - Also renamed to `Rezolver.Compilation.UnresolvedTypeCompiledTarget`
   - Did away with the concurrent dictionary creation method - now you just 
create a new instance as there's no need to enforce a one-instance-per-type 
rule
 - Added `Rezolver.Compilation.DelegatingCompiledTarget` to allow factory functions
to be used directly as implementations of `ICompiledTarget`
 - Added `IDirectTarget` - to be used on an `ITarget` which ultimately wraps 
a non-context-aware method or instance.
    - `ObjectTarget` now implements this too
    - As does `NullaryDelegateTarget` - a special `DelegateTarget` that 
is specialised for parameterless delegates which return something.

### Notes on `IDirectTarget`

The idea behind this is to simplify the resolving of objects directly from
`ITargetContainer` without compiling.  A new `IContainer` implementation will
be created which only works with `IDirectTarget` targets and targets which
are of the type for which they are registered.

The root `TargetContainer` instance will create an instance of this class 
(non-caching) which it then uses to service its own container service needs.

Interestingly - the Container can also do the same.

## 1.2.current and before

(Copy releases information from those releases)

## < 1.1

Initial development - numerous package releases on nuget etc.

