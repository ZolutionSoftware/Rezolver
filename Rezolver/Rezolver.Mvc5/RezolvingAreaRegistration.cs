using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Rezolver
{
	/// <summary>
	/// A new base class from which to inherit your area registrations.  The RegisterArea method is altered
	/// to provide an IRezolver instance that's been grabbed from context, meaning that, as part of your registration,
	/// you can register area-specific services/values.
	/// 
	/// REmember that when using this in conjunction with the RezolvingControllerActivator, types are resolved by the area
	/// name and controller name, leaving lots of scope for area/controller-based overrides.
	/// </summary>
	public abstract class RezolvingAreaRegistration : AreaRegistration
	{
		/// <summary>
		/// Implement this method, instead of the standard version (which is sealed by this class anyway),
		/// so that if there is an IRezolver instance in scope for you to register objects/types into, you
		/// get passed it.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="targetRezolver"></param>
		public abstract void RegisterArea(AreaRegistrationContext context, IRezolver targetRezolver);

		public sealed override void RegisterArea(AreaRegistrationContext context)
		{
			IRezolver rezolver = GetContextRezolver(context);
			RegisterArea(context, rezolver);
		}

		protected virtual IRezolver GetContextRezolver(AreaRegistrationContext context)
		{
			//by default we don't even read from the context, we actually use the current
			//dependency resolver that's been set into MVC.
			RezolverDependencyResolver depResolver = DependencyResolver.Current as RezolverDependencyResolver;
			if (depResolver != null)
				return depResolver.Rezolver;
			else
				return null;
		}
	}
}
