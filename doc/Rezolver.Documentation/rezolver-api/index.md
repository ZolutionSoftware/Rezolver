# Rezolver API reference

Looking for documentation on individual classes or methods in the Rezolver project?  You've come to the right
place!

Crucial types to look at are:

- **@Rezolver.TargetContainer** (implements <xref:Rezolver.ITargetContainer>): *Stores @Rezolver.ITarget instances, and 
is the main class you'll use for your 'registration' phase*
- **@Rezolver.Container** (implements <xref:Rezolver.IContainer>): *The standard, non-scoped, container you'll use in 
your composition root - you can create child scopes from this by calling its implementation of @Rezolver.IContainer.CreateLifetimeScope. 
This class requires an @Rezolver.ITargetContainer for its registrations*