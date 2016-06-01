using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	public class DecoratingEntry : IRezolveTargetEntry
	{
		public Type DeclaredType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IRezolveTarget DefaultTarget
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IEnumerable<IRezolveTarget> Targets
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool UseFallback
		{
			get
			{
				return false; ;
			}
		}

		public IRezolverBuilder ParentBuilder
		{
			get
			{
				return _decorated.ParentBuilder;
			}
		}

		public Type RegisteredType
		{
			get
			{
				return _decorated.RegisteredType;
			}
		}

		DecoratorTarget _target;
		IRezolveTargetEntry _decorated;
		RezolverBuilder _nested;
		public DecoratingEntry(DecoratorTarget target, IRezolveTargetEntry decorated)
		{
			_target = target;
			_decorated = decorated;

		}

		public void AddTarget(IRezolveTarget target, bool checkForDuplicates = false)
		{
			throw new NotImplementedException();
		}

		public Expression CreateExpression(CompileContext context)
		{
			throw new NotImplementedException();
		}

		public bool SupportsType(Type type)
		{
			throw new NotImplementedException();
		}
	}
}
