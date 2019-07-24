# Rezolver API reference

Looking for documentation on individual classes or methods in the Rezolver project?  You've come to the right
place!

Crucial types to look at are:

- **@Rezolver.Container** : The standard, non-scoped, container.  You can create child scopes from this by calling its
@Rezolver.Container.CreateScope* method, which returns instances of @Rezolver.ContainerScope (from which you can resolve instances).
This class uses an @Rezolver.IRootTargetContainer for its registrations, which you can supply on construction if required, or you can
add registrations to it through its own implementation of that interface - all the methods and properties of which are implemented by
proxying the inner root target container.
- **@Rezolver.TargetContainer** (implements <xref:Rezolver.IRootTargetContainer>): Stores @Rezolver.ITarget instances, and 
is the main class you'll use for your 'registration' phase if not registering into your container directly.
- **@Rezolver.ScopedContainer** - A disposable version of @Rezolver.Container that also acts as a 'root' scope.  If you using singletons
which are also disposable, then it's important that you have a long-running scope that sits at the root of your application which
keeps those objects alive.  Using this as a your root container is the easiest way to achieve that.