// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for building the expression trees for the <see cref="ExpressionTarget"/>
    /// target type.
    ///
    /// This builder takes care of all expressions, including lambdas (where additional parameters beyond the standard
    /// <see cref="ResolveContext"/> are turned into local variables with injected values), producing an expression which can
    /// be compiled by an <see cref="IExpressionCompiler"/> after a <see cref="TargetExpressionRewriter"/> has been used to
    /// expand any targets embedded in the expression.
    /// </summary>
    public class ExpressionTargetBuilder : ExpressionBuilderBase<ExpressionTarget>
    {
        /// <summary>
        /// rewrites the expression from an ExpressionTarget into something we can actually compile and use
        /// in the context of a call to a container's Resolve method.
        /// </summary>
        /// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
        private class ExpressionTranslator : ExpressionVisitor
        {
            private class MethodCallRewrite
            {
                private readonly MethodInfo _method;
                private readonly Func<ExpressionTranslator, MethodCallExpression, MethodCallExpression> _callback;

                public MethodCallRewrite(MethodInfo method, Func<ExpressionTranslator, MethodCallExpression, MethodCallExpression> callback)
                {
                    this._method = method;
                    this._callback = callback;
                }

                public bool RewriteIfMatch(ExpressionTranslator translator, MethodCallExpression call, out MethodCallExpression rewritten)
                {
                    rewritten = null;
                    if (this._method.IsGenericMethodDefinition)
                    {
                        if (!call.Method.IsGenericMethod || call.Method.GetGenericMethodDefinition() != this._method)
                        {
                            return false;
                        }
                    }
                    else if (this._method != call.Method)
                    {
                        return false;
                    }

                    rewritten = this._callback(translator, call);
                    return rewritten != null;
                }
            }

            /// <summary>
            /// The Resolve* operations which should be rewritten as ResolvedTargets.  Generic methods must have one type argument (which will
            /// be reused as the type that is resolved), non-generic methods must have a type parameter and the expression's argument MUST be
            /// a constant type.
            ///
            /// Note that these methods are used by index position in the Rewrites array a little further down - so if you change the order,
            /// be sure that the rewrites aren't affected.
            /// </summary>
            private static readonly MethodInfo[] RezolveMethods =
            {
                Extract.Method((ResolveContext rc) => rc.Resolve<int>()).GetGenericMethodDefinition(),
                Extract.Method((ResolveContext rc) => rc.Resolve((Type)typeof(int)))
            };

            /// <summary>
            /// The rewrites
            /// </summary>
            private static readonly MethodCallRewrite[] Rewrites =
                        {
                // rewrites to (ResolveContext.Resolve<T>()) of the ResolveContextExpression of the current compile context
                new MethodCallRewrite(
                    Extract.Method(() => ExpressionFunctions.Resolve<int>()).GetGenericMethodDefinition(),
                    (t, c) => Expression.Call(t._context.ResolveContextParameterExpression, RezolveMethods[0].MakeGenericMethod(c.Method.GetGenericArguments()[0]), c.Arguments)
                ),
                // rewrites to (ResolveContext.Resolve(t)) of the ResolveContextExpression of the current compile context
                new MethodCallRewrite(
                    Extract.Method(() => ExpressionFunctions.Resolve(typeof(int))),
                    (t, c) => Expression.Call(t._context.ResolveContextParameterExpression, RezolveMethods[1], c.Arguments)
                )
            };

            private readonly IExpressionCompileContext _context;

            public ExpressionTranslator(IExpressionCompileContext context)
            {
                this._context = context;
            }

            internal Type ExtractRezolveCallType(Expression e)
            {
                // note - at the moment, the type that's passed to the Resolve call which is to be turned into a ResolvedTarget must be a constant
                // in order for it to be translated.  If it is not, then it will not be converted to a ResolvedTarget and will instead be baked as a
                // method call.  This isn't a problem as such, it just reduces the potential efficiency of the generated expression as it will not be able
                // to 'lift' the expression for that target into the expression being compiled.
                if (!(e is MethodCallExpression methodExpr))
                {
                    return null;
                }

                for(var f = 0; f<RezolveMethods.Length; f++)
                {
                    if(RezolveMethods[f] == methodExpr.Method)
                    {
                        // the first non-null Constant type is compatible with System.Type is the one we use.
                        var typeArg = methodExpr.Arguments.OfType<ConstantExpression>().FirstOrDefault(arg => arg.Value != null && typeof(Type).IsAssignableFrom(arg.Type));
                        return (Type)typeArg.Value;
                    }
                    else if(RezolveMethods[f].IsGenericMethodDefinition && methodExpr.Method.IsGenericMethod && RezolveMethods[f] == methodExpr.Method.GetGenericMethodDefinition())
                    {
                        return methodExpr.Method.GetGenericArguments()[0];
                    }
                }

                return null;
            }

            protected override Expression VisitExtension(Expression node)
            {
                // if the expression is the TargetExpression, we simply return it - this happens
                // when an expression specifically uses a known ITarget to provide a value for something
                // within an expression (and allows us to mix and match expressions with targets).
                if (node is TargetExpression)
                {
                    return node;
                }

                return base.VisitExtension(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return new TargetExpression(new ObjectTarget(node.Value, node.Type));
            }

            private IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr)
            {
                return newExpr.Constructor.GetParameters()
                  .Zip(newExpr.Arguments, (info, expression) => new ParameterBinding(info, new ExpressionTarget(expression))).ToArray();
            }

            protected override Expression VisitNew(NewExpression node)
            {
                // we have to use the base's Visitor code so that argument bindings are translated correctly
                var translated = (NewExpression)base.VisitNew(node);
                ConstructorInfo ctor = null;
                ParameterBinding[] parameterBindings = null;

                ctor = translated.Constructor;
                parameterBindings = ExtractParameterBindings(translated).ToArray();

                return new TargetExpression(new ConstructorTarget(ctor, parameterBindings, null).Unscoped());
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                return new TargetExpression(new ExpressionTarget(Factory, node.Type));

                Expression Factory(ICompileContext context)
                {
                    var adaptedCtorExp = Visit(node.NewExpression);
                    // var ctorTargetExpr = constructorTarget.CreateExpression(c.New(node.Type));

                    // the goal here, then, is to find the new expression for this type and replace it
                    // with a memberinit equivalent to the one we visited.  Although the constructor target produces
                    // a NewExpression, it isn't going to be the root expression, because of the scoping boilerplate
                    // that is put around nearly all expressions produced by RezolveTargetBase implementations.
                    var rewriter = new NewExpressionMemberInitRewriter(node.Type, node.Bindings.Select(mb => VisitMemberBinding(mb)));
                    return rewriter.Visit(adaptedCtorExp);
                }
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                Expression body = node.Body;
                try
                {
                    ParameterExpression rezolveContextParam = node.Parameters.SingleOrDefault(p => p.Type == typeof(ResolveContext));
                    // if the lambda had a parameter of the type ResolveContext, swap it for the
                    // RezolveContextParameterExpression parameter expression that all the internal
                    // components use when building expression trees from targets.
                    if (rezolveContextParam != null && rezolveContextParam != this._context.ResolveContextParameterExpression)
                        body = new ExpressionSwitcher(new[]
                        {
                            new ExpressionReplacement(rezolveContextParam, this._context.ResolveContextParameterExpression)
                        }).Visit(body);
                }
                catch (InvalidOperationException ioex)
                {
                    // throw by the SingleOrDefault call inside the Try.
                    throw new ArgumentException($"The lambda expression {node} is not supported - it has multiple ResolveContext parameters, and only a maximum of one is allowed", nameof(node), ioex);
                }

                var variables = node.Parameters.Where(p => p.Type != typeof(ResolveContext)).ToArray();
                // if we have lambda parameters which need to be converted to block variables which are resolved
                // by assignment (dynamic service location I suppose you'd call it) then we need to wrap everything
                // in a block expression.
                if (variables.Length != 0)
                {
                    return Expression.Block(node.Body.Type,
                        // all parameters from the Lambda, except one typed as ResolveContext, are fed into the new block as variables
                        variables,
                        // start the block with a run of assignments for all the parameters of the original lambda
                        // with services resolved from the container
                        variables.Select(p => Expression.Assign(p, new TargetExpression(new ResolvedTarget(p.Type)))).Concat(
                            new[]
                            {
                                // and then concatenate the original body of the Lambda, which might have had
                                // any references to a ResolveContext parameter switched for the global RezolveContextParameterExpression
                                base.Visit(body)
                            }
                        )
                    );
                }
                else
                {
                    return base.Visit(body);
                }
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                // check to see if the method is one which needs to be rewritten before being analysed
                MethodCallExpression rewritten = null;
                foreach (var rewrite in Rewrites)
                {
                    if (rewrite.RewriteIfMatch(this, node, out rewritten))
                    {
                        break;
                    }
                }

                var rezolvedType = ExtractRezolveCallType(rewritten ?? node);
                if (rezolvedType != null)
                {
                    return new TargetExpression(new ResolvedTarget(rezolvedType));
                }

                // note that we use the rewritten version if we got one, because the chances are
                // that the original method call is one that doesn't actually do anything (or, worse
                // still, throws an exception) if called at runtime.
                return base.VisitMethodCall(rewritten ?? node);
            }
        }

        /// <summary>
        /// Builds an expression for the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        protected override Expression Build(ExpressionTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // reasonably simple - get the underlying expression, push it through the ExpressionTranslator to perform any parameter augmentations
            // or conversion to other targets (like ResolvedTarget, CconstructorTarget etc) and then push the result through a
            // TargetExpressionRewriter to compile any newly created targets into their respective expressions and into the resulting
            // expression.

            var translator = new ExpressionTranslator(context);
            var translated = translator.Visit(target.ExpressionFactory != null ? target.ExpressionFactory(context) : target.Expression);
            // the translator does lots of things - including identifying common code constructs which have rich target equivalents - such as
            // the NewExpression being the same as the ConstructorTarget.  When it creates a target in place of an expression, it wrap it
            // inside a TargetExpression - so these then have to be compiled again via the TargetExpressionRewriter.
            var targetRewriter = new TargetExpressionRewriter(compiler, context);
            return targetRewriter.Visit(translated);
        }
    }
}
