using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Represents the context in which a target is being compiled or fetched.
	/// 
	/// The class implements the <see cref="ITargetContainer"/> interface also to facilitate
	/// dependency lookups during compilation time.
	/// 
	/// THIS CLASS IS NOT THREAD-SAFE
	/// 
	/// TODO: Wondering if RezolveContext should be passed into the CompileContext...  Everything tells me it should.
	/// </summary>
	public class CompileContext : ITargetContainer
	{
		/// <summary>
		/// Key for a shared expression used during expression tree generation
		/// </summary>
		public class SharedExpressionKey : IEquatable<SharedExpressionKey>
		{
			public Type RequestingType { get; private set; }
			public Type TargetType { get; private set; }
			public string Name { get; private set; }

			public SharedExpressionKey(Type targetType, string name, Type requestingType = null)
			{
				targetType.MustNotBeNull("targetType");
				name.MustNotBeNull("name");
				TargetType = targetType;
				Name = name;
				RequestingType = requestingType;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				return base.Equals(obj as SharedExpressionKey);
			}

			public override int GetHashCode()
			{
				return TargetType.GetHashCode() ^
					Name.GetHashCode() ^
					(RequestingType != null ? RequestingType.GetHashCode() : 0);
			}

			public bool Equals(SharedExpressionKey other)
			{
				return object.ReferenceEquals(this, other) ||
					(RequestingType == other.RequestingType && TargetType == other.TargetType && Name == other.Name);
			}
		}

		/// <summary>
		/// The rezolver that is considered the current compilation 'scope'.
		/// 
		/// You should not use this to look up other targets
		/// for dependency resolution - for that, you should use the DependencyBuilder
		/// </summary>
		public IContainer Rezolver { get; }

		private Expression _rezolverExpression;
		/// <summary>
		/// Represents an expression that equals the <see cref="Rezolver"/> that belongs to this context.
		/// </summary>
		public Expression RezolverExpression {
			get
			{
				if (_rezolverExpression == null)
					_rezolverExpression = Expression.Constant(Rezolver, typeof(IContainer));
				return _rezolverExpression;
			}
			private set
			{
				//class can overwrite this value itself if needs be
				_rezolverExpression = value;
			}
		}

		private readonly Type _targetType;
		/// <summary>
		/// The desired type to be returned by the generated code.
		/// </summary>
		public Type TargetType { get { return _targetType; } }

		private readonly ParameterExpression _rezolveContextParameter;

		/// <summary>
		/// A expression to be used to bind to the RezolveContext that will be passed to the generated code at runtime.
		/// 
		/// If this is never set, then the framework will use <see cref="ExpressionHelper.RezolveContextParameter"/> by default.
		/// 
		/// In theory, you should never need to set this to anything else, unless you're doing something very interesting with
		/// the generated expressions.
		/// </summary>
		public ParameterExpression RezolveContextParameter { get { return _rezolveContextParameter ?? ExpressionHelper.RezolveContextParameter; } }

		private readonly Stack<ITarget> _compilingTargets;


		private MemberExpression _contextRezolverPropertyExpression;
		/// <summary>
		/// Returns an expression that represents reading the Rezolver property of the <see cref="RezolveContextParameter"/> 
		/// to aid in code generation.
		/// 
		/// This IS NOT the same as the <see cref="RezolverExpression"/> property.
		/// 
		/// This is always non-null.
		/// </summary>
		public MemberExpression ContextRezolverPropertyExpression
		{
			get
			{
				if (_contextRezolverPropertyExpression == null)
				{
					_contextRezolverPropertyExpression = Expression.Property(RezolveContextParameter, "Rezolver");
				}
				return _contextRezolverPropertyExpression;
			}
		}

		private MemberExpression _contextScopePropertyExpression;
		/// <summary>
		/// Returns an expression that represents reading the Scope property of the <see cref="RezolveContext"/>
		/// </summary>
		public MemberExpression ContextScopePropertyExpression
		{
			get
			{
				if (_contextScopePropertyExpression == null)
					_contextScopePropertyExpression = Expression.Property(RezolveContextParameter, "Scope");
				return _contextScopePropertyExpression;
			}
		}

		//TODO: add property expression getters for the other properties on RezolveContext.

		/// <summary>
		/// An enumerable representing the current stack of targets that are being compiled.
		/// 
		/// The underlying stack is not exposed through this enumerable.
		/// </summary>
		public IEnumerable<ITarget> CompilingTargets { get { return _compilingTargets.AsReadOnly(); } }

		private Dictionary<SharedExpressionKey, Expression> _sharedExpressions;

		/// <summary>
		/// Shared expressions are expressions that targets add to the compile context as they are compiled,
		/// enabling them to generate code which is both more efficient at runtime (e.g. avoiding the creation of
		/// redundant locals for blocks which can reuse a pre-existing local) and that can be more efficiently rewritten 
		/// - due to the reuse of identical expression references for things like conditional checks and so on.
		/// 
		/// A compiler MUST handle the case where this enumerable contains ParameterExpressions, as they will need
		/// to be added as local variables to an all-encompassing BlockExpression around the root of an expression tree 
		/// that is to be compiled.
		/// </summary>
		public IEnumerable<Expression> SharedExpressions
		{
			get
			{
				return _sharedExpressions.Values;
			}
		}

		private readonly bool _suppressScopeTracking;

		/// <summary>
		/// If true, then any target that is compiling within this scope should not generate any runtime code to fetch the
		/// object from, or track the object in, the current scope.
		/// </summary>
		/// <remarks>This is currently used, for example, by wrapper targets that generate their own
		/// scope tracking code (specifically, the <see cref="SingletonTarget"/> and <see cref="ScopedTarget"/>.
		/// 
		/// It's therefore very important that any custom <see cref="ITarget"/> implementations honour this flag in their
		/// implementation of <see cref="ITarget.CreateExpression(CompileContext)"/>.  The <see cref="TargetBase"/>
		/// class does honour this flag.</remarks>
		public bool SuppressScopeTracking
		{
			get
			{
				return _suppressScopeTracking;
			}
		}

		/// <summary>
		/// This is the IRezolveTargetContainer through which dependencies are resolved.
		/// Note that this class implements IRezolveTargetContainer by proxying this instance, 
		/// which is, by default, created as a child rezolver builder of the one that
		/// is attached to the <see cref="Rezolver"/>
		/// </summary>
		private ITargetContainer _rezolverBuilder;

		private CompileContext(CompileContext parentContext, bool inheritSharedExpressions, bool suppressScopeTracking)
		{
			parentContext.MustNotBeNull("parentContext");

			Rezolver = parentContext.Rezolver;
			RezolverExpression = parentContext.RezolverExpression;
			_compilingTargets = parentContext._compilingTargets;
			_rezolveContextParameter = parentContext._rezolveContextParameter;
			
			_rezolverBuilder = new ChildBuilder(parentContext._rezolverBuilder);
			_sharedExpressions = inheritSharedExpressions ? parentContext._sharedExpressions : new Dictionary<SharedExpressionKey, Expression>();
			_suppressScopeTracking = suppressScopeTracking;
		}

		/// <summary>
		/// Creates a new CompileContext
		/// </summary>
		/// <param name="rezolver">Required. Will be set into the <see cref="Rezolver"/> property.</param>
		/// <param name="targetType">Optional. Will be set into the <see cref="TargetType"/> property.</param>
		/// <param name="rezolveContextParameter">Optional.  Will be set into the <see cref="RezolveContextParameter"/></param>
		/// <param name="compilingTargets">Optional.  Allows you to seed the stack of compiling targets from creation.</param>
		/// <param name="dependencyContainer">Optional.  By default, this class implements <see cref="ITargetContainer"/> by wrapping
		/// a <see cref="ChildBuilder"/> over the one that is attached to the <paramref name="rezolver"/> that you pass on construction.
		/// 
		/// To override that behaviour, you can pass a different target container here.
		/// 
		/// You can, for example, disable the child rezolver creation by explicitly passing the <see cref="IContainer.Builder"/>
		/// property of the <paramref name="rezolver"/> parameter.</param>
		public CompileContext(IContainer rezolver,
			Type targetType = null,
			ParameterExpression rezolveContextParameter = null,
			IEnumerable<ITarget> compilingTargets = null,
			ITargetContainer dependencyContainer = null)
		{
			rezolver.MustNotBeNull("rezolver");

			Rezolver = rezolver;
			_rezolverBuilder = dependencyContainer ?? new ChildBuilder(Rezolver.Builder);
			_targetType = targetType;
			_rezolveContextParameter = rezolveContextParameter;
			_compilingTargets = new Stack<ITarget>(compilingTargets ?? Enumerable.Empty<ITarget>());
			_sharedExpressions = new Dictionary<SharedExpressionKey, Expression>();
		}

		/// <summary>
		/// Creates a new CompileContext using an existing one as a template.
		/// </summary>
		/// <param name="parentContext">Used to seed the compilation stack, rezolver, rezolve context parameter and optionally
		/// the target type (if you pass null for <paramref name="targetType"/>.</param>
		/// <param name="targetType">The target type that is expected to be compiled, or null to inherit
		/// the <paramref name="parentContext"/>'s <see cref="CompileContext.TargetType"/> property.</param>
		/// <param name="inheritSharedExpressions">If true, then the <see cref="SharedExpressions"/> for this context will be shared
		/// from the parent context - meaning that any new additions will be added back to the parent context again.  The default is
		/// false, however if you are chaining multiple targets' expressions together you will need to pass true.</param>
		/// <param name="suppressScopeTracking">If true, then any expressions constructed from <see cref="ITarget"/> objects
		/// should not contain automatically generated code to track objects in an enclosing scope.  The default is false.  This is 
		/// typically only enabled when one target is explicitly using expressions created from other targets, and has its own
		/// scope tracking code, or expects to be surrounded by automatically generated scope tracking code itself.</param>
		public CompileContext(CompileContext parentContext, Type targetType = null, bool inheritSharedExpressions = false, bool suppressScopeTracking = false)
			: this(parentContext, inheritSharedExpressions, suppressScopeTracking)
		{
			_targetType = targetType ?? parentContext.TargetType;
		}

		/// <summary>
		/// Retrieves
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="requestingType"></param>
		/// <returns></returns>
		public ParameterExpression GetOrAddSharedLocal(Type type, string name, Type requestingType = null)
		{
			try
			{
				return (ParameterExpression)GetOrAddSharedExpression(type, name, () => Expression.Parameter(type, name), requestingType);
			}
			catch (InvalidCastException)
			{
				throw new InvalidOperationException("Cannot add ParameterExpression: A shared expression of a different type has already been added with the same type and name.");
			}
		}

		public Expression GetOrAddSharedExpression(Type type, string name, Func<Expression> expressionFactory, Type requestingType = null)
		{
			type.MustNotBeNull("type");
			expressionFactory.MustNotBeNull("expressionFactory");
			Expression toReturn;
			//if this is 
			SharedExpressionKey key = new SharedExpressionKey(type, name, requestingType);
			if (!_sharedExpressions.TryGetValue(key, out toReturn))
				_sharedExpressions[key] = toReturn = expressionFactory();
			return toReturn;
		}

		/// <summary>
		/// Adds the target to the compilation stack if it doesn't already exist.
		/// 
		/// The method returns whether the target was added.
		/// </summary>
		/// <param name="toCompile"></param>
		/// <returns></returns>
		public bool PushCompileStack(ITarget toCompile)
		{
			toCompile.MustNotBeNull("toCompile");

			if (!_compilingTargets.Contains(toCompile))
			{
				_compilingTargets.Push(toCompile);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Call this to find out if a target is currently compiling without trying
		/// to also add it to the stack.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsCompiling(ITarget target)
		{
			return _compilingTargets.Contains(target);
		}

		/// <summary>
		/// Pops a target from the stack and returns it.  Note that if there
		/// are no targets on the stack, an InvalidOperationException will occur.
		/// </summary>
		/// <returns></returns>
		public ITarget PopCompileStack()
		{
			return _compilingTargets.Pop();
		}

		public void Register(ITarget target, Type serviceType = null)
		{
			_rezolverBuilder.Register(target, serviceType);
		}

		public ITarget Fetch(Type type)
		{
			return _rezolverBuilder.Fetch(type);
		}

		public IEnumerable<ITarget> FetchAll(Type type)
		{
			return _rezolverBuilder.FetchAll(type);
		}

		public ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
		}
	}
}
