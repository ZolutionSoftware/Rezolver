using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel.Channels;

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
#if DEBUG
			var expression =
				Expression.Lambda<Func<RezolveContext, object>>(target.CreateExpression(new CompileContext(context, typeof(object))), context.RezolveContextParameter);
			Debug.WriteLine("Compiling Func<RezolveContext, object> from static lambda {0} for target type {1}", expression, "System.Object");
			return new DelegatingCompiledRezolveTarget(expression.Compile());
#else
			return new DelegatingCompiledRezolveTarget(
				Expression.Lambda<Func<RezolveContext, object>>(target.CreateExpression(new CompileContext(context, typeof(object))), context.RezolveContextParameter).Compile());
#endif
		}
	}
}