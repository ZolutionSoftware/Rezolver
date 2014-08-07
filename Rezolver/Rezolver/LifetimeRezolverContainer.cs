using System;

namespace Rezolver
{
	public class LifetimeRezolverContainer : CachingRezolverContainer, ILifetimeRezolverContainer
	{
		public override IRezolveTargetCompiler Compiler
		{
			get
			{
				return _parentContainer.Compiler;
			}
		}

		protected override IRezolverScope Scope
		{
			get
			{
				return _parentContainer;
			}
		}

		public override object Resolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			var result = _parentContainer.Resolve(type, name, dynamicContainer);
			//I think targets need to compile a special version of their code which
			//accepts a lifetime scope (and optionally a dynamic container), so that
			//the target itself has full control over how it creates its object under
			//lifetime scopes.
			throw new NotImplementedException();
			return result;
		}

		public override T Resolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			return _parentContainer.Resolve<T>(name, dynamicContainer);
		}

		public override IRezolverContainer CreateLifetimeContainer()
		{
			throw new NotImplementedException();
		}

		private readonly IRezolverContainer _parentContainer;
		public LifetimeRezolverContainer(IRezolverContainer parentContainer)
		{
			_parentContainer = parentContainer;
		}

		public virtual void Dispose()
		{
			//dispose of all the tracked child objects and any child scopes.
		}
	}
}