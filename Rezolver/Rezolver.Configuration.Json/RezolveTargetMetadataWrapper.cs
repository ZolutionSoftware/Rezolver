using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	/// <summary>
	/// Metadata wrapper to accommodate the nature of how JSON configuration is deserialized.
	/// 
	/// Of little practical use outside of the Json configuration library, unless you are creating your
	/// own metadata types which need the Unwrap semantics
	/// </summary>
	public class RezolveTargetMetadataWrapper : RezolveTargetMetadataBase, IRezolveTargetMetadataExtension
	{
		public const string ExtensionTypeName = "#JSONWRAPPER#";
		public string ExtensionType
		{
			get { return ExtensionTypeName; }
		}

		private readonly IRezolveTargetMetadata _wrapped;

		public RezolveTargetMetadataWrapper(IRezolveTargetMetadata wrapped)
			: base(RezolveTargetMetadataType.Extension)
		{
			// TODO: Complete member initialization
			this._wrapped = wrapped;
		}

		/// <summary>
		/// Unwraps the metadata contained within this wrapper for the passed target types in readiness to 
		/// be pushed into an IConfigurationEntry
		/// </summary>
		/// <param name="forEntry"></param>
		/// <returns></returns>
		public virtual IRezolveTargetMetadata UnwrapMetadata(ITypeReference[] forTargetTypes)
		{
			if (_wrapped.Type == RezolveTargetMetadataType.Extension)
			{
				RezolveTargetMetadataWrapper wrapper = _wrapped as RezolveTargetMetadataWrapper;
				if (wrapper != null)
					return wrapper.UnwrapMetadata(forTargetTypes);
			}
			else if (_wrapped.Type == RezolveTargetMetadataType.Constructor)
			{
				ConstructorTargetMetadata ctorMetadata = _wrapped as ConstructorTargetMetadata;
				if(ctorMetadata != null)
				{
					//have to re-write the constructor metadata if the type reference is equal to '$self' as that's
					//a special token used in JSON configuration loading to shortcut registering a type as itself.
					if (ctorMetadata.TypesToBuild.Length == 1 && ctorMetadata.TypesToBuild[0].TypeName == JsonConfiguration.AutoConstructorType)
						return new ConstructorTargetMetadata(forTargetTypes);
				}
				
			}

			return _wrapped;
		}
	}
}
