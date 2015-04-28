Rezolver Design Notes
=====================

Much of the design for this framework is as you would expect from all IOC containers:

IRezolver interface
-------------------
The container, once it has been built.  It is from this that you obtain instances of
objects (possibly named).  The entry point for an IOC call.

IRezolverBuilder interface
--------------------------
As the name suggests, an instance of this interface contains all the type registrations
that will then be used to create an IRezolver.

IRezolveTarget
--------------
The core of Rezolver is dynamic code generation through expressions.  This type is used
to produce those expressions for the different methods by which objects
can be resolved.

ICompiledRezolveTarget
----------------------
An IRezolveTarget is compiled into one of these.  The methods on this type
produce the actual instances that are resolved by an IRezolver.

IRezolveTargetCompiler
----------------------
Responsible for creating ICompiledRezolveTarget instances from an IRezolveTarget - required
by IRezolver to turn targets into code.

There are two compilers - a core portable one which builds delegates from expressions, 
and one which builds dynamically compiled implementations of ICompiledRezolveTargets.
The second is the fastest and most efficient, but is unable to execute non-public code.