﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public abstract partial class RezolveTargetCompilerTestsBase
	{
		public interface ITransient
		{
		}

		public class Transient : ITransient
		{
			public static int Counter = 0;

			public Transient()
			{
				++Counter;
			}
		}

		public interface ISingleton { }

		public class Singleton : ISingleton
		{
			public static int Counter = 0;

			public Singleton()
			{
				++Counter;
			}
		}

		public interface IComposite
		{
			ISingleton Singleton { get; }
			ITransient Transient { get; }
		}

		public class Composite : IComposite
		{
			private readonly ISingleton _singleton;
			private readonly ITransient _transient;

			public Composite(ISingleton singleton, ITransient transient)
			{
				_singleton = singleton;
				_transient = transient;
			}

			public ISingleton Singleton
			{
				get { return _singleton; }
			}

			public ITransient Transient
			{
				get { return _transient; }
			}
		}

		/// <summary>
		/// interface for the type that will be used to test the registration and compilation of all the different target types.
		/// </summary>
		public interface ISuperComplex
		{
			int Int { get; }
			int? NullableInt { get; }
			string String { get; }
			ITransient Transient { get; }
			ISingleton Singleton { get; }
			IComposite Composite { get; }
		}

		public class SuperComplex : ISuperComplex
		{
			public SuperComplex(int @int,
				int? nullableInt,
				string @string,
				ITransient transient,
				ISingleton singleton,
				IComposite composite)
			{
				Int = @int;
				NullableInt = nullableInt;
				String = @string;
				Transient = transient;
				Singleton = singleton;
				Composite = composite;
			}

			public int Int { get; private set; }
			public int? NullableInt { get; private set; }
			public string String { get; private set; }
			public ITransient Transient { get; private set; }
			public ISingleton Singleton { get; private set; }
			public IComposite Composite { get; private set; }
		}

		/// <summary>
		/// Special test target for the dynamic tests - returns the dynamic container that is passed
		/// into the delegate that is built from the target, or a default if that dynamic container is
		/// passed as null.
		/// </summary>
		public class DynamicTestTarget : IRezolveTarget
		{
			private readonly IRezolverContainer _defaultContainer;

			public DynamicTestTarget(IRezolverContainer defaultContainer)
			{
				_defaultContainer = defaultContainer;
			}

			public bool SupportsType(Type type)
			{
				return type.IsAssignableFrom(typeof(IRezolverContainer));
			}

			public Expression CreateExpression(IRezolverContainer containerScope, Type targetType = null,
				ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> currentTargets = null)
			{
				//this method isn't always called with a dynamicContainerExpression passed
				if (targetType != null && !SupportsType(targetType))
					throw new ArgumentException(string.Format("Type not supported: {0}", targetType));
				if (dynamicContainerExpression != null)
				{
					return Expression.Coalesce(Expression.Convert(dynamicContainerExpression, targetType ?? DeclaredType),
						Expression.Convert(Expression.Constant(_defaultContainer, typeof(IRezolverContainer)), targetType ?? DeclaredType));
				}

				return Expression.Convert(Expression.Constant(_defaultContainer, typeof(IRezolverContainer)), targetType ?? DeclaredType);
			}

			public Type DeclaredType
			{
				get { return typeof(IRezolverContainer); }
			}
		}
	}
}