using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public static partial class IRezolverBuilderExtensions
	{
		/// <summary>
		/// Version of <see cref="RegisterType{TObject}(IRezolverBuilder, IPropertyBindingBehaviour)"/> that creates singleton instances that are also scoped
		/// to the root scope of the active <see cref="ILifetimeScopeRezolver"/> at the time <see cref="IRezolver.Resolve(RezolveContext)"/> (or <see cref="IRezolver.TryResolve(RezolveContext, out object)"/>)
		/// is called.
		/// </summary>
		/// <typeparam name="TObject">See <see cref="RegisterType{TObject}(IRezolverBuilder, IPropertyBindingBehaviour)"/></typeparam>
		/// <param name="builder"></param>
		/// <param name="propertyBindingBehaviour">See <see cref="RegisterType{TObject}(IRezolverBuilder, IPropertyBindingBehaviour)"/></param>
		public static void RegisterSingleton<TObject>(this IRezolverBuilder builder, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			RegisterSingleton(builder, typeof(TObject), propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// Version of <see cref="RegisterType{TObject, TService}(IRezolverBuilder, IPropertyBindingBehaviour)"/> that creates singleton instances that are also scoped
		/// to the root scope of the active <see cref="ILifetimeScopeRezolver"/> at the time <see cref="IRezolver.Resolve(RezolveContext)"/> (or <see cref="IRezolver.TryResolve(RezolveContext, out object)"/>)
		/// is called.
		/// </summary>
		/// <typeparam name="TObject">See <see cref="RegisterType{TObject, TService}(IRezolverBuilder, IPropertyBindingBehaviour)"/></typeparam>
		/// <typeparam name="TService">See <see cref="RegisterType{TObject, TService}(IRezolverBuilder, IPropertyBindingBehaviour)"/></typeparam>
		/// <param name="builder"></param>
		/// <param name="propertyBindingBehaviour">See <see cref="RegisterType{TObject, TService}(IRezolverBuilder, IPropertyBindingBehaviour)"/></param>
		public static void RegisterSingleton<TObject, TService>(this IRezolverBuilder builder, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			RegisterSingleton(builder, typeof(TObject), typeof(TService), propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// Version of <see cref="RegisterType(IRezolverBuilder, Type, Type, IPropertyBindingBehaviour)"/> that creates singleton instances that are also scoped
		/// to the root scope of the active <see cref="ILifetimeScopeRezolver"/> at the time <see cref="IRezolver.Resolve(RezolveContext)"/> (or <see cref="IRezolver.TryResolve(RezolveContext, out object)"/>)
		/// is called.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="objectType"><see cref="RegisterType(IRezolverBuilder, Type, Type, IPropertyBindingBehaviour)</param>
		/// <param name="serviceType"><see cref="RegisterType(IRezolverBuilder, Type, Type, IPropertyBindingBehaviour)</param>
		/// <param name="propertyBindingBehaviour"><see cref="RegisterType(IRezolverBuilder, Type, Type, IPropertyBindingBehaviour)</param>
		public static void RegisterSingleton(this IRezolverBuilder builder, Type objectType, Type serviceType = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			builder.MustNotBeNull(nameof(builder));
			objectType.MustNotBeNull(nameof(builder));

			RegisterSingletonInternal(builder, objectType, serviceType, propertyBindingBehaviour);
		}

		internal static void RegisterSingletonInternal(IRezolverBuilder builder, Type objectType, Type serviceType, IPropertyBindingBehaviour propertyBindingBehaviour)
		{
			builder.Register(New(objectType, propertyBindingBehaviour).Singleton(), serviceType: serviceType);
		}
	}
}
