﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    internal static class CompileContextExtensions
    {
		/// <summary>
		/// Fetches an object of the type <typeparamref name="TObj"/> directly from the 
		/// <see cref="ITargetContainer"/> represented by the <paramref name="context"/>.
		/// 
		/// See the remarks for more.
		/// </summary>
		/// <typeparam name="TObj">The type of object to be fetched.</typeparam>
		/// <param name="context">The context.</param>
		/// <remarks>This does not use the <see cref="IContainer"/> on the <paramref name="context"/>,
		/// but rather looks for an <see cref="ITarget"/> through the <see cref="ITargetContainer"/> 
		/// implementation of the <paramref name="context"/> that is registered for the target type 
		/// and then checks if that target is also an instance of <typeparamref name="TObj"/>.
		/// If it is, it simply casts and returns the target.
		/// 
		/// If not, then it also checks whether the target also supports the <see cref="ICompiledTarget"/>
		/// interface and, if so, it will call its <see cref="ICompiledTarget.GetObject(ResolveContext)"/>
		/// with a <see cref="ResolveContext"/> whose <see cref="ResolveContext.RequestedType"/> is set
		/// to the type <typeparamref name="TObj"/> - and return the result.
		/// 
		/// The function therefore allows infrastructure components to leverage the <see cref="ITargetContainer"/>
		/// available at compile time as a service provider for supporting objects during compilation - so long 
		/// as those objects also implement the <see cref="ITarget"/> or <see cref="ICompiledTarget"/> interfaces.
		/// 
		/// The concept is therefore best suited for pre-built objects (or singletons if you will).
		/// 
		/// The built-in <see cref="ObjectTarget"/> class is ideal as that does implement <see cref="ICompiledTarget"/>,
		/// returning its inner <see cref="ObjectTarget.Value"/> in its implementation of 
		/// <see cref="ICompiledTarget.GetObject(ResolveContext)"/> if the requested type is compatible with the type
		/// of the underlying object.
		/// 
		/// Note that the function returns null if no object can be obtained.
		/// 
		/// Exceptions may occur if the function falls down to using the <see cref="ICompiledTarget"/> route,
		/// since the <see cref="ICompiledTarget.GetObject(ResolveContext)"/> implementation might have unknown
		/// requirements.</remarks>
		internal static TObj FetchDirect<TObj>(this ICompileContext context)
		{
			return (TObj)context.FetchDirect(typeof(TObj));
		}

		/// <summary>
		/// Non generic version of <see cref="FetchDirect{TObj}(ICompileContext)"/>.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="objectType">Type of the object required.</param>
		internal static object FetchDirect(this ICompileContext context, Type objectType)
		{
			var result = context.Fetch(objectType);
			if(result != null)
				return FetchDirect(result, context, objectType);
			return result;
		}

		private static object FetchDirect(ITarget target, ICompileContext context, Type objectType)
		{
			if (TypeHelpers.IsAssignableFrom(objectType, target.GetType()))
				return target;

			ICompiledTarget resultCompiledTarget = target as ICompiledTarget;
			if (resultCompiledTarget != null)
				return (IExpressionCompiler)resultCompiledTarget.GetObject(new ResolveContext(context.Container, objectType));

			return null;
		}

		/// <summary>
		/// Similar to <see cref="FetchDirect{TObj}(ICompileContext)"/> except this enumerates all registered targets, returning
		/// any which are either of the type <typeparamref name="TObj"/> or which implement <see cref="ICompiledTarget"/> and return
		/// a value when <see cref="ICompiledTarget.GetObject(ResolveContext)"/> is called.
		/// 
		/// Note that the function never returns a null enumerable - the enumerable either has items in it or it doesn't.
		/// </summary>
		/// <typeparam name="TObj">The type of object to retrieve.</typeparam>
		/// <param name="context">The context.</param>
		internal static IEnumerable<TObj> FetchAllDirect<TObj>(this ICompileContext context)
		{
			return context.FetchAll(typeof(TObj))
				.Select(t => FetchDirect(t, context, typeof(TObj)))
				.Where(r => r != null)
				.OfType<TObj>();
		}

		/// <summary>
		/// Non generic version of <see cref="FetchAllDirect{TObj}(ICompileContext)"/>.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="objectType">Type of the objects required.</param>
		internal static IEnumerable<object> FetchAllDirect(this ICompileContext context, Type objectType)
		{
			return context.FetchAll(objectType)
				.Select(t => FetchDirect(t, context, objectType))
				.Where(r => r != null);
		}
	}
}
