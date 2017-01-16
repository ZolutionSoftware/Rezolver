using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
  /// <summary>
  /// An <see cref="IExpressionBuilder"/> specialised for building an expression for the <see cref="OptionalParameterTarget"/> target.
  /// </summary>
  public class OptionalParameterTargetBuilder : ExpressionBuilderBase<OptionalParameterTarget>
  {
    protected override Expression Build(OptionalParameterTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
    {
      return (target.MethodParameter.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault ?
        (Expression)Expression.Constant(target.MethodParameter.DefaultValue, target.MethodParameter.ParameterType) : Expression.Default(target.MethodParameter.ParameterType);
    }
  }
}
