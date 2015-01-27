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
			IRezolveTargetMetadata result = null;
			if (_wrapped.Type == RezolveTargetMetadataType.Extension)
				result = UnwrapExtensionMetadata(_wrapped, forTargetTypes);
			else if (_wrapped.Type == RezolveTargetMetadataType.Constructor)
				result = UnwrapConstructorMetadata(_wrapped, forTargetTypes);
			else if (_wrapped.Type == RezolveTargetMetadataType.List)
				result = UnwrapListMetadata(_wrapped, forTargetTypes);
			else if (_wrapped.Type == RezolveTargetMetadataType.MetadataList)
				result = UnwrapMetadataList(_wrapped, forTargetTypes);

			return result ?? _wrapped;
		}
		protected virtual IRezolveTargetMetadata UnwrapExtensionMetadata(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			RezolveTargetMetadataWrapper wrapper = meta as RezolveTargetMetadataWrapper;
			if (wrapper != null)
				return wrapper.UnwrapMetadata(forTargetTypes);
			return null;
		}

		protected virtual IRezolveTargetMetadata UnwrapConstructorMetadata(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			IConstructorTargetMetadata ctorMetadata = meta as IConstructorTargetMetadata;
			if (ctorMetadata != null)
			{
				//have to re-write the constructor metadata if the type reference is equal to '$self' as that's
				//a special token used in JSON configuration loading to short cut registering a type as itself.
				//this is going to get more involved if we start doing special constructor parameter bindings or
				//propety bindings.
				if (ctorMetadata.TypesToBuild.Length == 1 && ctorMetadata.TypesToBuild[0].TypeName == JsonConfiguration.AutoConstructorType)
					return new ConstructorTargetMetadata(forTargetTypes);
			}

			return null;
		}

		protected virtual IRezolveTargetMetadata UnwrapListMetadata(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{

		}

		protected virtual IRezolveTargetMetadataList UnwrapMetadataList(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			//have to unwrap the outer list, and all the inner items, if applicable
			IRezolveTargetMetadataList list = _wrapped as IRezolveTargetMetadataList;
			if (list != null)
			{
				return new RezolveTargetMetadataList(list.Targets.Select(t => UnwrapMetadataIfNecessary(t, forTargetTypes)));
			}
			return null;
		}

		private IRezolveTargetMetadata UnwrapMetadataIfNecessary(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			RezolveTargetMetadataWrapper wrapper = meta as RezolveTargetMetadataWrapper;
			if (wrapper != null)
				return wrapper.UnwrapMetadata(forTargetTypes);
			else
				return meta;
		}


	}
}
