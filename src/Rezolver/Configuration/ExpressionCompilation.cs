// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using Rezolver.Targets;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Implementation of <see cref="IContainerConfig"/> which configures expression-based compilation for targets in containers.
    ///
    /// The implementation sets all options and registers all the targets necessary to use the expression tree-based compilation provided by
    /// the <see cref="ExpressionCompiler"/>
    ///
    /// This is included in the <see cref="Container.DefaultConfig"/>, meaning that all containers
    /// created without a specific config will automatically be configured to use the <see cref="ExpressionCompiler"/>.</summary>
    public class ExpressionCompilation : IContainerConfig<ITargetCompiler>
    {
        /// <summary>
        /// The one and only instance of <see cref="ExpressionCompilation"/>
        /// </summary>
        public static IContainerConfig<ITargetCompiler> Instance { get; } = new ExpressionCompilation();

        private ExpressionCompilation()
        {
        }

        private class ExpressionBuilderRegistration
        {
            public IExpressionBuilder Instance;
            public Type ExpressionBuilderType;
            public Type TargetType;
        }

        private static IEnumerable<ExpressionBuilderRegistration> SearchForStandardTargetBuilders()
        {
            // TODO: Consider adding the ability to provide additional assemblies and/or individual builder
            // types by adding a new concrete behaviour type specifically geared around registering additional
            // builders.  This means that applications can extend this compiler behaviour simply by adding
            // instances of that new behaviour type and this class will automatically use them.

            var rezolverAssembly = typeof(IContainer).Assembly;
            // the well-known target types for compilation are ICompiledTarget, plus all the concrete target types in Rezolver.Targets
            foreach (var type in rezolverAssembly.ExportedTypes.Where(t =>
                t == typeof(ICompiledTarget) ||
                (t.Namespace == typeof(ObjectTarget).Namespace && t.IsPublic && !t.IsAbstract && t.IsClass)))
            {
                var builderInterfaceType = typeof(IExpressionBuilder<>).GetTypeInfo().MakeGenericType(type);
                // attempt to find a single expressionbuilder type in this assembly which implements the builderInterfaceType
                var possibleTypes = rezolverAssembly.DefinedTypes.Where(t => t.IsClass
                    && !t.ContainsGenericParameters
                    && !t.IsAbstract
                    && t.ImplementedInterfaces.Contains(builderInterfaceType)).ToArray();
                if (possibleTypes.Length == 1)
                {
                    // doesn't matter if the constructor is public or not
                    var defaultConstructor = possibleTypes[0].DeclaredConstructors
                        .Where(c => !c.IsStatic).SingleOrDefault(c => c.GetParameters().Length == 0);
                    if (defaultConstructor != null)
                    {
                        yield return new ExpressionBuilderRegistration()
                        {
                            TargetType = type,
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

        /// <summary>
        /// Implements the <see cref="IContainerConfig.Configure(IContainer, IRootTargetContainer)"/> method,
        /// registering all the targets necessary to use expression-based compilation for all the standard targets
        /// defined in the <c>Rezolver</c> core library.
        /// </summary>
        /// <param name="container">The container - ignored.</param>
        /// <param name="targets">Required - the target container into which the various targets will be registered.</param>
        /// <remarks>All targets registered by this function are <see cref="ObjectTarget"/> targets backed by concrete instances
        /// of the various components (compiler etc).</remarks>
        public virtual void Configure(IContainer container, IRootTargetContainer targets)
        {
            targets.MustNotBeNull(nameof(targets));
            // note - the singleton container is done like this because the expression compiler which uses
            // it Resolves it from the container during compilation.  This is to ensure that each container
            // gets the same singleton container (including OverridingContainer), rather than a single shared one
            // from the target container option.
            targets.RegisterObject(new SingletonTarget.SingletonContainer());
            // targets.RegisterObject(new ExpressionBuilderCache(container));
            // will be how containers pick up and use this compiler
            targets.SetOption<ITargetCompiler>(ExpressionCompiler.Default);
            // if you're looking to re-enter the compilation process for a particular
            // target - then you should request our compiler via the type IExpressionCompiler
            targets.SetOption<IExpressionCompiler>(ExpressionCompiler.Default);

            // loop through all the types in the core Rezolver assembly's Rezolver.Targets namespace, searching for an implementing
            // type in this assembly
            foreach (var registration in GetStandardTargetBuilders())
            {
                targets.SetOption(registration.Instance, registration.TargetType);
            }
        }
    }
}
