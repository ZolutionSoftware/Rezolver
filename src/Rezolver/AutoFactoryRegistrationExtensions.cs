// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver
{
	using System;

	public static partial class AutoFactoryRegistrationExtensions
	{
		/// <summary>Enables the automatic injection of a <see cref="Func{TResult}" /> for the given <typeparamref name="TResult" /></summary>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.
		/// </remarks>
		public static void RegisterAutoFunc<TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<>).MakeGenericType(typeof(TResult))
			RegisterAutoFactory<Func<TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,>).MakeGenericType(typeof(T1), typeof(TResult))
			RegisterAutoFactory<Func<T1, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T10">Type of the 10th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T10">Type of the 10th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T11">Type of the 11th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T10">Type of the 10th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T11">Type of the 11th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T12">Type of the 12th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T10">Type of the 10th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T11">Type of the 11th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T12">Type of the 12th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T13">Type of the 13th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T10">Type of the 10th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T11">Type of the 11th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T12">Type of the 12th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T13">Type of the 13th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T14">Type of the 14th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T10">Type of the 10th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T11">Type of the 11th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T12">Type of the 12th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T13">Type of the 13th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T14">Type of the 14th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T15">Type of the 15th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>(targets);
		}

		/// <summary>Enables the automatic injection of a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}" /> auto-factory for the given 
		/// <typeparamref name="TResult" /> where one or more dependencies which would usually be resolved from the container 
		/// will instead be supplied by the code which calls the delegate.</summary>
		/// <typeparam name="T1">Type of the 1st parameter of the delegate.</typeparam>
		/// <typeparam name="T2">Type of the 2nd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T3">Type of the 3rd parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T4">Type of the 4th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T5">Type of the 5th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T6">Type of the 6th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T7">Type of the 7th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T8">Type of the 8th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T9">Type of the 9th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T10">Type of the 10th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T11">Type of the 11th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T12">Type of the 12th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T13">Type of the 13th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T14">Type of the 14th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T15">Type of the 15th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="T16">Type of the 16th parameter of the delegate.  This must not be the same type as any other parameter types.</typeparam>
		/// <typeparam name="TResult">The return type of the delegate - equivalent to the service type that is to be resolved from the container when the delegate is called.</typeparam>
		/// <param name="targets">Required.  The <see cref="IRootTargetContainer" /> into which the newly created target will be registered</param>
		/// <remarks>A parameterised auto-factory provides a way both to hide the container from application code, but also 
		/// to allow dependencies to be supplied to the requested service instead of relying on the container to have registrations
		/// for it.
		///
		/// Note that scoping is honoured for the delegate call; with an injected auto-factory being bound to the scope from which
		/// it is resolved.
		/// 
		/// This methods ensures that enumerables of the delegate type are also injectable.</remarks>
		public static void RegisterAutoFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this IRootTargetContainer targets)
		{
			// typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16), typeof(TResult))
			RegisterAutoFactory<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>(targets);
		}
	}
}

