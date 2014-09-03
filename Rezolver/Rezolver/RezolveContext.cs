using System;
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
		public static RezolveContext EmptyContext = new RezolveContext(null);

		private class StubRezolver : IRezolver
		{
			private static StubRezolver _instance = new StubRezolver();

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

			public bool CanResolve(RezolveContext context)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public object Resolve(RezolveContext context)
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

			public void Register(IRezolveTarget target, Type type = null, RezolverPath path = null)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public IRezolveTarget Fetch(Type type, string name = null)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public IRezolveTarget Fetch<T>(string name = null)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}

			public INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
			{
				throw new InvalidOperationException(String.Format("The RezolveContext has no Rezolver set"));
			}
		}

		private Type _requestedType;
		public Type RequestedType { get { return _requestedType; } private set { _requestedType = value; } }

		private string _name;
		public string Name { get { return _name; } private set { _name = value; } }

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

		public RezolveContext(IRezolver rezolver, Type requestedType, string name)
			: this(rezolver)
		{
			RequestedType = requestedType;
			Name = name;
		}

		public RezolveContext(IRezolver rezolver, Type requestedType, ILifetimeScopeRezolver scope)
			: this(rezolver)
		{
			RequestedType = requestedType;
			Scope = scope;
		}

		public RezolveContext(IRezolver rezolver, Type requestedType, string name, ILifetimeScopeRezolver scope)
			: this(rezolver)
		{
			RequestedType = requestedType;
			Name = name;
			Scope = scope;
		}

		private RezolveContext(IRezolver rezolver)
		{
			_rezolver = rezolver ?? StubRezolver.Instance;
		}



		public override int GetHashCode()
		{
			return RequestedType.GetHashCode() ^ (Name != null ? Name.GetHashCode() : 0);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return Equals(obj as RezolveContext);
		}

		public bool Equals(RezolveContext other)
		{
			return RequestedType == other.RequestedType && Name == other.Name;
		}

		public static bool operator ==(RezolveContext left, RezolveContext right)
		{
			return object.ReferenceEquals(left, right) || (left.RequestedType == right.RequestedType && left.Name == right.Name);
		}

		public static bool operator !=(RezolveContext left, RezolveContext right)
		{
			return !object.ReferenceEquals(left, right) && (left.RequestedType != right.RequestedType || left.Name != right.Name);
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
				Name = null, //name is part of the object's identity - so should be nulled when changing the type
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, string name)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				Name = name,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver, Type requestedType)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver,
				Name = null,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver, Type requestedType, string name)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				Name = name,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				Name = null,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, string name, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				Name = name,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver, Type requestedType, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver,
				Name = null,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver,
				Name = Name,
				RequestedType = RequestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = Rezolver,
				Name = Name,
				RequestedType = RequestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(IRezolver rezolver, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				Rezolver = rezolver ?? Rezolver, //can't have a null rezolver
				Name = Name,
				RequestedType = RequestedType,
				Scope = scope
			};
		}
	}
}
