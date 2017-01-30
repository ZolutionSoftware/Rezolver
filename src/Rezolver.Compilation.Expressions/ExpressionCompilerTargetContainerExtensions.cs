// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rezolver.Compilation;
using Rezolver.Targets;
using System.Reflection;

namespace Rezolver
{
	public static class ExpressionCompilerTargetContainerExtensions
	{
		public static ITargetContainer UseExpressionCompiler(this ITargetContainer targets, ExpressionCompiler compiler = null)
		{
			compiler = compiler ?? ExpressionCompiler.Default;
			//will be how containers pick up and use this compiler
			targets.RegisterObject<ITargetCompiler>(compiler);
			//if you're looking to re-enter the compilation process for a particular
			//target - then you should request our compiler via the type IExpressionCompiler 
			targets.RegisterObject<IExpressionCompiler>(compiler);
			targets.RegisterObject<ICompileContextProvider>(compiler);
			//and then we have all the expression builders.
			//TODO: Implement a RegisterExpressionBuilder<TTarget>(this ITargetContainer, IExpressionBuilder<TTarget>>) method

			//loop through all the types in the core Rezolver assembly's Rezolver.Targets namespace, searching for an implementing
			//type in this assembly
			foreach (var registration in GetStandardTargetBuilders())
			{
				if (targets.Fetch(registration.ExpressionBuilderType) == null)
					targets.RegisterObject(registration.Instance, registration.ExpressionBuilderType);
			}

			//targets.RegisterObject<IExpressionBuilder<ConstructorTarget>>(new ConstructorTargetBuilder());

			return targets;
		}

		private class ExpressionBuilderRegistration
		{
			public IExpressionBuilder Instance;
			public Type ExpressionBuilderType;
		}

		private static IEnumerable<ExpressionBuilderRegistration> GetStandardTargetBuilders()
		{
			var rezolverAssembly = TypeHelpers.GetAssembly(typeof(IContainer));
			var thisAssembly = TypeHelpers.GetAssembly(typeof(ExpressionCompilerTargetContainerExtensions));
			//loop through all target types defined in the Rezolver assembly which reside in the same
			//namespace as one of the well-known targets and which are non-abstract classes
			foreach (var type in rezolverAssembly.ExportedTypes.Where(t =>
				 t.Namespace == typeof(ObjectTarget).Namespace && TypeHelpers.IsPublic(t) && !TypeHelpers.IsAbstract(t) && TypeHelpers.IsClass(t)))
			{
				var builderInterfaceType = typeof(IExpressionBuilder<>).GetTypeInfo().MakeGenericType(type);
				//attempt to find a single expressionbuilder type in this assembly which implements the builderInterfaceType
				var possibleTypes = thisAssembly.DefinedTypes.Where(t => t.ImplementedInterfaces.Contains(builderInterfaceType)).ToArray();
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