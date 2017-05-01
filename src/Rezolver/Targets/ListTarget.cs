// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver.Targets
{
	/// <summary>
	/// This target produces arrays (<see cref="AsArray"/> = <c>true</c>) or lists (<see cref="AsArray"/> = <c>false</c>) whose 
	/// individual items are built by ITarget instances.
	/// </summary>
	/// <remarks>
	/// The element type you feed on construction determines the type of array or the generic argument to List&lt;T&gt;.
	/// 
	/// Each of the rezolve targets that you then pass must support that type.
	/// </remarks>
	public class ListTarget : TargetBase
	{
		/// <summary>
		/// Gets the declared type of each element in the array or list that will be constructed.
		/// 
		/// The <see cref="DeclaredType"/> returned by this instance will either be <c>ElementType[]</c> or <c>List&lt;ElementType&gt;</c>
		/// depending on the value of <see cref="AsArray"/>.
		/// </summary>
		/// <value>The declared type of each element.</value>
		public Type ElementType
		{
			get;
			private set;
		}

		/// <summary>
		/// Implementation of the abstract property from the base.  This will always return either a type equal to an array of <see cref="ElementType"/>,
		/// or <see cref="List{T}"/> with <see cref="ElementType"/> as the generic parameter.  This is controlled by the <see cref="AsArray"/> property.
		/// </summary>
		public override Type DeclaredType
		{
			get
			{
				if (_declaredType == null)
				{
					if (!AsArray)
						_declaredType = typeof(List<>).MakeGenericType(ElementType);
					else
						_declaredType = ElementType.MakeArrayType();
				}
				return _declaredType;
			}
		}

		/// <summary>
		/// Returns true if the <see cref="Items"/> enumerable is empty.
		/// </summary>
		public override bool UseFallback
		{
			get
			{
				return !Items.Any();
			}
		}

		/// <summary>
		/// Gets the rezolve targets that will build each individual item in the resulting list or array.
		/// </summary>
		/// <value>The items.</value>
		public IEnumerable<ITarget> Items
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the target will build an array (<c>true</c>) or a list (<c>false</c>).
		/// </summary>
		public bool AsArray { get; private set; }

		private Type _declaredType;
		//note - only initialised if _declaredType is typeof(List<ElementType>)
		private ConstructorInfo _listConstructor;

		/// <summary>
		/// Gets the list constructor to be invoked when <see cref="AsArray"/> is false.
		/// </summary>
		/// <value>The list constructor.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Since this target is configured to build an array, getting the ListConstructor is not a valid operation.
		/// </exception>
		public ConstructorInfo ListConstructor
		{
			get
			{
				if (_listConstructor == null)
				{
					if (DeclaredType == typeof(List<>).MakeGenericType(ElementType))
					{
						_listConstructor = TypeHelpers.GetConstructor(DeclaredType, new[] { typeof(IEnumerable<>).MakeGenericType(ElementType) });
						if (_listConstructor == null)
							throw new InvalidOperationException(string.Format("Fatal error: Could not get IEnumerable<{0}> constructor for List<{0}>", ElementType));
					}
					else
						throw new InvalidOperationException("Since this target is configured to build an array, getting the ListConstructor is not a valid operation.");
				}
				return _listConstructor;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListTarget" /> class.
		/// </summary>
		/// <param name="elementType">Required. Type of the elements in the array or list.</param>
		/// <param name="items">Required. The targets that will create each the individual items.</param>
		/// <param name="asArray">A boolean indicating whether the target will build an array (<c>true</c>) or a list (<c>false</c>).</param>
		/// <exception cref="System.ArgumentNullException">
		/// elementType
		/// or
		/// items
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// All targets in the items enumerable must be non-null;items
		/// or
		/// All targets in the items enumerable must support the element type <paramref name="elementType"/></exception>
		public ListTarget(Type elementType, IEnumerable<ITarget> items, bool asArray = false)
		{
			elementType.MustNotBeNull(nameof(elementType));
			items.MustNotBeNull(nameof(items));
			items.MustNot(ts => ts.Any(t => t == null), "All targets in the items enumerable must be non-null", nameof(items));
			items.MustNot(ts => ts.Any(t => !t.SupportsType(elementType)), $"All targets in the items enumerable must support the element type { elementType }", nameof(items));

			ElementType = elementType;
			Items = items;
			AsArray = asArray;
		}
	}
}
