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
		/// <summary>
		/// Gets the metadata for the targets that will be used for the items that'll be returned
		/// in the Array or List that will be created by the <see cref="ListTarget" /> created from this metadata.
		/// </summary>
		/// <value>The elements.</value>
		public IRezolveTargetMetadataList Items { get; private set; }

		/// <summary>
		/// Maps to the <see cref="ListTarget.IsArray" /> property.  If true, then an array of <see cref="ElementType" />
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

		protected override IRezolveTarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			Type elementType;

			if (!context.TryParseTypeReference(ElementType, out elementType))
				return null;

			return new ListTarget(elementType, Items.CreateRezolveTargets(targetTypes, context, entry));
		}
	}
}
