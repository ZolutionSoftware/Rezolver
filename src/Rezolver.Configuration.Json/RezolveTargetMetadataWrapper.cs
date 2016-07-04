// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
	[Obsolete("no longer required", true)]
	public class RezolveTargetMetadataWrapper : RezolveTargetMetadataBase, IRezolveTargetMetadataExtension
	{
		public const string ExtensionTypeName = "#JSONWRAPPER#";
		public string ExtensionType
		{
			get { return ExtensionTypeName; }
		}

		public override ITypeReference DeclaredType
		{
			get { throw new NotImplementedException(); }
		}

		private readonly IRezolveTargetMetadata _wrapped;

		public RezolveTargetMetadataWrapper(IRezolveTargetMetadata wrapped)
			: base(RezolveTargetMetadataType.Extension)
		{
			// TODO: Complete member initialization
			this._wrapped = wrapped;
		}

		protected override IRezolveTargetMetadata BindBase(params ITypeReference[] targetTypes)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Unwraps the metadata contained within this wrapper for the passed target types in readiness to 
		/// be pushed into an IConfigurationEntry.  Note - if the metadata can't be unwrapped, then the method
        /// will simply return this instance.
		/// </summary>
		/// <param name="forTargetTypes">Type references that the unwrapped metadata should support.</param>
		/// <returns></returns>
        /// <remarks>Unwrapping is the process whereby a type declared for registration is back-referenced using the '$auto' unbound
        /// type name in configuration for, say a constructor target or similar.</remarks>
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

        /// <summary>
        /// Special case version of <see cref="UnwrapMetadata(ITypeReference[])"/> for metadata with the type <see cref="RezolveTargetMetadataType.Extension"/>.
        /// 
        /// The base implementation supports instances of the <see cref="RezolveTargetMetadataWrapper"/> type only - and simply chain the call through to its
        /// UnwrapMetadata method.
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="forTargetTypes"></param>
        /// <returns></returns>
		protected virtual IRezolveTargetMetadata UnwrapExtensionMetadata(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			RezolveTargetMetadataWrapper wrapper = meta as RezolveTargetMetadataWrapper;
			if (wrapper != null)
				return wrapper.UnwrapMetadata(forTargetTypes);
			return null;
		}

        /// <summary>
        /// Unwraps an <see cref="IConstructorTargetMetadata"/> object by binding it to the <paramref name="forTargetTypes"/>.
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="forTargetTypes"></param>
        /// <returns></returns>
		protected virtual IRezolveTargetMetadata UnwrapConstructorMetadata(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			IConstructorTargetMetadata ctorMetadata = meta as IConstructorTargetMetadata;
			if (ctorMetadata != null)
			{
				//have to re-write the constructor metadata if the type reference is equal to '$auto' as that's
				//a special token used in JSON configuration loading to short cut registering a type as itself,
				//or referring back to a type that's in scope at a given point in a file.
				//this is going to get more involved if we start doing special constructor parameter bindings or
				//propety bindings.
				if (ctorMetadata.TypesToBuild.Length == 1 && ctorMetadata.TypesToBuild[0].TypeName == JsonConfiguration.UnboundType)
					return new ConstructorTargetMetadata(forTargetTypes);
			}

			return null;
		}

        /// <summary>
        /// Unwraps a list metadata (and any inner target metadata) for the target types.
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="forTargetTypes"></param>
        /// <returns></returns>
		protected virtual IRezolveTargetMetadata UnwrapListMetadata(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			//when unwrapping a list - the '$auto' virtual type reference can be used by inner targets 
			//to refer back to the element type of the list or array.
			//but first, we can also use the $auto type on the array/list itself.  If this is the case, then
			//we examine the typereference
			IListTargetMetadata listMetadata = meta as IListTargetMetadata;
			if(listMetadata != null)
			{
				ITypeReference elementType = listMetadata.ElementType;
				if(elementType != null && listMetadata.ElementType.TypeName == JsonConfiguration.UnboundType)
				{

					var forArrayType = forTargetTypes.FirstOrDefault(t => t.IsArray);


					//if the list target is an array, then the type to be used is just the type reference from the target types
					//but without the array flag.  If it's a list, then it's not really possible, because we can't reliably identify
					//that the target type is List<T>, or a type derived from it, till we actually start parsing typenames.
					if(forArrayType != null && listMetadata.IsArray)
					{
						//we set the element type of the returned list to be equal to forArrayType, but without the IsArray flag set to true.
						elementType = new TypeReference(forArrayType.TypeName, forArrayType, forArrayType.GenericArguments);
					}
				}

				return new ListTargetMetadata(elementType, UnwrapMetadataList(listMetadata.Items, new[] { elementType }), listMetadata.IsArray);
			}
			return null;
		}

        /// <summary>
        /// Unwraps a series of metadata objects stored in the list (passed in the <paramref name="meta"/> parameter) for the target types,
        /// returning another metadata list containing the unwrapped metadata objects.
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="forTargetTypes"></param>
        /// <returns></returns>
		protected IRezolveTargetMetadataList UnwrapMetadataList(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			//have to unwrap the outer list, and all the inner items, if applicable
			IRezolveTargetMetadataList list = meta as IRezolveTargetMetadataList;
			if (list != null)
				return UnwrapMetadataList(list, forTargetTypes);
			return null;
		}

        /// <summary>
        /// Strongly-typed virtual method for unwrapping metadata lists (invoked by <see cref="UnwrapMetadataList(IRezolveTargetMetadata, ITypeReference[])"/> after 
        /// confirming that the metadata passed to it is of the correct type.
        /// </summary>
        /// <param name="listMeta"></param>
        /// <param name="forTargetTypes"></param>
        /// <returns></returns>
		protected virtual IRezolveTargetMetadataList UnwrapMetadataList(IRezolveTargetMetadataList listMeta, ITypeReference[] forTargetTypes)
		{
			return new RezolveTargetMetadataList(listMeta.Targets.Select(t => UnwrapMetadataIfNecessary(t, forTargetTypes)));
		}

		private IRezolveTargetMetadata UnwrapMetadataIfNecessary(IRezolveTargetMetadata meta, ITypeReference[] forTargetTypes)
		{
			RezolveTargetMetadataWrapper wrapper = meta as RezolveTargetMetadataWrapper;
			if (wrapper != null)
				return wrapper.UnwrapMetadata(forTargetTypes);
			else
				return meta;
		}

        /// <summary>
        /// Implementation of <see cref="CreateRezolveTargetBase(Type[], ConfigurationAdapterContext, IConfigurationEntry)"/>, except this implementation always
        /// throws a <see cref="NotSupportedException"/>, because it must be unwrapped (through a call to <see cref="UnwrapMetadata(ITypeReference[])"/>) before 
        /// it can be used to create an <see cref="ITarget"/>.
        /// </summary>
        /// <param name="targetTypes"></param>
        /// <param name="context"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
		protected override ITarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			throw new NotSupportedException("This metadata must be unwrapped for a specific set of target types - it is not possible to create a target from a wrapped metadata.");
		}
	}
}
