# Rezolver Benchmarks

Benchmarking an IOC container obviously has pitfalls.  In a web environment, for example, 
it's highly unlikely you'll hit the kind of traffic levels required for your choice of container
to have a noticeable impact on speed.

But, that said, speed is still important - especially with something as fundamental as an object
which is taking control of how you create *your* objects!  In any case, if you're reading this 
then you're a developer - which means you're probably interested in how fast Rezolver stacks 
up against your *previous* favourite container ;)

## Methodology

Over on GitHub [Daniel Palme](https://github.com/DanielPalme) has, for a while now, been 
maintaining a benchmarking suite for IOC containers - called 
[IOCPerformance](https://github.com/DanielPalme/IOCPerformance).  In addition to our own 
performance analysis, this program offers an easy way to stress test the Rezolver containers
both on their own, but also against other popular containers. 

The tests range from the basics - singletons, transients, objects which require a mixture of
these, complex objects which require lots of nested dependencies
 
Excluding Rezolver and the 'No' container, which the baseline, there are 31 containers now
being stress-tested by this application, with the list growing.

So, [we've forked this project](https://github.com/ZolutionSoftware/IOCPerformance) and 
added an adapter for Rezolver.  We then ran it on an
Intel(R) Xeon(R) CPU E3-1230 v3 @ 3.30GHz (speed step/power manage was switched off to ensure
the processor ran at its maximum clock).

# Results

> [!NOTE]
> A couple of results which you'll find on the [IOCPerformance](https://github.com/DanielPalme/IOCPerformance)
> pagr are left out of the individual results because Rezolver doesn't yet support them (specifically
> Interception and Conditional services).  Implementations are planned.

## Singleton

Singleton service via @Rezolver.Targets.SingletonTarget

![Singleton results - all][01-Singleton]

## Transient

Transient service using @Rezolver.Targets.ConstructorTarget

![Transient results - all][02-Transient]

## Combined

Singleton and transient mixed together.

![Combined results - all][03-Combined]

## Complex

Deep dependency graph, mix of transient and singleton

![][04-Complex]

## Properties

Constructor injection with property injection  - @Rezolver.Targets.ConstructorTarget in
conjunction with an @Rezolver.IMemberBindingBehaviour 

![][05-Property]

## Generics

Using @Rezolver.Targets.GenericConstructorTarget to register `Foo<>` and then request instances
of `Foo<Bar>`, `Foo<Baz>`, `Foo<Bat>` etc.

![][06-Generics]

## Enumerable

Registering multiple services for `T` and then resolving an `IEnumerable<T>`

![][07-IEnumerable]

## Child Containers

Create a new container from an existing one, register new services in it and resolve.

> [!NOTE]
> The poor performance of Rezolver here is caused by the fact that there's an overhead on the
> first @Rezolver.IContainer.Resolve* call to a container, as that's when it compiles the 
> associated @Rezolver.ICompiledTarget for that service.  Because of the way this benchmark
> is performed, it means that Rezolver spends most of its time dynamically compiling delegates.
>
> A solution is in the pipeline for this, which will be implemented once we've implemented another
> compiler that's based entirely on reflection and late-bound delegates instead of dynamically 
> constructed expression trees.  When this is done, the performance of child containers will 
> improve drastically.

![][09-Child-Container]

## Registering/Preparing the Container

Creating a container, registering services and optionally 'baking' the container (not required
by Rezolver).

![][11-Prepare-And-Register]

## Registering/Preparing the Container (+ Resolve)

Same as above, except a resolve operation is then subsequently fired at the container.  Again,
Rezolver suffers for the same reason as with the child containers.  When performance for that
has been improved, then this one will also.

![][12-Prepare-And-Register-And-Simple-Resolve]

## 'Advanced' compound results

All the so-called 'advanced' operations' times summed and graphed (note - if a container doesn't
even support a particular feature, then it'll be higher up these graphs).

### Fastest

![][Overview_Advanced_Fast]

### Slowest

![][Overview_Advanced_Slow]

### Notes

Rezolver doesn't make it into the 'Fast' graph because of the aforementioned first-call overhead
when resolving a service of a given type for the very first time.  This will change.


## 'Basic' compound results

All the so-called 'basic' operations' times summed and graphed (again - if a container doesn't
even support a particular feature, then it'll be higher up these graphs).

### Fastest

![][Overview_Basic_Fast]

### Slowest

![][Overview_Basic_Slow]

### Notes

Top 10!

## Register/Prepare (+ Resolve operation)

### Fasters

![][Overview_Prepare_Fast]

### Slowest

![][Overview_Prepare_Slow]


[01-Singleton]:../images/benchmark-results/01-Singleton.png
[02-Transient]:../images/benchmark-results/02-Transient.png  
[03-Combined]:../images/benchmark-results/03-Combined.png  
[04-Complex]:../images/benchmark-results/04-Complex.png  
[05-Property]:../images/benchmark-results/05-Property.png  
[06-Generics]:../images/benchmark-results/06-Generics.png  
[07-IEnumerable]:../images/benchmark-results/07-IEnumerable.png  
[09-Child-Container]:../images/benchmark-results/09-Child-Container.png
[11-Prepare-And-Register]:../images/benchmark-results/11-Prepare-And-Register.png  
[12-Prepare-And-Register-And-Simple-Resolve]:../images/benchmark-results/12-Prepare-And-Register-And-Simple-Resolve.png
[Overview_Advanced_Fast]:../images/benchmark-results/Overview_Advanced_Fast.png 
[Overview_Advanced_Slow]:../images/benchmark-results/Overview_Advanced_Slow.png
[Overview_Basic_Fast]:../images/benchmark-results/Overview_Basic_Fast.png  
[Overview_Basic_Slow]:../images/benchmark-results/Overview_Basic_Slow.png  
[Overview_Prepare_Fast]:../images/benchmark-results/Overview_Prepare_Fast.png  
[Overview_Prepare_Slow]:../images/benchmark-results/Overview_Prepare_Slow.png  