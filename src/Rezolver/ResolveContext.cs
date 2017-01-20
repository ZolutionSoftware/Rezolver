// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// Captures the state for a call to <see cref="IContainer.Resolve(ResolveContext)"/> 
	/// (or <see cref="IContainer.TryResolve(ResolveContext, out object)"/>), including the container on which
	/// the operation is invoked, any <see cref="IScopedContainer"/> that might be active for the call (if different), and the 
	/// type which is being resolved from the <see cref="IContainer"/>.
	/// </summary>
	public class ResolveContext : IEquatable<ResolveContext>
	{
		private class StubContainer : IContainer
		{
			private static readonly StubContainer _instance = new StubContainer();

			public static StubContainer Instance
			{
				get
				{
					return _instance;
				}
			}

			public ITargetCompiler Compiler
			{
				get { throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set")); }
			}

			public ITargetContainer Targets
			{
				get { throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set")); }
			}

			public bool CanResolve(ResolveContext context)
			{
				throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set"));
			}

			public object Resolve(ResolveContext context)
			{
				context.MustNotBeNull("context");
				throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set"));
			}

			public bool TryResolve(ResolveContext context, out object result)
			{
				context.MustNotBeNull("context");
				throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set"));
			}

			public IScopedContainer CreateLifetimeScope()
			{
				throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set"));
			}

			public ICompiledTarget FetchCompiled(ResolveContext context)
			{
				throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set"));
			}

			public void Register(ITarget target, Type type = null)
			{
				throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set"));
			}

			public IScopedContainer CreateLifetimeScope(IContainer overridingRezolver)
			{
				throw new NotImplementedException();
			}

			object IServiceProvider.GetService(Type serviceType)
			{
				throw new InvalidOperationException(String.Format("The ResolveContext has no Rezolver set"));
			}
		}

		private Type _requestedType;
		/// <summary>
		/// Gets the type being requested from the container
		/// </summary>
		/// <value>The type of the requested.</value>
		public Type RequestedType { get { return _requestedType; } private set { _requestedType = value; } }

		private IContainer _container;

		/// <summary>
		/// The container for this context.
		/// </summary>
		/// <remarks>This is the container which received the original call to <see cref="IContainer.Resolve(ResolveContext)"/>,
		/// but is not necessarily the same container that will eventually end up resolving the object.</remarks>
		public IContainer Container { get { return _container; } private set { _container = value; } }

		private IScopedContainer _scope;
		/// <summary>
		/// Gets the scope in which the resolve operation is taking place.  Objects which are to be bound to a 
		/// lifetime scope will be retrieved or placed in here or one of the scope's 
		/// <see cref="IScopedContainer.ParentScope"/> hierarchy.
		/// </summary>
		/// <value>The scope.</value>
		public IScopedContainer Scope { get { return _scope; } private set { _scope = value; } }

		private ResolveContext() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ResolveContext"/> class.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		public ResolveContext(IContainer container, Type requestedType)
		  : this(container)
		{
			RequestedType = requestedType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResolveContext"/> class.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		/// <param name="scope">The scope for this context.</param>
		public ResolveContext(IContainer container, Type requestedType, IScopedContainer scope)
		  : this(container)
		{
			RequestedType = requestedType;
			Scope = scope;
		}

		private ResolveContext(IContainer container)
		{
			_container = container ?? StubContainer.Instance;
			//automatically inherit the container as this context's scope, if it's of the correct type.
			//note - all the other constructors chain to this one.  Other constructors
			//might supply a separate scope in addition, which will overwrite the scope set here.
			_scope = container as IScopedContainer;
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		public override string ToString()
		{
			List<string> parts = new List<string>();

			parts.Add($"Type: {RequestedType}");
			parts.Add($"Container: {Container}");
			if (Scope != null)
			{
				if (Scope == Container)
					parts[parts.Count - 1] = $"Scope Container: {Scope}";
				else
					parts.Add($"Scope: {Scope}");
			}

			return $"({string.Join(", ", parts)})";
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return _requestedType.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return Equals(obj as ResolveContext);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		public virtual bool Equals(ResolveContext other)
		{
			return object.ReferenceEquals(this, other) || _requestedType == other._requestedType;
		}

		/// <summary>
		/// Implements the equality operator.  Contexts are checked for reference equality, and then
		/// their <see cref="RequestedType"/> properties are checked for equality also.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		public static bool operator ==(ResolveContext left, ResolveContext right)
		{
			//same ref - yes
			if (object.ReferenceEquals(left, right))
				return true;
			//one is null, the other not - short-circuit
			//have to be careful not to do left == null or right == null here or we stackoverflow...
			if (object.ReferenceEquals(null, left) != object.ReferenceEquals(null, right))
				return false;
			//now standard equality check on type/name
			return left._requestedType == right._requestedType;
		}

		/// <summary>
		/// Implements the inequality operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		public static bool operator !=(ResolveContext left, ResolveContext right)
		{
			//same reference
			if (object.ReferenceEquals(left, right))
				return false;
			//one is null, the other isn't - short-circuit
			//have to be careful not to do left == null or right == null here or we stackoverflow ...
			if (object.ReferenceEquals(null, left) != object.ReferenceEquals(null, right))
				return true;
			//now standard inequality check on type/name
			return left._requestedType != right._requestedType;

			//TODO: Going to need to think of a way to bring in user-defined equalities in here - for those
			//contexts where the registration does 'interesting' things with the context.
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the type.
		/// </summary>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		public ResolveContext CreateNew(Type requestedType)
		{
			return new ResolveContext()
			{
				Container = Container,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the <see cref="RequestedType"/> and <see cref="Container"/>.
		/// </summary>
		/// <param name="container">The container for the new context.</param>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		public ResolveContext CreateNew(IContainer container, Type requestedType)
		{
			return new ResolveContext()
			{
				Container = container,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the <see cref="RequestedType"/> and the <see cref="Scope"/>.
		/// </summary>
		/// <param name="requestedType">The type of object to be resolved from the container.</param>
		/// <param name="scope">The scope for the new context.</param>
		public ResolveContext CreateNew(Type requestedType, IScopedContainer scope)
		{
			return new ResolveContext()
			{
				Container = Container,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the <see cref="Container"/>.
		/// </summary>
		/// <param name="container">The container for the new context.</param>
		public ResolveContext CreateNew(IContainer container)
		{
			return new ResolveContext()
			{
				Container = container,
				RequestedType = RequestedType,
				Scope = Scope
			};
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the <see cref="Scope"/>.
		/// </summary>
		/// <param name="scope">The scope for the new context.</param>
		public ResolveContext CreateNew(IScopedContainer scope)
		{
			return new ResolveContext()
			{
				Container = Container,
				RequestedType = RequestedType,
				Scope = scope
			};
		}

		/// <summary>
		/// Returns a clone of this context, but replaces the <see cref="Container"/> and the <see cref="Scope"/>.
		/// </summary>
		/// <param name="container">The container for the new context.</param>
		/// <param name="scope">The scope for the new context.</param>
		public ResolveContext CreateNew(IContainer container, IScopedContainer scope)
		{
			return new ResolveContext()
			{
				Container = container ?? Container, //can't have a null container
				RequestedType = RequestedType,
				Scope = scope
			};
		}
	}
}
