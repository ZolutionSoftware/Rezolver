using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class ConstructorTargetMetadata : RezolveTargetMetadataBase, Rezolver.Configuration.IConstructorTargetMetadata
	{
		private ITypeReference[] _typesToBuild;

		/// <summary>
		/// Remember - the intention is that only one of these types will actually be bound on constructing the contructor target.
		/// </summary>
		public ITypeReference[] TypesToBuild
		{
			get
			{
				return _typesToBuild;
			}
		}

		/// <summary>
		/// Constructs a new instance of the ConstructorTargetMetadata class.
		/// </summary>
		/// <param name="typesToBuild"></param>
		/// <remarks>
		/// Please note that although the typesToBuild parameter is an array, in practise only one type can ever be built.
		/// 
		/// Multiple types are accepted to cover scenarios where a class, its bases and zero or more interfaces are all referenced
		/// as target registration types, and these types are passed directly to the constructor target.  In building such a target from
		/// the metadata an adapter will typically find the most derived type of the group and bind to that only.  Note that if one or
		/// more types are not related to the rest of the group, then an adapter is free to throw a runtime error.</remarks>
		public ConstructorTargetMetadata(ITypeReference[] typesToBuild)
			: base(RezolveTargetMetadataType.Constructor)
		{
			if (typesToBuild == null) throw new ArgumentNullException("typesToBuild");
			if (typesToBuild.Length == 0) throw new ArgumentException("The array cannot be empty", "typesToBuild");
			if (typesToBuild.Any(t => t == null)) throw new ArgumentException("All entries in the array must be non-null", "typesToBuild");
			// TODO: Complete member initialization
			this._typesToBuild = typesToBuild;
		}

		protected override IRezolveTarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			//note that we don't validate the metadata's types are compatible with the target types - this should already be handled
			//by any IRezolverBuilder implementation.
			Type[] typesToBuild;
			Type typeToBuild = null;
			
			//parse the TypesToBuild property value
			if (!context.TryParseTypeReferences(TypesToBuild, out typesToBuild))
				return null;

			if (typesToBuild.Length > 1)
			{
				//all the types in the list must have a common base which is in the list.
				var commonBase = typesToBuild.FirstOrDefault(t => typesToBuild.All(tt => tt != t || t.IsAssignableFrom(tt)));

				if (commonBase == null)
				{
					context.AddError(new ConfigurationError("If multiple types are provided for a constructor target, they must all be part of a common hierarchy", entry));
					return null;
				}

				//now get most derived.  Defined as one type which cannot be assigned from any of the others.
				//if there is more than one of these, then we error unless only one of them is a non-interface, non-abstract type.
				var mostDerived = typesToBuild.Where(t => typesToBuild.All(tt => tt != t || !t.IsAssignableFrom(tt))).ToArray();

				if (mostDerived.Length == 0)
				{
					context.AddError(new ConfigurationError("Couldn't identify a most-derived type to be built from the list of target types", entry));
					return null;
				}
				else if (mostDerived.Length > 1)
				{
					//get all non-abstract class types
					var nonAbstracts = typesToBuild.Where(t => t.IsClass && !t.IsAbstract).ToArray();
					if (nonAbstracts.Length > 1)
					{
						context.AddError(new ConfigurationError("More than one non-abstract class type provided in target types - can't automatically choose which one to build", entry));
						return null;
					}
					else
						typeToBuild = nonAbstracts[0];
				}
				else
					typeToBuild = mostDerived[0];
			}
			else
				typeToBuild = typesToBuild[0];

			if (typeToBuild.IsGenericTypeDefinition)
				return GenericConstructorTarget.Auto(typeToBuild);
			else
				return ConstructorTarget.Auto(typeToBuild);
		}
	}
}
