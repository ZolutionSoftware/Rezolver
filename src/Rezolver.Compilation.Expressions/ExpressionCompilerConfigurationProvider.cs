using Rezolver.Targets;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Implements the <see cref="ICompilerConfigurationProvider"/> to configure expression-based compilation for targets in containers.
	/// 
	/// The implementation registers all the targets necessary to use the expression tree-based compilation provided by 
	/// the <c>Rezolver.Compilation.Expressions</c> library.
	/// 
	/// Simply call <see cref="ExpressionCompiler.UseAsDefaultCompiler"/> to use this provider by default for all containers.</summary>
	/// <seealso cref="Rezolver.Compilation.ICompilerConfigurationProvider" />
	public class ExpressionCompilerConfigurationProvider : ICompilerConfigurationProvider
	{
		/// <summary>
		/// Implements the <see cref="ICompilerConfigurationProvider.Configure(IContainer, ITargetContainer)"/> method,
		/// registering all the targets necessary to use expression-based compilation for all the standard targets
		/// defined in the <c>Rezolver</c> core library.
		/// </summary>
		/// <param name="container">The container - ignored.</param>
		/// <param name="targets">Required - the target container into which the various targets will be registered.</param>
		/// <remarks>All targets registered by this function are <see cref="ObjectTarget"/> targets backed by concrete instances
		/// of the various components (compiler etc).</remarks>
		public virtual void Configure(IContainer container, ITargetContainer targets)
		{
			targets.MustNotBeNull(nameof(targets));

			//will be how containers pick up and use this compiler
			targets.RegisterObject<ITargetCompiler>(ExpressionCompiler.Default);
			//if you're looking to re-enter the compilation process for a particular
			//target - then you should request our compiler via the type IExpressionCompiler 
			targets.RegisterObject<IExpressionCompiler>(ExpressionCompiler.Default);
			targets.RegisterObject<ICompileContextProvider>(ExpressionCompiler.Default);
			//and then we have all the expression builders.
			//TODO: Implement a RegisterExpressionBuilder<TTarget>(this ITargetContainer, IExpressionBuilder<TTarget>>) method

			//loop through all the types in the core Rezolver assembly's Rezolver.Targets namespace, searching for an implementing
			//type in this assembly
			foreach (var registration in GetStandardTargetBuilders())
			{
				if (targets.Fetch(registration.ExpressionBuilderType) == null)
					targets.RegisterObject(registration.Instance, registration.ExpressionBuilderType);
			}
		}

		private class ExpressionBuilderRegistration
		{
			public IExpressionBuilder Instance;
			public Type ExpressionBuilderType;
		}

		private static IEnumerable<ExpressionBuilderRegistration> GetStandardTargetBuilders()
		{
			var rezolverAssembly = TypeHelpers.GetAssembly(typeof(IContainer));
			var thisAssembly = TypeHelpers.GetAssembly(typeof(ExpressionCompilerConfigurationProvider));
			//loop through all target types defined in the Rezolver assembly which reside in the same
			//namespace as one of the well-known targets and which are non-abstract classes
			foreach (var type in rezolverAssembly.ExportedTypes.Where(t =>
				 t.Namespace == typeof(ObjectTarget).Namespace && TypeHelpers.IsPublic(t) && !TypeHelpers.IsAbstract(t) && TypeHelpers.IsClass(t)))
			{
				var builderInterfaceType = typeof(IExpressionBuilder<>).GetTypeInfo().MakeGenericType(type);
				//attempt to find a single expressionbuilder type in this assembly which implements the builderInterfaceType
				var possibleTypes = thisAssembly.DefinedTypes.Where(t => t.IsClass
					&& !t.ContainsGenericParameters 
					&& !t.IsAbstract 
					&& t.ImplementedInterfaces.Contains(builderInterfaceType)).ToArray();
				if (possibleTypes.Length == 1)
				{
					//doesn't matter if the constructor is public or not
					var defaultConstructor = possibleTypes[0].DeclaredConstructors
						.Where(c => !c.IsStatic).SingleOrDefault(c => c.GetParameters().Length == 0);
					if (defaultConstructor != null)
					{
						yield return new ExpressionBuilderRegistration()
						{
							ExpressionBuilderType = builderInterfaceType,
							Instance = (IExpressionBuilder)defaultConstructor.Invoke(new object[0])
						};
					}
				}
			}
		}
	}
}
