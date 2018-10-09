using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    public class AutoFactoryTargetBuilder : ExpressionBuilderBase<AutoFactoryTarget>
    {
        protected override Expression Build(AutoFactoryTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var newContext = context.NewContext(target.ReturnType);
            ParameterExpression[] parameters = new ParameterExpression[0];
            // if there are parameters, we have to replace any Resolve calls for the parameter types in 
            // the inner expression with parameter expressions fed from the outer lambda
            if (target.ParameterTypes.Length != 0)
            {
                parameters = target.ParameterTypes.Select((pt, i) => Expression.Parameter(pt, $"p{i}")).ToArray();
                foreach (var parameter in parameters)
                {
                    context.RegisterExpression(parameter, parameter.Type, ScopeBehaviour.None);
                }

                //parameters = target.ParameterTypes.Select((pt, i) => Expression.Parameter(pt, $"p{i}")).ToArray();
                //Dictionary<Type, ParameterExpression> lookup = parameters.ToDictionary(pe => pe.Type);
                //// we're going to add a compilation filter.  The correct way to do this is to grab any existing compilation filter
                //// from the context and then set a new one with the new filters in it, and the original filter as the last entry
                //var newFilters = new ExpressionCompilationFilters(
                //    (ta, ctx, cmp) =>
                //    {
                //        if (ta is ResolvedTarget 
                //            && ta != target.InnerTarget 
                //            && lookup.TryGetValue(ta.DeclaredType, out ParameterExpression replacement))
                //            return replacement;
                //        return null;
                //    }
                //    /*target.ParameterTypes.Select(t =>
                //        new Func<ITarget, IExpressionCompileContext, IExpressionCompiler, Expression>((ta, ctx, cmp) =>
                //        {
                //            if (ta is ResolvedTarget && ta != target.InnerTarget && ta.DeclaredType == t && lookup.TryGetValue(ta.DeclaredType, out ParameterExpression replacement))
                //                return replacement;
                //            return null;
                //        })
                //    ).ToArray()*/
                //);
                //var existingFilter = newContext.GetOption<IExpressionCompilationFilter>();
                //if (existingFilter != null)
                //    newFilters.Add(existingFilter);
                //newContext.SetOption<IExpressionCompilationFilter>(newFilters);
            }

            var baseExpression = compiler.BuildResolveLambda(target.InnerTarget, newContext);
            var lambda = Expression.Lambda(target.DelegateType,
                Expression.Convert(Expression.Invoke(baseExpression, context.ResolveContextParameterExpression), target.ReturnType), parameters);
            return lambda;
        }
    }
}
