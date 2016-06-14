using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public static partial class ITargetContainerExtensions
	{
		/// <summary>
		/// Version of <see cref="RegisterType{TObject}(ITargetContainer, IPropertyBindingBehaviour)"/> that creates singleton instances that are also scoped
		/// to the root scope of the active <see cref="IScopedContainer"/> at the time <see cref="IContainer.Resolve(RezolveContext)"/> (or <see cref="IContainer.TryResolve(RezolveContext, out object)"/>)
		/// is called.
		/// </summary>
		/// <typeparam name="TObject">See <see cref="RegisterType{TObject}(ITargetContainer, IPropertyBindingBehaviour)"/></typeparam>
		/// <param name="builder"></param>
		/// <param name="propertyBindingBehaviour">See <see cref="RegisterType{TObject}(ITargetContainer, IPropertyBindingBehaviour)"/></param>
		public static void RegisterSingleton<TObject>(this ITargetContainer builder, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			RegisterSingleton(builder, typeof(TObject), propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// Version of <see cref="RegisterType{TObject, TService}(ITargetContainer, IPropertyBindingBehaviour)"/> that creates singleton instances that are also scoped
		/// to the root scope of the active <see cref="IScopedContainer"/> at the time <see cref="IContainer.Resolve(RezolveContext)"/> (or <see cref="IContainer.TryResolve(RezolveContext, out object)"/>)
		/// is called.
		/// </summary>
		/// <typeparam name="TObject">See <see cref="RegisterType{TObject, TService}(ITargetContainer, IPropertyBindingBehaviour)"/></typeparam>
		/// <typeparam name="TService">See <see cref="RegisterType{TObject, TService}(ITargetContainer, IPropertyBindingBehaviour)"/></typeparam>
		/// <param name="builder"></param>
		/// <param name="propertyBindingBehaviour">See <see cref="RegisterType{TObject, TService}(ITargetContainer, IPropertyBindingBehaviour)"/></param>
		public static void RegisterSingleton<TObject, TService>(this ITargetContainer builder, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			RegisterSingleton(builder, typeof(TObject), typeof(TService), propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// Version of <see cref="RegisterType(ITargetContainer, Type, Type, IPropertyBindingBehaviour)"/> that creates singleton instances that are also scoped
		/// to the root scope of the active <see cref="IScopedContainer"/> at the time <see cref="IContainer.Resolve(RezolveContext)"/> (or <see cref="IContainer.TryResolve(RezolveContext, out object)"/>)
		/// is called.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="objectType"><see cref="RegisterType(ITargetContainer, Type, Type, IPropertyBindingBehaviour)" /></param>
		/// <param name="serviceType"><see cref="RegisterType(ITargetContainer, Type, Type, IPropertyBindingBehaviour)" /></param>
		/// <param name="propertyBindingBehaviour"><see cref="RegisterType(ITargetContainer, Type, Type, IPropertyBindingBehaviour)" /></param>
		public static void RegisterSingleton(this ITargetContainer builder, Type objectType, Type serviceType = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			builder.MustNotBeNull(nameof(builder));
			objectType.MustNotBeNull(nameof(builder));

			RegisterSingletonInternal(builder, objectType, serviceType, propertyBindingBehaviour);
		}

		internal static void RegisterSingletonInternal(ITargetContainer builder, Type objectType, Type serviceType, IPropertyBindingBehaviour propertyBindingBehaviour)
		{
			builder.Register(New(objectType, propertyBindingBehaviour).Singleton(), serviceType: serviceType);
		}
	}
}
