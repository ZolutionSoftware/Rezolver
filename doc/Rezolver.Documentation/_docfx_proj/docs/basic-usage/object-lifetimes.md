# Object lifetimes in Rezolver

> [!TIP]
> Read [Creating and using a container](create-and-use-a-container.md) if you haven't already done so.

As with any IOC container, Rezolver understands the concept of object lifetimes, which are implemented
as different @Rezolver.ITarget implementations that can be registered in an @Rezolver.ITargetContainer.

## What is a lifetime?

No, this is not a philosophical question ;)

In a crude sense, an object lifetime describes when a new object is created and whether a previously 
created object is subsequently returned from a container's @Rezolver.IContainer.Resolve* method.

> [!NOTE]
> In practise, it's not that simple - because an IOC container's understanding of a lifetime does not
> strictly map to whether a new instance is actually created at all.
> 
> It's more accurate to say that the lifetime determines **when** the
> ***action*** that's been associated with a given service type is actually ***executed***.

Rezolver understands three main lifetimes, which should be familiar to anyone who's used an IOC
container before:

### Transient

In *general*, a transient object is simply one which is created when needed; and thrown 
away when it's fulfilled its purpose.

This typically translates to calling the `new` operator with the constructor of a particular type, 
when you want an object, and then allowing that object to go out of scope, as in this snippet:

```cs
// Factory method always creates a new instance
static MyObject CreateObject()
{
    return new MyObject();
}

static MyObject _transient = CreateObject();

void Test()
{
    var transient1 = CreateObject();
    var transient2 = CreateObject();
    //note - using XUnit nomenclature here
    Assert.NotSame(transient1, transient2);
    Assert.NotSame(_transient, transient1);
    Assert.NotSame(_transient, transient2);
}

```

This idea of transience is encompassed by the following targets in the @Rezolver.Targets namespace:

- @Rezolver.Targets.ConstructorTarget
- @Rezolver.Targets.DecoratorTarget
- @Rezolver.Targets.DelegateTarget *(see below)*
- @Rezolver.Targets.ExpressionTarget *(see below)*
- @Rezolver.Targets.GenericConstructorTarget
- @Rezolver.Targets.ListTarget

Remember that a target in Rezolver is an instruction to perform an *action* when the associated service
type is resolved by the container.  In the case of the above list, for all but two we can *definitely* say that 
a new object will be created if that target is registered against a type that's requested.

So, the @Rezolver.Targets.ConstructorTarget will always result in a type's constructor being called; the 
@Rezolver.Targets.ListTarget will always result in a new `Array` or `List<T>` being created, and so on.

In the case of the @Rezolver.Targets.DelegateTarget and @Rezolver.Targets.ExpressionTarget targets, however,
all we can say about those is that the ***action*** they represent will be executed *every time* the container 
resolves them.

That's the same as if we were to change the definition of `CreateObject` in our earlier code example as 
follows:

```cs
private static IMyObjectService _service = new MyObjectService();

static MyObject CreateObject()
{
    return _service.GetObject();
}
```

The point here being that we can no longer **guarantee** the transience of the object that is produced by the
`CreateObject` method because we no longer know how the service is producing that object.  All we can definitely 
say is that we *definitely* execute the service's `GetObject` method every time we call `CreateObject`.

We will go into more depth about using delegates and expressions (and indeed custom targets) as we delve deeper
into the Rezolver framework - but for now it's important to know that some targets produce inherently transient
results, whereas some *might* do, but they equally might ***not***.

### Singleton

Every developer should know what a singleton is.  Okay - any *reasonably experienced* developer should know what
a singleton is!

In short, a singleton is exactly as the name suggests: a type for which there is **guaranteed** to be only one 
instance.

Here's a simple implementation (note: please don't take this as the best, or *only*, way to implement one - it's
just an example!):

```cs
public class MySingleton
{
    public MySingleton Instance { get; } = new MySingleton();

    private MySingleton()
    {

    }
}
```
An application wishing to use the `MySingleton` object must do so through the `Instance` property, which is
the only instance of that type for the entire `AppDomain`.

Equally, if the singleton object implements an interface or base, then it can be passed to code which requires
an instance of that interface or base without knowing that the underlying object itself is always the same 
reference.

> [!NOTE]
> Different IOC containers have differing understandings of what a singleton really is.
> In some cases it's one instance per-AppDomain (as with the above code snippet) and in other cases it's
> one instance per-container.
> 
> In yet more cases, it's configurable or, indeed, extensible.
> 
> Rezolver currently supports one instance per-AppDomain, but in future it will also support one per-container 
> and possibly more besides.

### Scoped

Scoping has two flavours, and also often relates specifically to objects which implement the `IDisposable` interface
from the .Net framework (although it doesn't have to).  In order for the idea of a scoped object to make any sense,
however, you need a scope within which the object will exist.

A scope can be thought of as being a 'bag' for objects which the application wants to create and keep alive for a specific
period of time and, when it no longer needs those objects, it disposes of the scope.

The reason why this pattern is usually only applied to disposable objects is because with transient objects you don't
need to worry about telling the system that you're done with an object - you simply allow it to go out of scope and
eventually the garbage collector will reclaim the memory that object occupied.

That said, there are scenarios why you might wish to explicitly restrict a service instance to one-per-scope, so that's
why scoped objects do not have to be disposable.

One feature which is consistent, however, is that the scope **itself** is disposable.

#### Implicit scoping

Implicit scoping applies only to `IDisposable` objects and affects when the object is disposed, not when it's 
created.

A simple demonstration of this, if you were handwriting it, would be as follows (*please note the implementation
of `IDisposable` shown here is ***not*** recommended!*):

```cs
public class Scope : IDisposable
{
    private List<IDisposable> _scopeObjects = new List<IDisposable>();
    public void Dispose()
    {
        //dispose all our tracked disposables
        foreach(var obj in _scopeObjects)
        {
            obj.Dispose();
        }
    }

    public T CreateObject<T>()
    {
        //this bit is merely to take the place of a generic 
        //factory/service/IOC container which knows how to build things.  
        T result;
        if(typeof(T) == typeof(MyDisposable))
            result = (T)(object)new MyDisposable();
        else
            throw new InvalidOperationException($"Don't know how to create { typeof(T) }");

        //if the object is disposable - track it
        if(result is IDisposable)
            _scopeObjects.Add((IDisposable)result);

        return result;
    }
}

public class MyDisposable : IDisposable
{
    public bool Disposed { get; private set; }
    public void Dispose()
    {
        Disposed = true;
    }
}

public class XUnitTest
{
    [Fact]
    public void ShouldDispose()
    {
        MyDisposable obj;
        using(var scope = new Scope())
        {
            obj = scope.CreateObject<MyDisposable>();
        }
        Assert.True(obj.Disposed);
    }
}
```

To reiterate - nothing in the above is recommended practise at all - it's entirely contrived just to demonstrate what
a scope does.