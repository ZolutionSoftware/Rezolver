﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Context of a call to an IRezolver's Resolve method.  The rezolver is included
	/// in the context to allow IRezolveTarget-generated code to refer back to the rezolver.
	/// 
	/// This also allows us to retarget compiled targets at other rezolvers (e.g. child rezolvers
	/// that override existing registrations or define new ones).
	/// </summary>
	public class RezolveContext : IEquatable<RezolveContext>
	{
		public static readonly RezolveContext EmptyContext = new RezolveContext(null);

		private class StubRezolver : IRezolver
		{
			private static readonly StubRezolver _instance = new StubRezolver();

			public static StubRezolver Instance
			{
				get
				{
					return _instance;
				}
			}

			public IRezolveTargetCompiler Compiler
			{
				get { throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set")); }
			}

			public IRezolverBuilder Builder
			{
				get { throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set")); }
			}

			public bool CanResolve(RezolveContext context)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public object Resolve(RezolveContext context)
			{
				context.MustNotBeNull("context");
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public bool TryResolve(RezolveContext context, out object result)
			{
				context.MustNotBeNull("context");
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public ILifetimeScopeRezolver CreateLifetimeScope()
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public ICompiledRezolveTarget FetchCompiled(RezolveContext context)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public void Register(IRezolveTarget target, Type type = null)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public ILifetimeScopeRezolver CreateLifetimeScope(IRezolver overridingRezolver)
			{
				throw new NotImplementedException();
			}

			object IServiceProvider.GetService(Type serviceType)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}
		}

		private Type _requestedType;
		public Type RequestedType { get { return _requestedType; } private set { _requestedType = value; } }

		private IRezolver _rezolver;

		/// <summary>
		/// The rezolver for this context.
		/// </summary>
		public IRezolver Rezolver { get { return _rezolver; } private set { _rezolver = value; } }

		private ILifetimeScopeRezolver _scope;
		public ILifetimeScopeRezolver Scope { get { return _scope; } private set { _scope = value; } }

		private RezolveContext() { }

		public RezolveContext(IRezolver rezolver, Type requestedType)
			: this(rezolver)
		{
			RequestedType = requestedType;
		}

		public RezolveContext(IRezolver rezolver, Type requestedType, ILifetimeScopeRezolver scope)
			: this(rezolver)
		{
			RequestedType = requestedType;
			Scope = scope;
		}

		private RezolveContext(IRezolver rezolver)
		{
			_rezolver = rezolver ?? StubRezolver.Instance;
			//automatically inherit the rezolver as this context's scope, if it's of the correct type.
			//note - all the other constructors chain to this one.  Note that other constructors
			//might supply a separate scope in addition, which will overwrite the scope set here.
			_scope = rezolver as ILifetimeScopeRezolver;
		}

		public override string ToString()
		{
			List<string> parts = new List<string>();

			parts.Add($"Type: {RequestedType}");
			parts.Add($"Rezolver: {Rezolver}");
			if (Scope != null)
			{
				if (Scope == Rezolver)
					parts[parts.Count - 1] = $"Scope Rezolver: {Scope}";
				else
					parts.Add($"Scope: {Scope}");
			}

			return $"({string.Join(", ", parts)})";
		}

		public override int GetHashCode()
		{
			return _requestedType.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return Equals(obj as RezolveContext);
		}

		public virtual bool Equals(RezolveContext other)
		{
			return object.ReferenceEquals(this, other) || _requestedType == other._requestedType;
		}

		public static bool operator ==(RezolveContext left, RezolveContext right)
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

		public static bool operator !=(RezolveContext left, RezolveContext right)
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
		/// Returns a clone of this context, but replaces the type, establishing the root context relationship
		/// also, either by inheriting this one's root context, or setting this as the root context.
		/// </summary>
		/// <param name="requestedType"></param>
		/// <returns></returns>
		public RezolveContext CreateNew(Type requestedType)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver, Type requestedType)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver, Type requestedType, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver,
				RequestedType = RequestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				RequestedType = RequestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver ?? Rezolver, //can't have a null rezolver
				RequestedType = RequestedType,
				Scope = scope
			};
		}
	}
}
