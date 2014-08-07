using System;

namespace Rezolver
{
	public class LifetimeRezolverContainer : RezolverContainerBase, ILifetimeRezolverContainer
	{
		
		public override IRezolveTargetCompiler Compiler
		{
			get
			{
				return _parentContainer.Compiler;
			}
			protected set { throw new NotSupportedException(); }
		}

		protected override IRezolverScope Scope
		{
			get
			{
				return _parentContainer;
			}
			set { throw new NotSupportedException(); }
		}

		public override object Resolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			return _parentContainer.Resolve(type, name, dynamicContainer);
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

		public void Dispose()
		{
			//dispose of all the tracked child objects and any child scopes.
		}
	}
}