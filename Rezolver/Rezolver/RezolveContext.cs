using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Context of a call to an IRezolver's Resolve method.
	/// </summary>
	public class RezolveContext : IEquatable<RezolveContext>
	{
		public static RezolveContext EmptyContext = new RezolveContext();

		private Type _requestedType;
		public Type RequestedType { get { return _requestedType; } private set { _requestedType = value; } }

		private string _name;
		public string Name { get { return _name; } private set { _name = value; } }

		private IRezolver _dynamicRezolver;
		public IRezolver DynamicRezolver { get { return _dynamicRezolver; } private set { _dynamicRezolver = value; } }

		private ILifetimeScopeRezolver _scope;
		public ILifetimeScopeRezolver Scope { get { return _scope; } private set { _scope = value; } }

		//private RezolveContext _rootContext;
		///// <summary>
		///// Returns the root RezolveContext for a resolve operation.  The root context is the one that
		///// is initially created at the start of a resolve call, and is used to obtain the initial lifetime scope
		///// and dynammic rezolver that are to be used for the whole resolve operation.
		///// </summary>
		//public RezolveContext RootContext
		//{
		//	get
		//	{
		//		return _rootContext ?? this;
		//	}
		//	private set
		//	{
		//		_rootContext = value;
		//	}
		//}

		public RezolveContext(Type requestedType)
		{
			RequestedType = requestedType;
		}

		public RezolveContext(Type requestedType, string name)
		{
			RequestedType = requestedType;
			Name = name;
		}

		public RezolveContext(Type requestedType, IRezolver dynamicRezolver)
		{
			RequestedType = requestedType;
			DynamicRezolver = dynamicRezolver;
		}

		public RezolveContext(Type requestedType, string name, IRezolver dynamicRezolver)
		{
			RequestedType = requestedType;
			Name = name;
			DynamicRezolver = dynamicRezolver;
		}

		public RezolveContext(Type requestedType, ILifetimeScopeRezolver scope)
		{
			RequestedType = requestedType;
			Scope = scope;
		}

		public RezolveContext(Type requestedType, string name, ILifetimeScopeRezolver scope)
		{
			RequestedType = requestedType;
			Name = name;
			Scope = scope;
		}

		public RezolveContext(Type requestedType, IRezolver dynamicRezolver, ILifetimeScopeRezolver scope)
		{
			RequestedType = requestedType;
			DynamicRezolver = dynamicRezolver;
			Scope = scope;
		}

		/// <summary>
		/// Constructs a new instance of the <see cref="RezolveContext"/> class.
		/// </summary>
		/// <param name="requestedType">The type of object requested from the Resolve operation.</param>
		/// <param name="name">The name, if any of the type requested from the Resolve operation.</param>
		/// <param name="dynamicRezolver">If a rezolver is passed into a resolve call then you pass it here.</param>
		/// <param name="scope">Any lifetime scope that is currently effective.  Objects which need to be registered
		/// into a lifetime scope will use this as their target scope.</param>
		public RezolveContext(Type requestedType, string name, IRezolver dynamicRezolver, ILifetimeScopeRezolver scope)
		{
			RequestedType = requestedType;
			Name = name;
			DynamicRezolver = dynamicRezolver;
			Scope = scope;
		}

		public RezolveContext()
			: this(null, null, null, null)
		{

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
				DynamicRezolver = DynamicRezolver,
				Name = Name,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, string name)
		{
			return new RezolveContext()
			{
				DynamicRezolver = DynamicRezolver,
				Name = name,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, IRezolver dynamicRezolver)
		{
			return new RezolveContext()
			{
				DynamicRezolver = dynamicRezolver,
				Name = Name,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, string name, IRezolver dynamicRezolver)
		{
			return new RezolveContext()
			{
				DynamicRezolver = dynamicRezolver,
				Name = name,
				RequestedType = requestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				DynamicRezolver = DynamicRezolver,
				Name = Name,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, string name, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				DynamicRezolver = DynamicRezolver,
				Name = name,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(Type requestedType, IRezolver dynamicRezolver, ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				DynamicRezolver = dynamicRezolver,
				Name = Name,
				RequestedType = requestedType,
				Scope = scope
			};
		}

		public RezolveContext CreateNew(IRezolver dynamicRezolver)
		{
			return new RezolveContext()
			{
				DynamicRezolver = dynamicRezolver,
				Name = Name,
				RequestedType = RequestedType,
				Scope = Scope
			};
		}

		public RezolveContext CreateNew(ILifetimeScopeRezolver scope)
		{
			return new RezolveContext()
			{
				DynamicRezolver = DynamicRezolver,
				Name = Name,
				RequestedType = RequestedType,
				Scope = scope
			};
		}
	}
}
