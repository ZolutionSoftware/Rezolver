// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T9">The type of the 9th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T9">The type of the 9th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T10">The type of the 10th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T9">The type of the 9th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T10">The type of the 10th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T11">The type of the 11th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T9">The type of the 9th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T10">The type of the 10th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T11">The type of the 11th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T12">The type of the 12th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T9">The type of the 9th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T10">The type of the 10th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T11">The type of the 11th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T12">The type of the 12th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T13">The type of the 13th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T9">The type of the 9th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T10">The type of the 10th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T11">The type of the 11th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T12">The type of the 12th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T13">The type of the 13th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T14">The type of the 14th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	

	/// <summary>
	/// Extension of the <see cref="DelegateTarget"/> class which provides strong typing for the generic Func delegate type.
	/// </summary>
	/// <typeparam name="T1">The type of the 1st delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T2">The type of the 2nd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T3">The type of the 3rd delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T4">The type of the 4th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T5">The type of the 5th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T6">The type of the 6th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T7">The type of the 7th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T8">The type of the 8th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T9">The type of the 9th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T10">The type of the 10th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T11">The type of the 11th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T12">The type of the 12th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T13">The type of the 13th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T14">The type of the 14th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="T15">The type of the 15th delegate parameter (will be resolved automatically)</typeparam>
	/// <typeparam name="TResult">The type of object produced by the delegate.</typeparam>
	/// <remarks>
	/// This class and its other generic cousins exist purely to simplify the creation of a <see cref="DelegateTarget"/>
	/// with a delegate expressed as a lambda expression.
	/// 
	/// e.g. <code>new DelegateTarget&lt;IMyservice&gt;(() =&gt; new MyService());</code>
	/// 
	/// With the <see cref="DelegateTarget"/> it is not possible to do this - you need a delegate variable.
	/// 
	/// Ultimately, however, all functionality beyond construction is provided by the base class.
	/// </remarks>
	public class DelegateTarget<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> : DelegateTarget
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult}"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Type of the declared.</param>
		public DelegateTarget(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> factory, Type declaredType = null)
			: base(factory, declaredType)
		{

		}
	}	
}