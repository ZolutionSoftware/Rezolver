using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Standard implementation of the <see cref="IListTargetMetadata"/> interface.
	/// </summary>
	public class ListTargetMetadata : RezolveTargetMetadataBase, IListTargetMetadata
	{
		/// <summary>
		/// Gets the declared element type of the array or list that will be created from this metadata.
		/// </summary>
		/// <value>The type of the element.</value>
		/// <exception cref="System.NotImplementedException"></exception>
		public ITypeReference ElementType
		{
			get;
			private set;
		}

		public override ITypeReference DeclaredType
		{
			get
			{
				if (ElementType.IsUnbound)
					return ElementType;

				if (IsArray)
				{
					return new TypeReference(ElementType.TypeName, ElementType, true, ElementType.GenericArguments);
				}
				else
				{
					return new TypeReference(typeof(List<>).AssemblyQualifiedName, ElementType, ElementType);
				}
			}
		}

		/// <summary>
		/// Gets the metadata for the targets that will be used for the items that'll be returned
		/// in the Array or List that will be created by the <see cref="ListTarget" /> created from this metadata.
		/// </summary>
		/// <value>The elements.</value>
		public IRezolveTargetMetadataList Items { get; private set; }

		/// <summary>
		/// Maps to the <see cref="ListTarget.AsArray" /> property.  If true, then an array of <see cref="ElementType" />
		/// will be created, otherwise a List&lt;<see cref="ElementType" />&gt; will be created by the ListTarget
		/// created from this metadata.
		/// </summary>
		/// <value><c>true</c> if this instance represents a ListTarget that will create an array; otherwise, <c>false</c>.</value>
		public bool IsArray { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ListTargetMetadata"/> class.
		/// </summary>
		/// <param name="elementType">Type of the elements of the eventual array/list.</param>
		/// <param name="items">Metadata for the targets that will eventually create the items for the array or list.</param>
		/// <param name="isArray">if set to <c>true</c> then an array is to be built, otherwise a list is to be built.</param>
		public ListTargetMetadata(ITypeReference elementType, IRezolveTargetMetadataList items, bool isArray)
			: base(RezolveTargetMetadataType.List)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");
			if (items == null)
				throw new ArgumentNullException("items");

			ElementType = elementType;
			Items = items;
			IsArray = isArray;
		}

		protected override ITarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			Type elementType;

			if (!context.TryParseTypeReference(ElementType, out elementType))
				return null;

			return new ListTarget(elementType, Items.CreateRezolveTargets(targetTypes, context, entry), IsArray);
		}

		public override IRezolveTargetMetadata Bind(ITypeReference[] targetTypes)
		{
			//this ALWAYS forces a bind on the inner targets, regardless of whether this object's
			//declared type is unbound or not - that's because this object might not be unbound,
			//but the inner targets might be.

			//little bit annoying - have to duplicate the argument check here.
			if (targetTypes == null) throw new ArgumentNullException("targetTypes");
			if (targetTypes.Length == 0) throw new ArgumentException("Array must contain at least one target type", "targetTypes");
			if (targetTypes.Any(t => t == null)) throw new ArgumentException("All items in the array must be non-null", "targetTypes");

			//look at lifting the code for this from the Json project's RezolverMetadataWrapper
			var forArrayType = targetTypes.FirstOrDefault(t => t.IsArray);

			ITypeReference newElementType = ElementType;
			//if for an array, then the type to be used is just the type reference from the target types
			//but without the array flag.  If it's a list, then it's not really possible, because we can't reliably identify
			//that the target type is List<T>, or a type derived from it, till we actually start parsing typenames.
			if (forArrayType != null && IsArray)
			{
				//we set the element type of the returned list to be equal to forArrayType, but without the IsArray flag set to true.
				newElementType = new TypeReference(forArrayType.TypeName, forArrayType, forArrayType.GenericArguments);
			}

			var boundList = Items.Bind(new[] { newElementType }) as IRezolveTargetMetadataList;

			if (boundList == null)
				throw new InvalidOperationException("The Items metadata failed to bind a new metadata list - this indicates a bad implementation");

			return new ListTargetMetadata(newElementType, boundList, IsArray);
		}

		protected override IRezolveTargetMetadata BindBase(ITypeReference[] targetTypes)
		{
			throw new NotImplementedException();
		}
	}
}
