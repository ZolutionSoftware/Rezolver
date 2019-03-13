// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rezolver
{

    internal class TargetTypeSelectorParams
    {
        private static bool? GetInternalContravarianceOverride(Type type) =>
            type.GetCustomAttributes<Runtime.ContravarianceAttribute>(false).SingleOrDefault()?.Enable;

        public TargetTypeSelectorParams Parent { get; }

        public Type Type { get; }

        public Type TypeParameter { get; }

        /// <summary>
        /// <c>true</c> if contravariance (and covariance when it's implemented) are to be allowed
        /// for this search.  This acts as a master gate for all variance - e.g. even if a type parameter
        /// is contravariant, setting this to <c>false</c> will prevent contravariant searching.
        ///
        /// Further checks are then performed for the individual types of variant searching to detect if they're
        /// to be used.
        /// </summary>
        public bool EnableVariance { get; }

        /// <summary>
        /// If Contravariant type searches are enabled, this controls the types
        /// which are to be considered.
        /// </summary>
        public Contravariance Contravariance { get; }

        public ITargetContainer RootTargets { get; }

        private readonly bool? _forceContravariance;
        /// <summary>
        /// If <c>null</c>, enabling of contravariance for the search type is determined
        /// by a target container option (or internal attribute for internal types).
        ///
        /// If not null, contravariant searches are only performed for the search type
        /// if it is <c>true</c>.
        /// </summary>
        /// <remarks>This is required for when options are sought from a target container
        /// because the <see cref="Options.IOptionContainer{TService, TOption}"/> interface
        /// is contravariant and allows us to set option on a per-service bases, contravariant
        /// searching is forced to <c>true</c> for those, and is forced to <c>false</c>
        /// for <see cref="Options.IOptionContainer{TOption}"/>, to avoid endless recursion into the
        /// options API.
        ///
        /// This is demonstrated by the paradox created when using the
        /// <see cref="Options.EnableContravariance"/> option to disable contravariance for a
        /// type and any of its derivatives or implementations - that option lookup *requires*
        /// contravariance for that type to be enabled in order to work!</remarks>
        public bool? ForceContravariance
        {
            get
            {
                return this._forceContravariance ?? Parent?.ForceContravariance;
            }
        }

        public IEnumerable<Type> TypeParameterChain
        {
            get
            {
                var current = this;
                Type last = null;
                while (current != null && current.TypeParameter != null)
                {
                    if (current.TypeParameter != last)
                    {
                        yield return current.TypeParameter;
                    }

                    current = current.Parent;
                }
            }
        }

        public bool TypeParameterIsVariant
        {
            get
            {
                return TypeParameter?.IsVariantTypeParameter() ?? false;
            }
        }

        public bool TypeParameterIsContravariant
        {
            get
            {
                return TypeParameter?.IsContravariantTypeParameter() ?? false;
            }
        }

        public bool TypeParameterIsCovariant
        {
            get
            {
                return TypeParameter?.IsCovariantTypeParameter() ?? false;
            }
        }

        public TargetTypeSelectorParams(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Constructor used for the top-level search
        /// </summary>
        /// <param name="type"></param>
        /// <param name="owner"></param>
        public TargetTypeSelectorParams(Type type, TargetTypeSelector owner)
            : this(type)
        {
            RootTargets = owner?.RootTargets;
            // variance always starts off enabled.
            EnableVariance = true;

            bool enableContravariance = true;

            this._forceContravariance = GetInternalContravarianceOverride(Type);
            if (this._forceContravariance == null)
            {
                if (RootTargets != null)
                    enableContravariance = RootTargets.GetOption(
                        Type, Options.EnableContravariance.Default);
            }
            else
            {
                enableContravariance = this._forceContravariance.Value;
            }

            if (enableContravariance)
            {
                Contravariance = Contravariance.BasesAndInterfaces;
            }
            else
            {
                Contravariance = Contravariance.None;
            }
        }

        public TargetTypeSelectorParams(Type type,
            Type typeParameter = null,
            TargetTypeSelectorParams parent = null,
            Contravariance? contravariantSearchType = null)
            : this(type)
        {
            Parent = parent;
            RootTargets = parent?.RootTargets;
            TypeParameter = typeParameter;
            // We enable variance if we have no parent
            // Or if we have a variant type parameter and
            // the parent hasn't disabled variance.
            EnableVariance = parent == null ||
                (TypeParameterIsVariant && Parent.EnableVariance);

            if (EnableVariance)
            {
                bool enableContravariance;
                if (ForceContravariance == null)
                {
                    // start off always enabled
                    enableContravariance = true;
                    var overridenContravariance = GetInternalContravarianceOverride(Type);

                    if (overridenContravariance != null)
                    {
                        // once it's forced, all child searches will avoid testing the EnableContravariance option
                        this._forceContravariance = overridenContravariance;
                        enableContravariance = overridenContravariance.Value;
                    }
                    else if (RootTargets != null)
                        enableContravariance = RootTargets.GetOption(
                            Type, Options.EnableContravariance.Default);
                }
                else
                {
                    enableContravariance = ForceContravariance.Value;
                }

                if (contravariantSearchType != null)
                {
                    Contravariance = contravariantSearchType.Value;
                }
                else
                {
                    if (!enableContravariance)
                    {
                        Contravariance = Contravariance.None;
                    }
                    else
                    {
                        // if the parent has its contravariance search set to None, we inherit that
                        // and move on.
                        if (Parent?.Contravariance == Contravariance.None)
                        {
                            Contravariance = Contravariance.None;
                        }
                        else
                        {
                            var numContras = TypeParameterChain.Count(t => t.IsContravariantTypeParameter());
                            if (numContras <= 1 || (numContras % 2) == 1)
                            {
                                Contravariance = Contravariance.BasesAndInterfaces;
                            }
                            else
                            {
                                Contravariance = Contravariance.Derived;
                            }
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            if (TypeParameter != null)
            {
                return $"{TypeParameter.Name}(#{TypeParameter.GenericParameterPosition}) = {Type.CSharpLikeTypeName()} for {Parent}";
            }
            else
            {
                return Type.CSharpLikeTypeName();
            }
        }
    }
}
