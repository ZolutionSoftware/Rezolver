using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// This exists primarily for the configuration system.
	/// 
	/// This target produces arrays or lists whose individual items are built by IRezolveTarget instances.  
	/// If you are setting up your rezolver environment from code - e.g. using expressions,
	/// then you shouldn't need to use this unless you have some pretty extreme requirements for the different
	/// targets that will contribute each element of your list/array.
	/// </summary>
	/// <remarks>
	/// The element type you feed on construction determines the type of array or the generic argument to List&lt;T&gt;.
	/// 
	/// Each of the rezolve targets that you then pass must support that type.
	/// 
	/// You can control whether the created instance is an array or a List&lt;T&gt; with the 'asArray' boolean constructor argument.
	/// </remarks>
	public class ListTarget : RezolveTargetBase
	{
		/// <summary>
		/// Gets the declared type of each element in the array or list that will be constructed.
		/// 
		/// The <see cref="DeclaredType"/> returned by this instance will either be ElementType[] or List&lt;ElementType&gt;
		/// depending on the value of <see cref="AsArray"/>.
		/// </summary>
		/// <value>The declared type of each element.</value>
		public Type ElementType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the rezolve targets that will build each individual item in the resulting list or array.
		/// </summary>
		/// <value>The items.</value>
		public IEnumerable<IRezolveTarget> Items
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
		/// Gets the list constructor to be used in generating the new List expression when <see cref="AsArray"/> is true.
		/// </summary>
		/// <value>The list constructor.</value>
		/// <exception cref="System.InvalidOperationException">
		/// Since this target is configured to build an array, getting the ListConstructor is not a valid operation.
		/// </exception>
		private ConstructorInfo ListConstructor
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
		public ListTarget(Type elementType, IEnumerable<IRezolveTarget> items, bool asArray = false)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");
			if (items == null)
				throw new ArgumentNullException("items");
			if (items.Any(i => i == null))
				throw new ArgumentException("All targets in the items enumerable must be non-null", "items");
			if (items.Any(i => !i.SupportsType(elementType)))
				throw new ArgumentException(string.Format("All targets in the items enumerable must support the element type {0}", elementType), "items");

			ElementType = elementType;
			Items = items;
			AsArray = asArray;
		}

		/// <summary>
		/// Constructs an expression that represents building an array of <see cref="ElementType"/> or List&lt;<see cref="ElementType"/>&gt;
		/// depending on the value of <see cref="AsArray"/>.
		/// </summary>
		/// <param name="context">The current compile context</param>
		/// <returns>System.Linq.Expressions.Expression.</returns>
		protected override System.Linq.Expressions.Expression CreateExpressionBase(CompileContext context)
		{
			//either way we're always going to be building an array.  If AsArray is true, we'll just return that, 
			//otherwise we pass that to a List<T> constructor.
			var arrayExpr = Expression.NewArrayInit(ElementType,
				Items.Select(t => t.CreateExpression(new CompileContext(context, ElementType, true))));

			if (AsArray)
				return arrayExpr;
			else
				return Expression.New(ListConstructor, arrayExpr);
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
	}
}
