using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// As the name suggests, this joins two rezolvers into one using the dynamic rezolver mechanism.
	/// 
	/// 
	/// </summary>
	public class CombinedRezolver : IRezolver
	{
		private readonly IRezolver _first;
		private readonly IRezolver _second;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second">Note, you can pass null - in which case this instance
		/// simply acts as a proxy for the first rezolver.</param>
		public CombinedRezolver(IRezolver first, IRezolver second)
		{
			first.MustNotBeNull("first");
			_first = first;
			_second = second;
		}

		public IRezolveTargetCompiler Compiler
		{
			get { return _first.Compiler; }
		}

		public bool CanResolve(RezolveContext context)
		{
			return _first.CanResolve(context) || (_second != null && _second.CanResolve(context));
		}

		public object Resolve(RezolveContext context)
		{
			return _second != null ?
				_first.Resolve(context.CreateNew(new CombinedRezolver(_second, context.DynamicRezolver)))
				: _first.Resolve(context);
		}

		public ILifetimeScopeRezolver CreateLifetimeScope()
		{
			return new LifetimeScopeRezolver(this);
		}

		public ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			throw new NotImplementedException();
		}

		public void Register(IRezolveTarget target, Type type = null, RezolverPath path = null)
		{
			throw new NotImplementedException();
		}

		public IRezolveTarget Fetch(Type type, string name = null)
		{
			throw new NotImplementedException();
		}

		public IRezolveTarget Fetch<T>(string name = null)
		{
			throw new NotImplementedException();
		}

		public INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
		{
			throw new NotImplementedException();
		}
	}
}
