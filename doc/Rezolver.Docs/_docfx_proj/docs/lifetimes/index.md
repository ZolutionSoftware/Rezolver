# Object Lifetimes

As with most IOC containers, Rezolver understands three primary lifetimes for the objects it produces, including 
support for automatic disposal of `IDisposable` objects when an @Rezolver.ContainerScope is used.

# Transient Objects

This is what's ultimately produced by a Rezolver @Rezolver.Container most of the time - a transient object is created when
requested from the container.  It's the same as using `new` to create an instance inside a function and then allowing 
it to go out of scope when that function returns.

[Read more about transient objects](transient.md).

# Singletons

Most developers will be familiar with the singleton pattern - an object created only once per process (often created lazily - i.e. on
demand) and which is frequently accessed via `static` accessors or similar.  Specifically, an application will usually enforce
rules at the code level to ensure that it's impossible to create more than one instance of the type.

Rezolver lets you change any registration into a singleton by simply wrapping an @Rezolver.ITarget inside a @Rezolver.Targets.SingletonTarget.

[Read more about singletons](singleton.md).

# Scoped Objects

Unsurprisingly, understanding scoped objects requires an understanding of scopes.

At its most fundamental level, a scope is simply a disposable 'bag' of objects which is created at the start of a particular process 
(e.g when a web request is received, or when a job is pulled off a job queue) and which is then disposed when that process completes.

Most IOC containers refer to these as *lifetime scopes* (*__note:__ 'container scopes' in Rezolver, implemented by <xref:Rezolver.ContainerScope>) 
and they are most often used (but not exclusively) to track objects that implement the `IDisposable` interface in order that they can be 
disposed without the developer explicitly having to do so themselves - often because the developer can never really be sure that she has 
*definitely* finished with that object.

But the presence of a scope at the point where an object is produced/obtained by the container does not *automatically* mean that that object 
is itself 'scoped'.  Indeed, transients and singletons can be used inside a scope in exactly the same way as outside.

Instead, a scoped object has a special lifetime behaviour such that it behaves like a singleton, except you get one instance 
*per scope* instead of one per-process.  A scoped object also *requires* a scope to be available in order for it to be resolved.

Scopes are also hierarchical - one scope can contain zero or more other child scopes whose scoped objects are independent of each other 
(and those of the parent) - but each child scope will be disposed when the parent scope is disposed.

Rezolver allows you to change any registration into a scoped object by wrapping an @Rezolver.ITarget inside a @Rezolver.Targets.ScopedTarget.

[Read more about container scopes](container-scopes.md) (particularly if you will be using `IDisposable` objects in your application).

[Read more about scoped objects](scoped.md).

