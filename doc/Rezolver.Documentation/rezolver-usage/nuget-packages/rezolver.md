# Nuget Package: Rezolver

[See package page on nuget](https://www.nuget.org/packages/Rezolver).

The core nuget package for using Rezolver in your class library or application, includes all the 
core abstractions and classes required to create @TargetContainer objects and @Container objects in your composition root.

Also contains the core @Rezolver.ITarget implementations and logic which allow you to create/retrieve objects in numerous ways:
 
- Binding generic and non-generic class constructors using dynamically resolved arguments (@Rezolver.ConstructorTarget, @Rezolver.GenericConstructorTarget and @Rezolver.RezolvedTarget)
- Invoking pre-built delegates (@Rezolver.DelegateTarget)
- Pre-built objects (@Rezolver.ObjectTarget)
- Dynamic code built from custom expression trees (@Rezolver.ExpressionTarget)
- Decoration (@Rezolver.DecoratorTarget)
- Singleton objects (@Rezolver.SingletonTarget)
- Scoped objects (@Rezolver.ScopedTarget)
