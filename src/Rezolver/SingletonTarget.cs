using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Rezolver
{
	/// <summary>
	/// Decorates any IRezolveTarget instance inside a lazily-initialised instance which will only ever
	/// create the target object once.
	/// </summary>
	public class SingletonTarget : RezolveTargetBase
	{
        /// <summary>
        /// Used in ensuring the correct construction and scope tracking of a singleton instance.
        /// 
        /// Implements the ICompiledRezolveTarget interface for the single case of creating a lazy on demand (from the
        /// first RezolveContext that's passed to its GetObject method, and returning its value.
        /// </summary>
        private class SingletonTargetLazyInitialiser : ICompiledRezolveTarget
        {
            private SingletonTarget _target;
            private Func<RezolveContext, Lazy<object>> _lazyFactory;

            internal SingletonTargetLazyInitialiser(SingletonTarget target, Func<RezolveContext, Lazy<object>> lazyFactory)
            {
                _target = target;
                _lazyFactory = lazyFactory;
            }

            /// <summary>
            /// Creates the singleton target's Lazy on demand if required, and then returns the object that the underlying
            /// compiled code produces.
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            object ICompiledRezolveTarget.GetObject(RezolveContext context)
            {
                lock (_target._lazyLocker)
                {
                    //there's a small chance of multiple lazys being constructed here and a 
                    //smaller chance of more than one being invoked - if multiple threads try to 
                    //invoke one of these against the same SingletonTarget at the same time
                    if (_target._lazy == null)
                        _target._lazy = _lazyFactory(context);
                }

                return _target._lazy.Value;
            }
        }

        private static readonly MethodInfo ICompiledRezolveTarget_GetObject = MethodCallExtractor.ExtractCalledMethod((ICompiledRezolveTarget t) => t.GetObject(null));
        private static readonly ConstructorInfo LazyObject_Ctor = MethodCallExtractor.ExtractConstructorCall(() => new Lazy<object>(() => (object)null));

		private IRezolveTarget _innerTarget;
        //might seem silly - but we need a stateless locking mechanism over the lazy to ensure we only ever create one.
        //because the creation of our singleton instances is parameterised by RezolveContext, we can't use another lazy,
        //so we resort to using the lock mechanism.  As it is, the chances of creating two separate instances from two 
        //different lazy objects for the same target are very small - but since the chance is there, it's worth coding it
        //away.
        private readonly object _lazyLocker = new object();
		private Lazy<object> _lazy;

        /// <summary>
        /// Overrides the base class to ensure that automatic generation of the scope tracking code by RezolveTargetBase is disabled.
        /// 
        /// For the singleton, it's important that the scope tracking call occurs within the lazy's callback.
        /// </summary>
        protected override bool SuppressScopeTracking
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SingletonTarget"/> class.
        /// </summary>
        /// <param name="innerTarget">The target whose result (when compiled) is to be used as the singleton instance.</param>
        public SingletonTarget(IRezolveTarget innerTarget)
		{
			innerTarget.MustNotBeNull("innerTarget");
			_innerTarget = innerTarget;
		}

        protected override Expression CreateScopeSelectionExpression(CompileContext context, Expression expression)
        {
            return ExpressionHelper.Make_Scope_GetScopeRootCallExpression(context);
        }
        
        protected override Expression CreateExpressionBase(CompileContext context)
		{
            //this isn't quite right, imho.  One singleton target should technically be able to create
            //more than one instance, if that target is in a RezolverBuilder that is, in turn, used to 
            //build more than one top-level rezolver (a permitted use case).
            //The way this code is written, however, the singleton will only ever create one instance and,
            //in a cruel twist, will only ever belong to one scope (if applicable).
            //to fix this will mean being able to easily identify different rezolver trees from each other,
            //and creating one instance for each unique tree.  This isn't even possible at the moment, because
            //there's no way to identify the root rezolver in a chain of rezolvers, unless you force the use
            //of scopes, which we don't necessarily want to do.
            //So I'm not sure if I've coded myself down a hole here - or if adding the necessary properties to 
            //the IRezolver interface to uniquely identify each one, and walk any parent/child relationship from 
            //child back to root would actually be a good thing to do in general anyway, so should therefore just
            //be done.

            //if the lazy has already been constructed, then simply returning an expression that reads its value.
            if(_lazy != null)
            {
                return Expression.Property(Expression.Constant(_lazy), "Value");
            }
            else
            {
                //if our inner target is also a singleton, we just chain straight through to its own CreateExpression
                //call.  Note that if this is the case, our own lazy will never be constructed and the code will always
                //fall into this branch.
                if (_innerTarget is SingletonTarget)
                    return _innerTarget.CreateExpression(context);
                else
                {
                    //get the underlying expression for the target that is to be turned into a singleton - but disable the
                    //generation of any scope-tracking code.
                    var innerExpression = ExpressionHelper.GetLambdaBodyForTarget(_innerTarget,
                        new CompileContext(context, context.TargetType, inheritSharedExpressions: true, suppressScopeTrackingExpressions: true));
                    //generate our scope tracking expression.
                    var scopeTracking = CreateScopeTrackingExpression(context, innerExpression);

                    //there's an argument for using the compiler from the compilecontext (via the Rezolver) here to generate this 
                    //lambda, but our interface specifically creates an ICompiledRezolveTarget - not a delegate - so it's not 
                    //really suitable.
                    //in any case, compiling directly to a delegate will work on all platforms on which Rezolver will work in the 
                    //first place, it just might not be as fast as it might be if we honoured the context's compiler, if
                    //that happens to be the AssemblyRezolveTargetCompiler.
                    var lazyLambdaExpr = Expression.Lambda(Expression.New(LazyObject_Ctor,
                        //creating the factory delegate for the lazy uniquely for each invokation of the outer lambda
                        Expression.Lambda(Expression.Convert(scopeTracking, typeof(object)))), context.RezolveContextParameter);
                    var lazyLambda = (Func<RezolveContext, Lazy<object>>)lazyLambdaExpr.Compile();
                    //now we create and capture an instance of the SingletonTargetLazyInitialiser class, passing our
                    //dynamically constructed delegate along with this target - and emit a call to its
                    //GetObject implementation
                    return Expression.Call(Expression.Constant(new SingletonTargetLazyInitialiser(this, lazyLambda), typeof(ICompiledRezolveTarget)), 
                        ICompiledRezolveTarget_GetObject, context.RezolveContextParameter);
                }
            }            
		}

		public override bool SupportsType(Type type)
		{
			return _innerTarget.SupportsType(type);
		}

		public override Type DeclaredType
		{
			get { return _innerTarget.DeclaredType; }
		}
	}

    /// <summary>
    /// Extension method(s) to convert targets into singleton targets.
    /// </summary>
    public static class IRezolveTargetSingletonExtensions
    {
        /// <summary>
        /// Constructs a <see cref="SingletonTarget"/> that wraps the target on which the method is invoked.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static SingletonTarget Singleton(this IRezolveTarget target)
        {
            target.MustNotBeNull(nameof(target));
            return new SingletonTarget(target);
        }
    }
}
