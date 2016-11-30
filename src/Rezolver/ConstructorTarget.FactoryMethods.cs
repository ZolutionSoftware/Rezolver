using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
    public partial class ConstructorTarget
    {
		/// <summary>
		/// Generic version of the <see cref="Auto(Type, IPropertyBindingBehaviour)"/> method.
		/// </summary>
		/// <typeparam name="T">The type that is to be constructed when the new target is compiled and executed.</typeparam>
		/// <param name="propertyBindingBehaviour">See the documentation for the <paramref name="propertyBindingBehaviour"/> parameter
		/// on the non-generic version of this method.</param>
		/// <returns>Either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>, depending on whether
		/// <typeparamref name="T"/> is a generic type definition.</returns>
		public static ITarget Auto<T>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			return Auto(typeof(T), propertyBindingBehaviour);
		}

		/// <summary>
		/// Creates a late bound <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> for the 
		/// given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type that is to be constructed when this target is compiled and executed.</param>
		/// <param name="propertyBindingBehaviour">Optional.  An object which selects properties on the new instance which are
		/// to be bound from the container.</param>
		/// <returns>Either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>, depending on whether the 
		/// <paramref name="type"/> is a generic type definition.</returns>
		/// <remarks>This factory is merely a shortcut for calling the <see cref="ConstructorTarget.ConstructorTarget(Type, IPropertyBindingBehaviour, IDictionary{string, ITarget})"/>
		/// with only the <paramref name="type"/> and <paramref name="propertyBindingBehaviour"/> arguments supplied.  When creating a 
		/// <see cref="GenericConstructorTarget"/>, the function uses the <see cref="GenericConstructorTarget.GenericConstructorTarget(Type, IPropertyBindingBehaviour)"/> constructor.</remarks>
		public static ITarget Auto(Type type, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			//conduct a very simple search for the constructor with the most parameters
			type.MustNotBeNull(nameof(type));

			if (TypeHelpers.IsGenericTypeDefinition(type))
				return new GenericConstructorTarget(type, propertyBindingBehaviour);

			return new ConstructorTarget(type, propertyBindingBehaviour);
		}

		/// <summary>
		/// Non-generic version of <see cref="WithArgs{T}(IDictionary{string, ITarget})"/>.
		/// Creates a <see cref="ConstructorTarget"/> with a set of named targets which will be used like
		/// named arguments to late-bind the constructor when code-generation occurs.
		/// </summary>
		/// <param name="declaredType">The type whose constructor is to be bound.</param>
		/// <param name="namedArgs">The named arguments to be used when building the expression.</param>
		/// <remarks>Both versions of this method will create a target which will try to find the best-matching
		/// constructor where all of the named arguments match, and with the fewest number of auto-resolved
		/// arguments.
		/// 
		/// So, a class with a constructor such as 
		/// 
		/// <code>Foo(IService1 s1, IService2 s2)</code>
		/// 
		/// Can happily be bound if you only provide a named argument for 's1'; the target will simply
		/// attempt to auto-resolve the argument for the <code>IService2 s2</code> parameter when constructing 
		/// the object - and will fail only if it can't be resolved at that point.
		/// </remarks>
		public static ITarget WithArgs(Type declaredType, IDictionary<string, ITarget> namedArgs)
		{
			declaredType.MustNotBeNull("declaredType");
			namedArgs.MustNotBeNull("args");

			return WithArgsInternal(declaredType, namedArgs);
		}

		/// <summary>
		/// Performs the same operation as <see cref="WithArgs(Type, IDictionary{string, ITarget})"/> except the
		/// arguments are pulled from the publicly readable properties and fields of the passed <paramref name="namedArgs"/>
		/// object.
		/// </summary>
		/// <param name="declaredType">The type whose constructor is to be bound.</param>
		/// <param name="namedArgs">An object whose properties/fields provide the names and values for the argument
		/// which are to be used when binding the type's constructor.  Only properties and fields whose value is
		/// <see cref="ITarget"/> are considered.</param>
		/// <remarks>This overload exists to simplify the process of creating a ConstructorTarget with argument bindings
		/// by removing the need to create an argument dictionary in advance.  An anonymous type can instead be used
		/// to supply the arguments.</remarks>
		/// <example>This example shows how to provide an ObjectTarget for the parameter 'param1' when creating a
		/// ConstructorTarget for the type 'MyType':
		/// <code>ConstructorTarget.WithArgs(typeof(MyType), new { param1 = new ObjectTarget(&quot;Hello World&quot;) });</code></example>
		public static ITarget WithArgs(Type declaredType, object namedArgs)
		{
			declaredType.MustNotBeNull(nameof(declaredType));
			return WithArgsInternal(declaredType, namedArgs.ToMemberValueDictionary<ITarget>());
		}

		/// <summary>
		/// Similar to <see cref="WithArgs(Type, IDictionary{string, ITarget})"/> except this one creates
		/// a <see cref="ConstructorTarget"/> that is specifically bound to a particular constructor on a 
		/// given type, using any matched argument bindings from the provided <paramref name="namedArgs" /> dictionary,
		/// and using <see cref="RezolvedTarget"/> targets for any that are not matched.
		/// </summary>
		/// <param name="declaredType">Required.  Type of the object to be constructed.</param>
		/// <param name="ctor">Required. The constructor to be bound.</param>
		/// <param name="namedArgs">Optional. Any arguments to be supplied to parameters on the <paramref name="ctor"/>
		/// by name.  Any parameters for which matches are not found in this dictionary will be automatically
		/// bound either from compile-time defaults or by resolving those types dynamically.</param>
		/// <remarks>Although this overload accepts a dictionary of arguments, note that it will not
		/// result in the <see cref="NamedArgs"/> property being set on the target that is created - it's
		/// just an alternative for deriving the <see cref="ParameterBindings"/> with which the target
		/// will be created.
		/// 
		/// Also, this function will not fail if the args dictionary contains named arguments that cannot
		/// be matched to parameters on the <paramref name="ctor"/>.</remarks>
		public static ITarget WithArgs(ConstructorInfo ctor, IDictionary<string, ITarget> namedArgs)
		{
			ctor.MustNotBeNull("ctor");
			namedArgs = namedArgs ?? _emptyArgsDictionary;

			ParameterBinding[] bindings = ParameterBinding.BindMethod(ctor, namedArgs);

			return new ConstructorTarget(ctor, null, bindings);
		}

		/// <summary>
		/// Performs the same operation as <see cref="WithArgs(ConstructorInfo, IDictionary{string, ITarget})"/>
		/// except the arguments are pulled from the publicly readable properties and fields of the passed <paramref name="namedArgs"/>
		/// object.
		/// </summary>
		/// <param name="ctor">Required. The constructor to be bound.</param>
		/// <param name="namedArgs">An object whose properties/fields provide the names and values for the argument
		/// which are to be used when binding the type's constructor.  Only properties and fields whose value is
		/// <see cref="ITarget"/> are considered.</param>
		/// <remarks>Although this overload accepts a dictionary of arguments, note that it will not
		/// result in the <see cref="NamedArgs"/> property being set on the target that is created - it's
		/// just an alternative for deriving the <see cref="ParameterBindings"/> with which the target
		/// will be created.
		/// 
		/// Also, this function will not fail if the args dictionary contains named arguments that cannot
		/// be matched to parameters on the <paramref name="ctor"/>.</remarks>
		public static ITarget WithArgs(ConstructorInfo ctor, object namedArgs)
		{
			return WithArgs(ctor, namedArgs.ToMemberValueDictionary<ITarget>());
		}

		/// <summary>
		/// Creates a <see cref="ConstructorTarget"/> with a set of named targets which will be used like
		/// named arguments to late-bind the constructor when code-generation occurs.
		/// </summary>
		/// <typeparam name="T">The type whose constructor is to be bound</typeparam>
		/// <param name="namedArgs">The named arguments to be used when building the expression.</param>
		/// <remarks>Both versions of this method will create a target which will try to find the best-matching
		/// constructor where all of the named arguments match, and with the fewest number of auto-resolved
		/// arguments.
		/// 
		/// So, a class with a constructor such as 
		/// 
		/// <code>Foo(IService1 s1, IService2 s2)</code>
		/// 
		/// Can happily be bound if you only provide a named argument for 's1'; the target will simply
		/// attempt to auto-resolve the argument for the <code>IService2 s2</code> parameter when constructing 
		/// the object - and will fail only if it can't be resolved at that point.
		/// </remarks>
		public static ITarget WithArgs<T>(IDictionary<string, ITarget> namedArgs)
		{
			namedArgs.MustNotBeNull("args");

			return WithArgsInternal(typeof(T), namedArgs);
		}

		/// <summary>
		/// Performs the same operation as <see cref="WithArgs{T}(IDictionary{string, ITarget})"/> except the
		/// arguments are pulled from the publicly readable properties and fields of the passed <paramref name="namedArgs"/>
		/// object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="namedArgs">An object whose properties/fields provide the names and values for the argument
		/// which are to be used when binding the type's constructor.  Only properties and fields whose value is
		/// <see cref="ITarget"/> are considered.</param>
		/// <remarks>This overload exists to simplify the process of creating a ConstructorTarget with argument bindings
		/// by removing the need to create an argument dictionary in advance.  An anonymous type can instead be used
		/// to supply the arguments.</remarks>
		/// <example>This example shows how to provide an ObjectTarget for the parameter 'param1' when creating a
		/// ConstructorTarget for the type 'MyType':
		/// <code>ConstructorTarget.WithArgs&lt;MyType&gt;(new { param1 = new ObjectTarget(&quot;Hello World&quot;) });</code></example>
		public static ITarget WithArgs<T>(object namedArgs)
		{
			return WithArgsInternal(typeof(T), namedArgs.ToMemberValueDictionary<ITarget>());
		}

		internal static ITarget WithArgsInternal(Type declaredType, IDictionary<string, ITarget> namedArgs)
		{
			return new ConstructorTarget(declaredType, namedArgs: namedArgs);
		}

		/// <summary>
		/// Creates a new <see cref="ConstructorTarget" /> from the passed lambda expression (whose <see cref="LambdaExpression.Body" /> must be a <see cref="NewExpression" />)
		/// </summary>
		/// <typeparam name="T">The type of the object to be created by the new <see cref="ConstructorTarget" /></typeparam>
		/// <param name="newExpr">Required.  The expression from which to create the target.</param>
		/// <param name="adapter">Optional.  The adapter to be used to convert any additional expressions in the lambda into <see cref="ITarget" /> instances (e.g. for argument values).
		/// If not provided, then the <see cref="TargetAdapter.Default" /> will be used.</param>
		/// <returns>An <see cref="ITarget" /> that actually be an intstance of <see cref="ConstructorTarget"/></returns>
		/// <exception cref="ArgumentNullException">If <paramref name="newExpr"/> is null.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="newExpr"/> does not have a NewExpression as its root (Body) node, or if the type of
		/// that expression does not equal <typeparamref name="T"/>
		/// </exception>
		/// <remarks>This method does not support member binding expressions - e.g. <code>c => new MyObject() { A = "hello" }</code> - these can be converted into
		/// targets using a (compliant) <see cref="ITargetAdapter"/> object's <see cref="ITargetAdapter.CreateTarget(Expression)"/> method.  At least, the supplied
		/// <see cref="TargetAdapter"/> class does support them.
		/// 
		/// When providing custom expressions to be used as targets in an <see cref="ITargetContainer"/>, it is possible to explicitly define properties/arguments as
		/// being resolved from the container itself, in exactly the same way as generated by the other factory methods such as <see cref="Auto{T}(IPropertyBindingBehaviour)"/>
		/// and <see cref="SingletonTargetDictionaryExtensions.RegisterType{TObject}(ITargetContainer, IPropertyBindingBehaviour)"/>.  To do this, simply call the 
		/// <see cref="Functions.Resolve{T}"/> function on the object passed into your expression (see the signature of the lambda <paramref name="newExpr"/>),
		/// and Rezolver will convert that call into a <see cref="RezolvedTarget"/>.
		/// </remarks>
		public static ITarget FromNewExpression<T>(Expression<Func<T>> newExpr, ITargetAdapter adapter = null)
		{
			newExpr.MustNotBeNull(nameof(newExpr));
			NewExpression newExprBody = null;

			newExprBody = newExpr.Body as NewExpression;
			if (newExprBody == null)
				throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyIsNotNewExpressionFormat, newExpr), nameof(newExpr));
			else if (newExprBody.Type != typeof(T))
				throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyNewExpressionIsWrongTypeFormat, newExpr, typeof(T)), nameof(newExpr));

			return FromNewExpression(typeof(T), newExprBody, adapter);
		}

		/// <summary>
		/// Non-generic version of <see cref="FromNewExpression{T}(Expression{Func{T}}, ITargetAdapter)"/>.  See the documentation
		/// on that method for more.
		/// </summary>
		/// <param name="declaredType">The of the object to be created by the new <see cref="ConstructorTarget"/></param>
		/// <param name="newExpr"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
		public static ITarget FromNewExpression(Type declaredType, NewExpression newExpr, ITargetAdapter adapter = null)
		{
			ConstructorInfo ctor = null;
			ParameterBinding[] parameterBindings = null;

			ctor = newExpr.Constructor;
			parameterBindings = ExtractParameterBindings(newExpr, adapter ?? TargetAdapter.Default).ToArray();

			return new ConstructorTarget(ctor, null, parameterBindings);
		}

		private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr, ITargetAdapter adapter)
		{
			return newExpr.Constructor.GetParameters()
			  .Zip(newExpr.Arguments, (info, expression) => new ParameterBinding(info, adapter.CreateTarget(expression))).ToArray();
		}
	}
}
