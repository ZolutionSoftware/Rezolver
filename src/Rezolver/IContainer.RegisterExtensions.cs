// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
  public static partial class IRezolverExtensions
  {
    //registration extensions - wrap around the resolver's Builder object

    /// <summary>
    /// Method which validates that a rezolver can take registrations through a builder.
    /// </summary>
    /// <param name="rezolver"></param>
    /// <param name="parameterName"></param>
    private static void RezolverMustHaveBuilder(IContainer rezolver, string parameterName = "rezolver")
    {
      try
      {
        if (rezolver.Builder == null)
          throw new ArgumentException("rezolver's Builder property must be non-null", parameterName);
      }
      catch (NotSupportedException ex)
      {
        throw new ArgumentException("rezolver does not support registration through its IRezolveTargetContainer", ex);
      }
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainer.Register(ITarget, Type)"/> method of the <paramref name="rezolver"/>
    /// argument's <see cref="IContainer.Builder"/>
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <param name="rezolver"></param>
    /// <param name="target"></param>
    /// <param name="type"></param>
    public static void Register(this IContainer rezolver, ITarget target, Type type = null)
    {
      rezolver.MustNotBeNull("rezolver");
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.Register(target, type);
    }


    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterMultiple(ITargetContainer, IEnumerable{ITarget}, Type)"/>, please see that method for 
    /// reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <param name="rezolver">The rezolver.</param>
    /// <param name="targets"></param>
    /// <param name="commonServiceType"></param>
    /// <param name="path"></param>
    public static void RegisterMultiple(this IContainer rezolver, IEnumerable<ITarget> targets, Type commonServiceType = null)
    {
      rezolver.MustNotBeNull("rezolver");
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterMultiple(targets, commonServiceType);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterExpression{T}(ITargetContainer, Expression{Func{RezolveContextExpressionHelper, T}}, Type, RezolverPath, ITargetAdapter)"/>,
    /// please see that method for the reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rezolver">The rezolver.</param>
    /// <param name="expression"></param>
    /// <param name="type"></param>
    /// <param name="path"></param>
    /// <param name="adapter"></param>
    public static void RegisterExpression<T>(this IContainer rezolver, Expression<Func<RezolveContextExpressionHelper, T>> expression, Type type = null, ITargetAdapter adapter = null)
    {
      rezolver.MustNotBeNull("rezolver");
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterExpression<T>(expression, type, adapter);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterObject{T}(ITargetContainer, T, Type, bool)"/>, please see that method
    /// for reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rezolver">The rezolver.</param>
    /// <param name="obj"></param>
    /// <param name="type"></param>
    /// <param name="suppressScopeTracking"></param>
    public static void RegisterObject<T>(this IContainer rezolver, T obj, Type type = null, bool suppressScopeTracking = true)
    {
      rezolver.MustNotBeNull("rezolver");
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterObject(obj, serviceType: type, suppressScopeTracking: suppressScopeTracking);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterType{TObject}(ITargetContainer, IPropertyBindingBehaviour)"/>, please 
    /// see that method for reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="rezolver">The rezolver.</param>
    /// <param name="propertyBindingBehaviour">The property binding behaviour.</param>
    /// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via their
    /// 'Auto' static methods and then registering them.</remarks>
    public static void RegisterType<TObject>(this IContainer rezolver, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      rezolver.MustNotBeNull("rezolver");
      rezolver.Builder.RegisterType<TObject>(propertyBindingBehaviour: propertyBindingBehaviour);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterType{TObject, TService}(ITargetContainer, RezolverPath, IPropertyBindingBehaviour)"/>, please
    /// see that method for reference for the parameters
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TService"></typeparam>
    /// <param name="rezolver"></param>
    /// <param name="propertyBindingBehaviour"></param>
    public static void RegisterType<TObject, TService>(this IContainer rezolver, IPropertyBindingBehaviour propertyBindingBehaviour = null)
        where TObject : TService
    {
      rezolver.MustNotBeNull("rezolver");
      rezolver.Builder.RegisterType<TObject, TService>(propertyBindingBehaviour: propertyBindingBehaviour);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterType(ITargetContainer, Type, Type, IPropertyBindingBehaviour)"/>, please
    /// see that method for reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <param name="rezolver"></param>
    /// <param name="objectType"></param>
    /// <param name="serviceType"></param>
    /// <param name="propertyBindingBehaviour"></param>
    public static void RegisterType(this IContainer rezolver, Type objectType, Type serviceType = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      rezolver.MustNotBeNull("rezolver");
      objectType.MustNotBeNull("objectType");

      rezolver.Builder.RegisterType(objectType, serviceType: serviceType, propertyBindingBehaviour: propertyBindingBehaviour);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterScoped{TObject}(ITargetContainer, IPropertyBindingBehaviour)"/>, please
    /// see that method for reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="rezolver"></param>
    /// <param name="propertyBindingBehaviour"></param>
    public static void RegisterScoped<TObject>(this IContainer rezolver, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      rezolver.MustNotBeNull(nameof(rezolver));
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterScoped<TObject>(propertyBindingBehaviour: propertyBindingBehaviour);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterScoped{TObject, TService}(ITargetContainer, IPropertyBindingBehaviour)"/>, please
    /// see that method for reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TService"></typeparam>
    /// <param name="rezolver"></param>

    /// <param name="propertyBindingBehaviour"></param>
    public static void RegisterScoped<TObject, TService>(this IContainer rezolver, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      rezolver.MustNotBeNull(nameof(rezolver));
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterScoped<TObject, TService>(propertyBindingBehaviour: propertyBindingBehaviour);
    }

    /// <summary>
    /// Wrapper for the <see cref="ITargetContainerExtensions.RegisterScoped(ITargetContainer, Type, Type, RezolverPath, IPropertyBindingBehaviour)"/>, please
    /// see that method for reference for the parameters.
    /// 
    /// Note - the rezolver must have a non-null <see cref="IContainer.Builder"/> in order for this extension method to work.
    /// </summary>
    /// <param name="rezolver"></param>
    /// <param name="objectType"></param>
    /// <param name="serviceType"></param>
    /// <param name="propertyBindingBehaviour"></param>
    public static void RegisterScoped(this IContainer rezolver, Type objectType, Type serviceType = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
    {
      rezolver.MustNotBeNull(nameof(rezolver));
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterScoped(objectType, serviceType: serviceType, propertyBindingBehaviour: propertyBindingBehaviour);
    }

    /// <summary>
    /// Batch-registers multiple targets with different contracts.  Basically, this is like calling Register(IRezolveTarget) multiple times, but with an 
    /// enumerable.
    /// </summary>
    /// <param name="rezolver"></param>
    /// <param name="targets"></param>
    public static void RegisterAll(this IContainer rezolver, IEnumerable<ITarget> targets)
    {
      rezolver.MustNotBeNull(nameof(rezolver));
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterAll(targets);
    }

    /// <summary>
    /// Parameter array version of the RegisterAll method.
    /// </summary>
    /// <param name="rezolver">The rezolver.</param>
    /// <param name="targets">The targets.</param>
    public static void RegisterAll(this IContainer rezolver, params ITarget[] targets)
    {
      rezolver.MustNotBeNull(nameof(rezolver));
      RezolverMustHaveBuilder(rezolver);
      rezolver.Builder.RegisterAll(targets);
    }
  }
}
