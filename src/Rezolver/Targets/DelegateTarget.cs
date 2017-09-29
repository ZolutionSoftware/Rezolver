// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
	/// <summary>
	/// An <see cref="ITarget" /> which resolve objects by executing a delegate with argument injection.
	/// </summary>
	/// <remarks>The delegate must be non-void and can have any number of parameters.
	/// 
	/// A compiler must ensure that any parameters for the <see cref="Factory"/> are automatically 
	/// resolved from the container, and that a parameter of the type <see cref="IResolveContext"/> 
	/// will receive the context passed to the <see cref="IContainer.Resolve(IResolveContext)"/>
	/// method call for which this target is being compiled and/or executed.</remarks>
 	public class DelegateTarget : TargetBase
	{
		/// <summary>
		/// Gets the factory delegate that will be invoked when this target is compiled and executed
		/// </summary>
		/// <value>The factory.</value>
		public Delegate Factory { get; }
		/// <summary>
		/// Gets the MethodInfo for the <see cref="Factory"/> delegate.
		/// </summary>
		/// <remarks>Whilst this can be easily obtained from the delegate yourself (by using the 
		/// <see cref="RuntimeReflectionExtensions.GetMethodInfo(Delegate)"/> extension method) however, this 
		/// class also uses it to determine the <see cref="DeclaredType"/> of the target or whether the delegate 
		/// is actually compatible with the one supplied on construction, therefore if you need to introspect 
		/// the delegate, you might as well use this.</remarks>
		public MethodInfo FactoryMethod { get; }

		private Type _declaredType;

		/// <summary>
		/// Gets the declared type of object that is constructed by this target, either set on 
		/// construction or derived from the return type of the <see cref="Factory"/>
		/// </summary>
		public override Type DeclaredType
		{
			get
			{
				return _declaredType ?? FactoryMethod.ReturnType;
			}
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateTarget" /> class.
        /// </summary>
        /// <param name="factory">Required - the factory delegate.  Must have a return type and can take
        /// 0 or more parameters.</param>
        /// <param name="declaredType">Optional - type that will be set into the <see cref="DeclaredType" /> for the target;
        /// if not provided, then it will be derived from the <paramref name="factory" />'s return type</param>
        /// <exception cref="ArgumentException">If the <paramref name="factory" /> represents a void delegate or if
        /// <paramref name="declaredType" /> is passed but the type is not compatible with the return type of
        /// <paramref name="factory" />.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="factory" /> is null</exception>
        public DelegateTarget(Delegate factory, Type declaredType = null)
		{
			factory.MustNotBeNull(nameof(factory));
            FactoryMethod = factory.GetMethodInfo();
            FactoryMethod.MustNot(m => FactoryMethod.ReturnType == null || FactoryMethod.ReturnType == typeof(void), "Factory must have a return type", nameof(factory));
            FactoryMethod.MustNot(m => m.GetParameters().Any(p => p.ParameterType.IsByRef), "Delegates which have ref or out parameters are not permitted as the factory argument", nameof(factory));

            if (declaredType != null)
            {
                if (!TypeHelpers.AreCompatible(FactoryMethod.ReturnType, declaredType) && !TypeHelpers.AreCompatible(declaredType, FactoryMethod.ReturnType))
                    throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, FactoryMethod.ReturnType), nameof(declaredType));
            }

            _declaredType = declaredType;
            Factory = factory;
		}
	}
}
