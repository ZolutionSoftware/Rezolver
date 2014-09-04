using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	public class RezolveTargetDelegateCompiler : IRezolveTargetCompiler
	{
		public static readonly IRezolveTargetCompiler Default = new RezolveTargetDelegateCompiler();

		public class DelegatingCompiledRezolveTarget : ICompiledRezolveTarget
		{
			private readonly Func<RezolveContext, object> _getObjectDelegate;

			public DelegatingCompiledRezolveTarget(Func<RezolveContext, object> getObjectDelegate)
			{
				_getObjectDelegate = getObjectDelegate;
			}

			public object GetObject(RezolveContext context)
			{
				return _getObjectDelegate(context);
			}
		}

		public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, CompileContext context)
		{
			context = new CompileContext(context, typeof(object), false);
			var baseExpression = target.CreateExpression(context);
			var sharedLocals = context.SharedLocals.ToArray();
			if(sharedLocals.Length != 0)
				baseExpression = Expression.Block(baseExpression.Type, sharedLocals, baseExpression);
#if DEBUG
			var expression =
				Expression.Lambda<Func<RezolveContext, object>>(baseExpression, context.RezolveContextParameter);
			Debug.WriteLine("Compiling Func<RezolveContext, object> from static lambda {0} for target type {1}", expression, "System.Object");
			return new DelegatingCompiledRezolveTarget(expression.Compile());
#else
			return new DelegatingCompiledRezolveTarget(
				Expression.Lambda<Func<RezolveContext, object>>(baseExpression, context.RezolveContextParameter).Compile());
#endif
		}
	}
}