// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
            var (returnType, parameterTypes) = TypeHelpers.DecomposeDelegateType(context.TargetType);
            var compileReturnType = target.ReturnType.ContainsGenericParameters ? returnType : target.ReturnType;
            var newContext = context.NewContext(compileReturnType);
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
            }
            var baseExpression = compiler.BuildResolveLambda(target.Bind(newContext), newContext);
            var lambda = Expression.Lambda(context.TargetType,
                Expression.Convert(Expression.Invoke(baseExpression, context.ResolveContextParameterExpression), compileReturnType), parameters);
            return lambda;
        }
    }
}
