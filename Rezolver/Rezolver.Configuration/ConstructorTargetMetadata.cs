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


	}
}
