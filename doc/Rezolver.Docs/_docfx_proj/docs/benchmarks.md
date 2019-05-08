# Rezolver Benchmarks

> [!NOTE]
> Daniel Palme has now integrated Rezolver into his benchmarks as standard, he regularly updates 
> [this blog post](http://www.palmmedia.de/Blog/2011/8/30/ioc-container-benchmark-performance-comparison) 
> with in depth comments and analysis on his latest results.
> * * *
> The current status of Rezolver's performance is 'Average', which is fair, because of the results
> of the Child Container tests, which we are working on improving.

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

The tests include singletons, transients, objects which require a mixture of
these, complex objects which require lots of nested dependencies, resolving enumerables and child 
containers, and does provide a reasonably thorough examination of a container's features in 
addition to its performance.
 
Excluding Rezolver and the 'No' container, which the baseline, there are 31 containers now
being stress-tested by this application, with the list growing.

# Notes

Rezolver performs very well across the board - especially in v2 now after a huge amount of refactoring
and optimisation work, however it suffers in benchmarks which constantly destroy and recreate containers.

In particular, the performance in the 'Child Containers' benchmark is particularly poor - caused by 
the fact that there's an overhead on the first @Rezolver.Container.Resolve* call to a container, 
as that's when it compiles the associated factory for that service.

Because of the way this benchmark is performed, it means that Rezolver spends most of its time 
dynamically compiling the same delegates.

As the other benchmarks show, however, Rezolver's normal-use performance is up there with the best
which, when coupled with its extensibility and featureset, should make it a decent contender for
any new project.