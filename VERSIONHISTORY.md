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
 - Added `Rezolver.Options` namespace
   - Given an `ITargetContainer`, you can use extension methods to get and set options
which alter behaviour of the container.  For example, if you don't want your container
to accept multiple registrations for any type, or for a particular type or group of generics,
you can use `.SetOption<AllowMultiple[, Type]>(false)`.
 - Options currently available:
   - `AllowMultiple`: (As above)
   - `FetchAllMatchingGenerics`: Controls the underlying behaviour of the `GenericTargetContainer`
class' `FetchAll` implementation works which, when using automatic `IEnumerable<>` resolving,
also controls which objects are actually included in an enumerable.
   - `UseGlobalsForUnsetServiceOptions`: Controls how the container resolves options for
specific service types.  By default, if an option is not set for the given service type, or
matching open generic for that service type, then it'll look for a globally-defined option - i.e.
one which is not Set against a specific service type.

## 1.2.current and before

(Copy releases information from those releases)

## < 1.1

Initial development - numerous package releases on nuget etc.

