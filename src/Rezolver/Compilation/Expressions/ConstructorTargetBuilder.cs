// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for building the target <see cref="ConstructorTarget"/>
    /// </summary>
    public class ConstructorTargetBuilder : ExpressionBuilderBase<ConstructorTarget>
    {
        /// <summary>
        /// Override of <see cref="ExpressionBuilderBase{TTarget}.Build(TTarget, IExpressionCompileContext, IExpressionCompiler)"/>
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The compiler to be used to build the target</param>
        protected override Expression Build(ConstructorTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return Build(target.Bind(context), context, compiler);
        }

        /// <summary>
        /// Builds an expression for the specified <see cref="ConstructorBinding" />.
        /// Called by <see cref="Build(ConstructorTarget, IExpressionCompileContext, IExpressionCompiler)" />
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="context">The context.</param>
        /// <param name="compiler">The compiler to be used to build the target.</param>
        /// <remarks>The returned expression will either be a NewExpression or a MemberInitExpression</remarks>
        protected virtual Expression Build(ConstructorBinding binding, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var newExpr = Expression.New(binding.Constructor,
                binding.BoundArguments.Select(
                    a => compiler.Build(a.Target, context.NewContext(a.Parameter.ParameterType))));

            if (binding.MemberBindings.Length == 0)
            {
                return newExpr;
            }
            else
            {
                ParameterExpression localVar = null;
                List<MemberAssignment> memberBindings = new List<MemberAssignment>();
                List<Expression> adHocBindings = new List<Expression>();

                foreach (var mb in binding.MemberBindings)
                {
                    // as soon as we have one list binding (which we can actually implement
                    // using the list binding expression) we need to capture the locally newed
                    // object into a local variable and pass it to the function below
                    if (mb is ListMemberBinding listBinding)
                    {
                        if (localVar == null)
                        {
                            localVar = Expression.Parameter(newExpr.Type, "toReturn");
                        }

                        adHocBindings.Add(GenerateListBindingExpression(localVar, listBinding, context, compiler));
                    }
                    else
                    {
                        memberBindings.Add(Expression.Bind(mb.Member, compiler.Build(mb.Target, context.NewContext(mb.MemberType))));
                    }
                }

                Expression toReturn = newExpr;

                if (memberBindings.Count != 0)
                {
                    toReturn = Expression.MemberInit(newExpr, memberBindings);
                }

                if (adHocBindings.Count != 0)
                {
                    List<Expression> blockCode = new List<Expression>
                    {
                        Expression.Assign(localVar, toReturn)
                    };
                    blockCode.AddRange(adHocBindings);
                    blockCode.Add(localVar);
                    toReturn = Expression.Block(new[] { localVar }, blockCode);
                }

                return toReturn;
            }
        }

        internal static void AddToCollection<T>(Action<T> addDelegate, IEnumerable<T> source)
        {
            foreach (var i in source)
            {
                addDelegate(i);
            }
        }

        private static readonly MethodInfo AddToCollection_Method = Extract.Method(() => AddToCollection<int>(null, null)).GetGenericMethodDefinition();

        private Expression GenerateListBindingExpression(Expression targetObj, ListMemberBinding listBinding, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var method = AddToCollection_Method.MakeGenericMethod(listBinding.ElementType);
            var enumType = typeof(IEnumerable<>).MakeGenericType(listBinding.ElementType);
            var enumerable = compiler.Build(listBinding.Target, context.NewContext(enumType));
            var enumLocal = Expression.Parameter(enumType, "enumerable");
            var enumAssign = Expression.Assign(enumLocal, enumerable);
            var addDelegateParam = Expression.Parameter(listBinding.ElementType, "item");

            var callAddToCollection = Expression.Call(null,
                method,
                Expression.Lambda(
                    Expression.Call(
                        listBinding.Member is PropertyInfo prop ? Expression.Property(targetObj, prop) : Expression.Field(targetObj, (FieldInfo)listBinding.Member),
                        listBinding.AddMethod,
                        addDelegateParam),
                    addDelegateParam),
                enumLocal);
            return Expression.Block(new[] { enumLocal, addDelegateParam },
                enumAssign,
                callAddToCollection);
        }
    }
}
