using Rezolver.Targets;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Implements the <see cref="IContainerBehaviour"/> to configure expression-based compilation for targets in containers.
	/// 
	/// The implementation registers all the targets necessary to use the expression tree-based compilation provided by 
	/// the <c>Rezolver.Compilation.Expressions</c> library.
	/// 
	/// This configuration provider is automatically configured as the default for all containers when the Rezolver 
	/// library is referenced.</summary>
	public class ExpressionCompilerConfigurationProvider : IContainerBehaviour
	{
		/// <summary>
		/// Implements the <see cref="IContainerBehaviour.Configure(IContainer, ITargetContainer)"/> method,
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
            //extract the singleton to its own behaviour
            targets.RegisterObject(new SingletonTarget.SingletonContainer());
            targets.RegisterObject(new ExpressionBuilderCache(container));
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
                //NOTE: MUST pass the concrete type below as an argument to the declaredType parameter below
                //otherwise the object target is created as IExpressionBuilder, which is not capable of
                //handling the type represented by ExpressionBuilderType - because it is a base interface
                //to that type, not a derived interface.
				if (targets.Fetch(registration.ExpressionBuilderType) == null)
					targets.RegisterObject(registration.Instance, declaredType: registration.ExpressionBuilderType);
			}
		}

		private class ExpressionBuilderRegistration
		{
			public IExpressionBuilder Instance;
			public Type ExpressionBuilderType;
		}
		private static IEnumerable<ExpressionBuilderRegistration> SearchForStandardTargetBuilders()
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

		private static Lazy<IEnumerable<ExpressionBuilderRegistration>> _standardTargetBuilders = new Lazy<IEnumerable<ExpressionBuilderRegistration>>(
			() => new List<ExpressionBuilderRegistration>(SearchForStandardTargetBuilders()));
		
		
		private static IEnumerable<ExpressionBuilderRegistration> GetStandardTargetBuilders()
		{
			return _standardTargetBuilders.Value;
		}

        public IEnumerable<IContainerBehaviour> GetDependencies(IEnumerable<IContainerBehaviour> behaviours)
        {
            throw new NotImplementedException();
        }
    }
}
