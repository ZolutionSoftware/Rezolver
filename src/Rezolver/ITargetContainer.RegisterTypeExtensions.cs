// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Extensions for <see cref="ITargetContainer"/> to provide shortcuts for the simplest cases of registering
	/// <see cref="ConstructorTarget"/> and <see cref="GenericConstructorTarget"/> targets.
	/// </summary>
	public static partial class RegisterTypeTargetContainerExtensions
	{
		/// <summary>
		/// Registers an instance of <typeparamref name="TObject"/> to be created by an <see cref="IContainer"/> via constructor injection.  
		/// The registration will auto-bind a constructor based on the services available in the <see cref="ITargetContainer"/> and 
		/// <see cref="IContainer"/> available at the time <see cref="IContainer.Resolve(RezolveContext)"/> is first called.
		/// 
		/// Optionally, property injection can be configured for the new object, depending on the <see cref="IMemberBindingBehaviour"/> 
		/// object passed for the optional parameter <paramref name="propertyBindingBehaviour"/>.
		/// </summary>
		/// <typeparam name="TObject">The type of the object that is to be constructed when resolved.  Also doubles up as the type to be 
		/// used for the registration itself.</typeparam>
		/// <param name="targetContainer">The target container on which the registration is to be performed.</param>
		/// <param name="propertyBindingBehaviour">Can be used to enable and control property injection in addition to constructor injection
		/// on the instance of <typeparamref name="TObject"/> that is created.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via
		/// the <see cref="ConstructorTarget.Auto{T}(IMemberBindingBehaviour)"/> or 
		/// <see cref="GenericConstructorTarget.Auto{TGeneric}(IMemberBindingBehaviour)"/> static methods and then registering it.</remarks>
		public static void RegisterType<TObject>(this ITargetContainer targetContainer, IMemberBindingBehaviour propertyBindingBehaviour = null)
		{
			RegisterType(targetContainer, typeof(TObject), propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// Registers an instance of <typeparamref name="TObject"/> for the service type <typeparamref name="TService"/> to be created by 
		/// an <see cref="IContainer"/> via constructor injection.  
		/// The registration will auto-bind a constructor based on the services available in the <see cref="ITargetContainer"/> and 
		/// <see cref="IContainer"/> available at the time <see cref="IContainer.Resolve(RezolveContext)"/> is first called.
		/// 
		/// Optionally, property injection can be configured for the new object, depending on the <see cref="IMemberBindingBehaviour"/> 
		/// object passed for the optional parameter <paramref name="propertyBindingBehaviour"/>.
		/// </summary>
		/// <typeparam name="TObject">The type of the object that is to be constructed when resolved.</typeparam>
		/// <typeparam name="TService">The type against which the registration will be performed.  <typeparamref name="TObject"/> must be
		/// compatible with this type.</typeparam>
		/// <param name="targetContainer">The target container on which the registration is to be performed.</param>
		/// <param name="propertyBindingBehaviour">Can be used to enable and control property injection in addition to constructor injection
		/// on the instance of <typeparamref name="TObject"/> that is created.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via
		/// the <see cref="ConstructorTarget.Auto{T}(IMemberBindingBehaviour)"/> or 
		/// <see cref="GenericConstructorTarget.Auto{TGeneric}(IMemberBindingBehaviour)"/> static methods and then registering it against
		/// the type <typeparamref name="TService"/>.</remarks>
		public static void RegisterType<TObject, TService>(this ITargetContainer targetContainer, IMemberBindingBehaviour propertyBindingBehaviour = null)
		where TObject : TService
		{
			RegisterType(targetContainer, typeof(TObject), serviceType: typeof(TService), propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// Registers an instance of <paramref name="objectType"/> (optionally for the service type <paramref name="serviceType"/>) to be 
		/// created by  an <see cref="IContainer"/> via constructor injection.  
		/// The registration will auto-bind a constructor based on the services available in the <see cref="ITargetContainer"/> and 
		/// <see cref="IContainer"/> available at the time <see cref="IContainer.Resolve(RezolveContext)"/> is first called.
		/// 
		/// Optionally, property injection can be configured for the new object, depending on the <see cref="IMemberBindingBehaviour"/> 
		/// object passed for the optional parameter <paramref name="propertyBindingBehaviour"/>.
		/// </summary>
		/// <param name="targetContainer">The target container on which the registration is to be performed.</param>
		/// <param name="objectType">The type of the object that is to be constructed when resolved.</param>
		/// <param name="serviceType">Optional.  The type against which the registration will be performed, if different from 
		/// <paramref name="objectType"/>.  <paramref name="objectType"/> must be compatible with this type, if it's provided.</param>
		/// <param name="propertyBindingBehaviour">Optional.  Can be used to enable and control property injection in addition to constructor injection
		/// on the instance of <paramref name="objectType"/> that is created.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via
		/// the <see cref="ConstructorTarget.Auto(Type, IMemberBindingBehaviour)"/> or 
		/// <see cref="GenericConstructorTarget.Auto(Type, IMemberBindingBehaviour)"/> static methods and then registering it against
		/// the type <paramref name="serviceType"/> or <paramref name="objectType"/>.</remarks>
		public static void RegisterType(this ITargetContainer targetContainer, Type objectType, Type serviceType = null, IMemberBindingBehaviour propertyBindingBehaviour = null)
		{
			targetContainer.MustNotBeNull(nameof(targetContainer));
			objectType.MustNotBeNull(nameof(objectType));
			RegisterTypeInternal(targetContainer, objectType, serviceType, propertyBindingBehaviour);
		}

		internal static void RegisterTypeInternal(ITargetContainer targetContainer, Type objectType, Type serviceType, IMemberBindingBehaviour propertyBindingBehaviour)
		{
			targetContainer.Register(ConstructorTarget.Auto(objectType, propertyBindingBehaviour), serviceType: serviceType);
		}
	}
}
