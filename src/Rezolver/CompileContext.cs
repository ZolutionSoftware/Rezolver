// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Provides support and compile-time state for the compilation of an <see cref="ITarget"/> by an <see cref="ITargetCompiler"/>.
	/// THIS CLASS IS NOT THREAD-SAFE
	/// </summary>
	/// <remarks>The purpose of this class is to help an <see cref="ITarget"/> generate an expression tree that will ultimately be
	/// compiled using <see cref="ITargetCompiler.CompileTarget(ITarget, CompileContext)"/> method.  The goal being to produce 
	/// an <see cref="ICompiledTarget"/> whose <see cref="ICompiledTarget.GetObject(RezolveContext)"/> method will be called to get
	/// instances when a container's <see cref="IContainer.Resolve(RezolveContext)"/> method is invoked.
	/// 
	/// Therefore, the context of the expression tree being generated is a method which takes a single <see cref="RezolveContext"/> parameter - hence
	/// the expressions exposed by this type (e.g. <see cref="RezolveContextExpression"/>) are there to help generate code
	/// that will work in that context.
	/// 
	/// Working directly with this, or any other code connected with the expression tree/compilation process, is an advanced topic!
	/// 
	/// The class implements the <see cref="ITargetContainer"/> interface also to extend
	/// dependency lookups during compilation time.  Indeed, if you are developing your own 
	/// <see cref="ITarget"/> implementation and need to resolve any dependencies from an <see cref="ITargetContainer"/>
	/// during compilation, it should be done through the context's implementation of ITargetContainer.
	/// </remarks>
	public class CompileContext : ITargetContainer
	{
		/// <summary>
		/// Represents an 
		/// </summary> 
		public class CompileStackEntry : IEquatable<CompileStackEntry>
		{
			public ITarget Target { get; }
			public Type CompilingType { get; }


			public CompileStackEntry(ITarget target, Type compilingType)
			{
				Target = target;
				CompilingType = compilingType;
			}

			public bool Equals(CompileStackEntry other)
			{
				if (other == null) return false;
				return object.ReferenceEquals(Target, other.Target) && CompilingType.Equals(other.CompilingType);
			}
		}
		/// <summary>
		/// Key for a shared expression used during expression tree generation.  As a consumer of this library, you
		/// are unlikely ever to need to use it.
		/// </summary>
		public class SharedExpressionKey : IEquatable<SharedExpressionKey>
		{
			/// <summary>
			/// Gets the type that registered the shared expression
			/// </summary>
			public Type RequestingType { get; private set; }
			/// <summary>
			/// The intended type of the expression that is cached by this key.
			/// </summary>
			public Type TargetType { get; private set; }
			/// <summary>
			/// Gets the name used for expressions that are cached using this key.
			/// </summary>
			/// <value>The name.</value>
			public string Name { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="SharedExpressionKey"/> class.
			/// </summary>
			/// <param name="targetType">Required. Eventual runtime type of the object produced by the expression that will be cached using this key.</param>
			/// <param name="name">Required. The name used for storing and retrieving expressions cached with this key.</param>
			/// <param name="requestingType">The type (e.g. the runtime type of an <see cref="ITarget"/> implementation) whose compilation requires the cached expression.</param>
			public SharedExpressionKey(Type targetType, string name, Type requestingType = null)
			{
				targetType.MustNotBeNull("targetType");
				name.MustNotBeNull("name");
				TargetType = targetType;
				Name = name;
				RequestingType = requestingType;
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				return base.Equals(obj as SharedExpressionKey);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
			public override int GetHashCode()
			{
				return TargetType.GetHashCode() ^
				  Name.GetHashCode() ^
				  (RequestingType != null ? RequestingType.GetHashCode() : 0);
			}

			/// <summary>
			/// Indicates whether the current object is equal to another object of the same type.
			/// </summary>
			/// <param name="other">An object to compare with this object.</param>
			/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
			public bool Equals(SharedExpressionKey other)
			{
				return object.ReferenceEquals(this, other) ||
				  (RequestingType == other.RequestingType && TargetType == other.TargetType && Name == other.Name);
			}
		}

		/// <summary>
		/// The container that is considered the current compilation 'scope' - i.e. the container for which the compilation
		/// is being performed and, usually, the one on which the <see cref="IContainer.Resolve(RezolveContext)"/> method was 
		/// originally called which triggered the compilation call.
		/// </summary>
		/// <remarks>
		/// NOTE - For compile-time dependency resolution (i.e. other <see cref="ITarget"/>s) you should use this class' implementation
		/// of <see cref="ITargetContainer"/>.
		/// </remarks>
		public IContainer Container { get; }

		private Expression _containerExpression;

		/// <summary>
		/// A ConstantExpression that equals the <see cref="Container"/> that is active for this context - you can use this during code generation
		/// to alter your expression's behaviour if the <see cref="RezolveContext.Container"/> during a future call to <see cref="IContainer.Resolve(RezolveContext)"/>
		/// is different from the one for which the expression was first compiled.
		/// 
		/// For this kind of behaviour you will also need to use the <see cref="RezolveContextExpression"/>
		/// </summary>
		public Expression ContainerExpression
		{
			get
			{
				if (_containerExpression == null)
					_containerExpression = Expression.Constant(Container, typeof(IContainer));
				return _containerExpression;
			}
			private set
			{
				//class can overwrite this value itself if needs be
				_containerExpression = value;
			}
		}

		private readonly Type _targetType;
		/// <summary>
		/// The type that is to be returned by the generated code.
		/// </summary>
		public Type TargetType { get { return _targetType; } }

		private readonly ParameterExpression _rezolveContextExpression;

		/// <summary>
		/// An expression to be used to bind to the <see cref="RezolveContext"/> parameter be passed to the generated code at runtime
		/// (the context parameter for <see cref="IContainer.Resolve(RezolveContext)"/> and, eventually, <see cref="ICompiledTarget.GetObject(RezolveContext)"/>).
		/// </summary>
		/// <remarks>If this is never explicitly set, the framework uses <see cref="ExpressionHelper.RezolveContextParameterExpression"/> by default.
		/// 
		/// In theory, you should never need to set this to anything else, unless you're doing something very interesting with
		/// the generated expressions.</remarks>
		public ParameterExpression RezolveContextExpression { get { return _rezolveContextExpression ?? ExpressionHelper.RezolveContextParameterExpression; } }

		private readonly Stack<CompileStackEntry> _compilingTargets;


		private MemberExpression _contextContainerPropertyExpression;
		/// <summary>
		/// Returns an expression that represents reading the <see cref="RezolveContext.Container"/> property of the <see cref="RezolveContextExpression"/> 
		/// during the execution of an <see cref="ICompiledTarget"/>'s <see cref="ICompiledTarget.GetObject(RezolveContext)"/> method.
		/// </summary>
		/// <remarks>This IS NOT the same as the <see cref="ContainerExpression"/> property, which always returns a constant reference to
		/// the original <see cref="IContainer"/> for which this compilation context was created.
		/// 
		/// Always non-null.</remarks>
		public MemberExpression ContextContainerPropertyExpression
		{
			get
			{
				if (_contextContainerPropertyExpression == null)
					_contextContainerPropertyExpression = Expression.Property(RezolveContextExpression, nameof(RezolveContext.Container));

				return _contextContainerPropertyExpression;
			}
		}

		private MemberExpression _contextScopePropertyExpression;
		/// <summary>
		/// Returns an expression that represents reading the <see cref="RezolveContext.Scope"/> property of the <see cref="RezolveContext"/>
		/// referenced by the <see cref="RezolveContextExpression"/> at resolve-time.
		/// </summary>
		public MemberExpression ContextScopePropertyExpression
		{
			get
			{
				if (_contextScopePropertyExpression == null)
					_contextScopePropertyExpression = Expression.Property(RezolveContextExpression, nameof(RezolveContext.Scope));
				return _contextScopePropertyExpression;
			}
		}

		/// <summary>
		/// An enumerable representing the current stack of targets that are being compiled on this context.
		/// 
		/// The underlying stack is not exposed through this enumerable.
		/// </summary>
		public IEnumerable<CompileStackEntry> CompilingTargets { get { return _compilingTargets.AsReadOnly(); } }

		private Dictionary<SharedExpressionKey, Expression> _sharedExpressions;

		/// <summary>
		/// The expressions that have been registered by targets whilst creating expressions for compiled targets.
		/// </summary>
		/// <remarks>
		/// Shared expressions are expressions which targets add to the compile context as they are compiled,
		/// enabling them to generate code which is both more efficient at runtime (e.g. avoiding the creation of
		/// redundant locals for blocks which can reuse a pre-existing local) and which can be more efficiently rewritten 
		/// - due to the reuse of identical expression references for things like conditional checks and so on.
		/// 
		/// A compiler MUST handle the case where this enumerable contains ParameterExpressions, as they will need
		/// to be added as local variables to an all-encompassing BlockExpression around the root of an expression tree 
		/// that is to be compiled.
		/// </remarks>
		public IEnumerable<Expression> SharedExpressions
		{
			get
			{
				return _sharedExpressions.Values;
			}
		}

		private readonly bool _suppressScopeTracking;

		/// <summary>
		/// If true, then any target that is compiling within this context should not generate any runtime code to fetch the
		/// object from, or track the object in, the current <see cref="IScopedContainer"/> (identified by the <see cref="ContextScopePropertyExpression" />).
		/// </summary>
		/// <remarks>This is currently used, for example, by wrapper targets that generate their own
		/// scope tracking code (specifically, the <see cref="SingletonTarget"/> and <see cref="ScopedTarget"/>.
		/// 
		/// It's therefore very important that any custom <see cref="ITarget"/> implementations which intend to do their own
		/// scoping honour this flag in their implementation of <see cref="ITarget.CreateExpression(CompileContext)"/>.
		/// The <see cref="TargetBase"/> class does honour this flag.</remarks>
		public bool SuppressScopeTracking
		{
			get
			{
				return _suppressScopeTracking;
			}
		}

		/// <summary>
		/// This is the ITargetContainer through which dependencies are resolved by this context.
		/// 
		/// Note that this class implements ITargetContainer by proxying this instance, 
		/// which is, by default, created as a child container of the one that
		/// is attached to the <see cref="Container"/>
		/// </summary>
		private ITargetContainer _dependencyTargetContainer;

		private CompileContext(CompileContext parentContext, bool inheritSharedExpressions, bool suppressScopeTracking)
		{
			//TODO: Consider tracking parentContext in a local property to allow compilers to walk the compilation stack for things like
			//compile-time dynamic behaviours for targets based on the types which request them.
			//Note that any attempt to do this will also likely need to be mirrored at resolve-time wherever possible.
			parentContext.MustNotBeNull("parentContext");

			Container = parentContext.Container;
			ContainerExpression = parentContext.ContainerExpression;
			_compilingTargets = parentContext._compilingTargets;
			_rezolveContextExpression = parentContext._rezolveContextExpression;

			_dependencyTargetContainer = new ChildTargetContainer(parentContext._dependencyTargetContainer);
			_sharedExpressions = inheritSharedExpressions ? parentContext._sharedExpressions : new Dictionary<SharedExpressionKey, Expression>();
			_suppressScopeTracking = suppressScopeTracking;
		}

		/// <summary>
		/// Creates a new CompileContext
		/// </summary>
		/// <param name="container">Required. The container for which compilation is being performed.  Will be set into the <see cref="Container" /> property.</param>
		/// <param name="dependencyTargetContainer">Required - An <see cref="ITargetContainer" /> that contains the <see cref="ITarget" />s that
		/// will be required to complete compilation.
		/// Note - this argument is passed to a new <see cref="ChildTargetContainer" /> that is created and proxied by this class' implementation
		/// of <see cref="ITargetContainer" />.
		/// As a result, it's possible to register new targets directly into the context via the <see cref="Register(ITarget, Type)" /> method,
		/// without modifying the underlying targets in the container you pass.
		/// Some of the core <see cref="ITarget" />s exposed by this library take advantage of that functionality (notably, the <see cref="DecoratingTargetContainer" />).</param>
		/// <param name="targetType">Optional. Will be set into the <see cref="TargetType" /> property.  If not provided, then any code generated within this context
		/// should compile for the <see cref="ITarget.DeclaredType"/>.</param>
		/// <param name="rezolveContextExpression">Optional.  Will be set into the <see cref="RezolveContextExpression" /> property.  If not provided, then
		/// the <see cref="ExpressionHelper.RezolveContextParameterExpression"/> global reference will be used.</param>
		/// <param name="compilingTargets">Optional.  Allows you to seed the stack of compiling targets from creation.</param>
		public CompileContext(IContainer container,
		  ITargetContainer dependencyTargetContainer,
		  Type targetType = null,
		  ParameterExpression rezolveContextExpression = null,
		  IEnumerable<CompileStackEntry> compilingTargets = null
		  )
		{
			container.MustNotBeNull(nameof(container));
			dependencyTargetContainer.MustNotBeNull(nameof(dependencyTargetContainer));
			Container = container;
			_dependencyTargetContainer = new ChildTargetContainer(dependencyTargetContainer);
			_targetType = targetType;
			_rezolveContextExpression = rezolveContextExpression;
			_compilingTargets = new Stack<CompileStackEntry>(compilingTargets ?? Enumerable.Empty<CompileStackEntry>());
			_sharedExpressions = new Dictionary<SharedExpressionKey, Expression>();
		}

		/// <summary>
		/// Creates a new CompileContext using an existing one as a template.
		/// </summary>
		/// <param name="parentContext">Used to seed the compilation stack, container, rezolve context parameter and optionally
		/// the target type (if you pass null for <paramref name="targetType" />.</param>
		/// <param name="targetType">The target type that is expected to be compiled, or null to inherit
		/// the <paramref name="parentContext" />'s <see cref="CompileContext.TargetType" /> property.</param>
		/// <param name="inheritSharedExpressions">If true (the default), then the <see cref="SharedExpressions" /> for this context will be shared
		/// from the parent context - meaning that any new additions will be added back to the parent context again.  This is the most common
		/// behaviour when chaining multiple targets' expressions together.  Passing false for this parameter is only required in rare situations.</param>
		/// <param name="suppressScopeTracking">If true, then any expressions constructed from <see cref="ITarget" /> objects
		/// should not contain automatically generated code to track objects in an enclosing scope.  The default is false.  This is
		/// typically only enabled when one target is explicitly using expressions created from other targets, and has its own
		/// scope tracking code, or expects to be surrounded by automatically generated scope tracking code itself.</param>
		public CompileContext(CompileContext parentContext, Type targetType = null, bool inheritSharedExpressions = true, bool suppressScopeTracking = false)
		  : this(parentContext, inheritSharedExpressions, suppressScopeTracking)
		{
			_targetType = targetType ?? parentContext.TargetType;
		}

		/// <summary>
		/// Spawns a new context for the passed <paramref name="targetType" />, with everything else being inherited from this context by default.
		/// </summary>
		/// <param name="targetType">Required.  The type to be compiled.</param>
		/// <param name="inheritSharedExpressions">if set to <c>true</c> [inherit shared expressions].</param>
		/// <param name="suppressScopeTracking">if set to <c>true</c> [suppress scope tracking].</param>
		/// <returns>A new <see cref="CompileContext" /></returns>
		/// <remarks>This is a convenience method which simply wraps the <see cref="CompileContext.CompileContext(CompileContext, Type, bool, bool)" /> constructor,
		/// except in this method the <paramref name="targetType" /> is required.</remarks>
		public CompileContext New(Type targetType, bool inheritSharedExpressions = true, bool suppressScopeTracking = false)
		{
			targetType.MustNotBeNull(nameof(targetType));
			return new CompileContext(this, targetType, inheritSharedExpressions: inheritSharedExpressions, suppressScopeTracking: suppressScopeTracking);
		}

		/// <summary>
		/// Creates or retrieves a shared <see cref="ParameterExpression"/> with the given name and type, optionally registered for the given <paramref name="requestingType"/>.
		/// </summary>
		/// <param name="type">The runtime type of the ParameterExpression.</param>
		/// <param name="name">The runtime name of the ParameterExpression - and also the name used to retrieve it later.</param>
		/// <param name="requestingType">Optional - to avoid naming clashes with shared parameter expressions created by other targets, you can pass a type here
		/// (usually the runtime type of your <see cref="ITarget"/> implementation).</param>
		/// <remarks>If you use a shared <see cref="ParameterExpression"/> for a local in your expression trees, you are signifying that all of your targets which use
		/// that local, regardless of whether they represent different results at runtime, can safely store and retrieve whatever state they are tracking within
		/// without interfering with each other.</remarks>
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

		/// <summary>
		/// Gets or adds a shared expression (created by the <paramref name="expressionFactory"/> if it's not already cached) with the given name, type, optionally
		/// for the given <paramref name="requestingType"/>.
		/// </summary>
		/// <param name="type">The runtime type of the Expression.</param>
		/// <param name="name">The runtime name of the Expression - and also the name used to retrieve it later.</param>
		/// <param name="expressionFactory">The factory method to use to construct the shared expression from scratch, if it's not already cached.</param>
		/// <param name="requestingType">Optional - to avoid naming clashes with shared expressions created by other targets, you can pass a type here
		/// (usually the runtime type of your <see cref="ITarget"/> implementation).</param>
		/// <returns>Expression.</returns>
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
		/// </summary>
		/// <param name="toCompile">The target to be pushed</param>
		/// <returns>A boolean indicating whether the target was added.  Will be false if the target is already on the stack.</returns>
		public bool PushCompileStack(ITarget toCompile)
		{
			toCompile.MustNotBeNull("toCompile");
			CompileStackEntry entry = new CompileStackEntry(toCompile, this.TargetType);
			if (!_compilingTargets.Contains(entry))
			{
				_compilingTargets.Push(entry);
				return true;
			}
			return false;
		}

		///// <summary>
		///// Call this to find out if a target is currently compiling without trying to add it to the stack.
		///// </summary>
		///// <param name="target">The target to be checked</param>
		///// <returns><c>true</c> if the target has previously been added through a call to <see cref="PushCompileStack(ITarget)"/>, otherwise <c>false</c></returns>
		//public bool IsCompiling(ITarget target)
		//{
		//	return _compilingTargets.Contains(target);
		//}

		/// <summary>
		/// Pops a target from the stack and returns it.  Note that if there
		/// are no targets on the stack, an <see cref="InvalidOperationException"/> will occur.
		/// </summary>
		/// <returns></returns>
		public CompileStackEntry PopCompileStack()
		{
			return _compilingTargets.Pop();
		}

		/// <summary>
		/// Implements <see cref="ITargetContainer.Register(ITarget, Type)"/> by wrapping around the child target container created by this context on construction.
		/// </summary>
		/// <param name="target">See <see cref="ITargetContainer.Register(ITarget, Type)"/> for more</param>
		/// <param name="serviceType">See <see cref="ITargetContainer.Register(ITarget, Type)"/> for more</param>
		public void Register(ITarget target, Type serviceType = null)
		{
			_dependencyTargetContainer.Register(target, serviceType);
		}

		/// <summary>
		/// Implements <see cref="ITargetContainer.Fetch(Type)"/> by wrapping around the child target container created by this context on construction.
		/// </summary>
		/// <param name="type">See <see cref="ITargetContainer.Fetch(Type)"/> for more.</param>
		public ITarget Fetch(Type type)
		{
			if (typeof(IContainer) == type)
				return new ExpressionTarget(this.ContextContainerPropertyExpression);

			return _dependencyTargetContainer.Fetch(type);
		}

		/// <summary>
		/// Implements <see cref="ITargetContainer.FetchAll(Type)"/> by wrapping around the child target container created by this context on construction.
		/// </summary>
		/// <param name="type">See <see cref="ITargetContainer.FetchAll(Type)"/> for more</param>
		public IEnumerable<ITarget> FetchAll(Type type)
		{
			if (typeof(IContainer) == type)
				return new[] { new ExpressionTarget(this.ContextContainerPropertyExpression) };

			return _dependencyTargetContainer.FetchAll(type);
		}

		/// <summary>
		/// Always throws a <see cref="NotSupportedException"/>
		/// </summary>
		/// <param name="existing">Ignored</param>
		/// <param name="type">Ignored</param>
		/// <exception cref="NotSupportedException">Always thrown</exception>
		public ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
		}
	}
}
