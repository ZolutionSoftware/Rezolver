using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Abstract base class for metadata that builds an object target.
	/// </summary>
	public abstract class ObjectTargetMetadataBase : RezolveTargetMetadataBase, IObjectTargetMetadata
	{
		public ObjectTargetMetadataBase() : base(RezolveTargetMetadataType.Object) { }
		protected override ITarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			//find the first type that the object target metadata object will dish out
			List<IConfigurationError> tempErrors = new List<IConfigurationError>();
			object theObject = null;
			foreach (var type in targetTypes)
			{
				try
				{
					theObject = GetObject(type);
					break;
				}
				catch (Exception ex)
				{
					tempErrors.Add(new ConfigurationError(ex, entry));
				}
			}

			//can't check for null on the theObject because it's a valid return.  But, if every
			//target type we tried yielded an error, then it's broke...
			if (tempErrors.Count == targetTypes.Length)
			{
				context.AddErrors(tempErrors);
				return null;
			}

			return new ObjectTarget(theObject);
		}

		/// <summary>
		/// Called to get the object that will be registered in the IRezolveTargetContainer to be returned when a
		/// caller requests one of its registered types. The method can construct an object anew everytime it is
		/// called, or it can always return the same instance; this behaviour is implementation-dependant.
		/// </summary>
		/// <param name="type">The type of object that is desired.  The implementation determines whether this
		/// parameter is required.  If it is, and you pass null, then an ArgumentNullException will be thrown.
		/// If you pass an argument, the implementation is not bound to check or honour the type.  Its purpose
		/// is to provide a hint only, not a guarantee that the object returned is compatible with the type.</param>
		/// <returns>An object.  Note - if the operation returns null this is not an error.</returns>
		public abstract object GetObject(Type type);
	}
}
