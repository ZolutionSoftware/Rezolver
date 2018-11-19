# Rezolver API reference

Looking for documentation on individual classes or methods in the Rezolver project?  You've come to the right
place!

Crucial types to look at are:

- **@Rezolver.TargetContainer** (implements <xref:Rezolver.ITargetContainer>): Stores @Rezolver.ITarget instances, and 
is the main class you'll use for your 'registration' phase.

- **@Rezolver.Container** (implements <xref:Rezolver.IContainer>): The standard, non-scoped, container you'll use in 
your composition root - you can create child scopes from this by calling its implementation of @Rezolver.IScopeFactory.CreateScope, 
which returns instances of @Rezolver.IContainerScope.  This class uses an @Rezolver.ITargetContainer for its registrations,
which you can supply construction if required.

- **@Rezolver.ScopedContainer** - A disposable @Rezolver.IContainer that also acts as a 'root' scope.