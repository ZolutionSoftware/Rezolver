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
        private static void RezolverMustHaveBuilder(IRezolver rezolver, string parameterName = "rezolver")
        {
            try
            {
                if (rezolver.Builder == null)
                    throw new ArgumentException("rezolver's Builder property must be non-null", parameterName);
            }
            catch (NotSupportedException ex)
            {
                throw new ArgumentException("rezolver does not support registration through its IRezolverBuilder", ex);
            }
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/> method of the <paramref name="rezolver"/>
        /// argument's <see cref="IRezolver.Builder"/>
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <param name="rezolver"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="path"></param>
        public static void Register(this IRezolver rezolver, IRezolveTarget target, Type type = null, RezolverPath path = null)
        {
            rezolver.MustNotBeNull("rezolver");
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.Register(target, type, path);
        }


        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterMultiple(IRezolverBuilder, IEnumerable{IRezolveTarget}, Type, RezolverPath)"/>, please see that method for 
        /// reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <param name="rezolver">The rezolver.</param>
        /// <param name="targets"></param>
        /// <param name="commonServiceType"></param>
        /// <param name="path"></param>
        public static void RegisterMultiple(this IRezolver rezolver, IEnumerable<IRezolveTarget> targets, Type commonServiceType = null, RezolverPath path = null)
        {
            rezolver.MustNotBeNull("rezolver");
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.RegisterMultiple(targets, commonServiceType, path);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterExpression{T}(IRezolverBuilder, Expression{Func{RezolveContextExpressionHelper, T}}, Type, RezolverPath, IRezolveTargetAdapter)"/>,
        /// please see that method for the reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rezolver">The rezolver.</param>
        /// <param name="expression"></param>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="adapter"></param>
        public static void RegisterExpression<T>(this IRezolver rezolver, Expression<Func<RezolveContextExpressionHelper, T>> expression, Type type = null, RezolverPath path = null, IRezolveTargetAdapter adapter = null)
        {
            rezolver.MustNotBeNull("rezolver");
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.RegisterExpression<T>(expression, type, path, adapter);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterObject{T}(IRezolverBuilder, T, Type, RezolverPath, bool)"/>, please see that method
        /// for reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rezolver">The rezolver.</param>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="path"></param>
        public static void RegisterObject<T>(this IRezolver rezolver, T obj, Type type = null, RezolverPath path = null, bool suppressScopeTracking = true)
        {
            rezolver.MustNotBeNull("rezolver");
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.RegisterObject(obj, serviceType: type, suppressScopeTracking: suppressScopeTracking);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterType{TObject}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/>, please 
        /// see that method for reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="rezolver">The rezolver.</param>
        /// <param name="path">The path.</param>
        /// <param name="propertyBindingBehaviour">The property binding behaviour.</param>
        /// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via their
        /// 'Auto' static methods and then registering them.</remarks>
        public static void RegisterType<TObject>(this IRezolver rezolver, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
        {
            rezolver.MustNotBeNull("rezolver");
            rezolver.Builder.RegisterType<TObject>(path: path, propertyBindingBehaviour: propertyBindingBehaviour);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterType{TObject, TService}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/>, please
        /// see that method for reference for the parameters
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="rezolver"></param>
        /// <param name="path"></param>
        /// <param name="propertyBindingBehaviour"></param>
        public static void RegisterType<TObject, TService>(this IRezolver rezolver, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
            where TObject : TService
        {
            rezolver.MustNotBeNull("rezolver");
            rezolver.Builder.RegisterType<TObject, TService>(path: path, propertyBindingBehaviour: propertyBindingBehaviour);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterType(IRezolverBuilder, Type, Type, RezolverPath, IPropertyBindingBehaviour)"/>, please
        /// see that method for reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <param name="rezolver"></param>
        /// <param name="objectType"></param>
        /// <param name="serviceType"></param>
        /// <param name="path"></param>
        /// <param name="propertyBindingBehaviour"></param>
        public static void RegisterType(this IRezolver rezolver, Type objectType, Type serviceType = null, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
        {
            rezolver.MustNotBeNull("rezolver");
            objectType.MustNotBeNull("objectType");

            rezolver.Builder.RegisterType(objectType, serviceType: serviceType, path: path, propertyBindingBehaviour: propertyBindingBehaviour);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterScoped{TObject}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/>, please
        /// see that method for reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="rezolver"></param>
        /// <param name="path"></param>
        /// <param name="propertyBindingBehaviour"></param>
        public static void RegisterScoped<TObject>(this IRezolver rezolver, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
        {
            rezolver.MustNotBeNull(nameof(rezolver));
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.RegisterScoped<TObject>(path: path, propertyBindingBehaviour: propertyBindingBehaviour);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterScoped{TObject, TService}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/>, please
        /// see that method for reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="rezolver"></param>
        /// <param name="path"></param>
        /// <param name="propertyBindingBehaviour"></param>
        public static void RegisterScoped<TObject, TService>(this IRezolver rezolver, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
        {
            rezolver.MustNotBeNull(nameof(rezolver));
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.RegisterScoped<TObject, TService>(path: path, propertyBindingBehaviour: propertyBindingBehaviour);
        }

        /// <summary>
        /// Wrapper for the <see cref="IRezolverBuilderExtensions.RegisterScoped(IRezolverBuilder, Type, Type, RezolverPath, IPropertyBindingBehaviour)"/>, please
        /// see that method for reference for the parameters.
        /// 
        /// Note - the rezolver must have a non-null <see cref="IRezolver.Builder"/> in order for this extension method to work.
        /// </summary>
        /// <param name="rezolver"></param>
        /// <param name="objectType"></param>
        /// <param name="serviceType"></param>
        /// <param name="path"></param>
        /// <param name="propertyBindingBehaviour"></param>
        public static void RegisterScoped(this IRezolver rezolver, Type objectType, Type serviceType = null, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
        {
            rezolver.MustNotBeNull(nameof(rezolver));
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.RegisterScoped(objectType, serviceType: serviceType, path: path, propertyBindingBehaviour: propertyBindingBehaviour);
        }

        /// <summary>
        /// Batch-registers multiple targets with different contracts.  Basically, this is like calling Register(IRezolveTarget) multiple times, but with an 
        /// enumerable.
        /// </summary>
        /// <param name="rezolver"></param>
        /// <param name="targets"></param>
        public static void RegisterAll(this IRezolver rezolver, IEnumerable<IRezolveTarget> targets)
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
        public static void RegisterAll(this IRezolver rezolver, params IRezolveTarget[] targets)
        {
            rezolver.MustNotBeNull(nameof(rezolver));
            RezolverMustHaveBuilder(rezolver);
            rezolver.Builder.RegisterAll(targets);
        }
    }
}
