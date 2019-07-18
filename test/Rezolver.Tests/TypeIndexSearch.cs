using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Tests
{
    internal readonly struct TypeIndexSearch
    {
        public bool KnownGenericsOnly { get; }
        public bool IncludeClassVariants { get; }
        public bool IncludeInterfaceVariants { get; }
        public bool IncludeGenericDefinition { get; }
        public TypeIndexEntry TypeParameter { get; }
        public int NumContra { get; }
        public bool EnableVariance { get; }
        public List<(TypeIndexEntry entry, bool variantMatch)> Results { get; }
        public bool ResultsAreVariant { get; }

        /// <summary>
        /// negative is bases/interfaces; positive is derived types or implementations.
        /// </summary>
        public int VarianceDirection { get; }

        public TypeIndexSearch(
            bool knownGenericsOnly = true,
            bool includeClassVariants = false,
            bool includeInterfaceVariants = false,
            bool includeGenericDefinition = true,
            bool enableVariance = true,
            TypeIndexEntry typeParameter = null)
        {
            KnownGenericsOnly = knownGenericsOnly;
            TypeParameter = typeParameter;
            ResultsAreVariant = false;
            VarianceDirection = 0;
            NumContra = 0;

            if (TypeParameter != null)
            {
                if (!TypeParameter.IsVariantTypeParameter)
                {
                    EnableVariance = false;
                    IncludeClassVariants = false;
                    IncludeInterfaceVariants = false;
                }
                else
                {
                    EnableVariance = enableVariance;
                    IncludeClassVariants = includeClassVariants;
                    IncludeInterfaceVariants = includeInterfaceVariants;
                    if (TypeParameter.IsCovariantTypeParameter)
                    {
                        VarianceDirection = 1; // derived types/implementations
                    }
                    else  // must be contravariant because of EnableVariance assignment further up
                    {
                        VarianceDirection = -1; //bases/interfaces
                        NumContra = 1;
                    }
                }
            }
            else
            {
                EnableVariance = enableVariance;
                IncludeClassVariants = includeClassVariants;
                IncludeInterfaceVariants = includeInterfaceVariants;
            }

            IncludeGenericDefinition = includeGenericDefinition;
            Results = new List<(TypeIndexEntry entry, bool variantMatch)>(50);
        }

        public TypeIndexSearch(
            in TypeIndexSearch parent,
            bool includeClassVariants = false,
            bool includeInterfaceVariants = false,
            bool includeGenericDefinition = true,
            TypeIndexEntry typeParameter = null,
            bool resultsAreVariant = false,
            bool useNewResultsList = false)
        {
            KnownGenericsOnly = parent.KnownGenericsOnly;
            IncludeGenericDefinition = includeGenericDefinition;
            TypeParameter = typeParameter;
            Results = useNewResultsList ? new List<(TypeIndexEntry entry, bool variantMatch)>() : parent.Results;
            ResultsAreVariant = resultsAreVariant;

            if (TypeParameter != null && TypeParameter == parent.TypeParameter)
            {
                // same type parameter as the source, so we set the three supplied arguments,
                // and inherit (rather than calculate) the rest.
                IncludeClassVariants = includeClassVariants;
                IncludeInterfaceVariants = includeInterfaceVariants;
                NumContra = parent.NumContra;
                EnableVariance = parent.EnableVariance;
                VarianceDirection = parent.VarianceDirection;
                ResultsAreVariant = parent.ResultsAreVariant;
            }
            else
            {
                EnableVariance = parent.EnableVariance && (TypeParameter?.IsVariantTypeParameter ?? true);

                if (!EnableVariance)
                {
                    VarianceDirection = 0;
                    IncludeClassVariants = false;
                    IncludeInterfaceVariants = false;
                    NumContra = 0;
                }
                else
                {
                    IncludeClassVariants = includeClassVariants;
                    IncludeInterfaceVariants = includeInterfaceVariants;
                    NumContra = parent.NumContra + (TypeParameter.IsContravariantTypeParameter ? 1 : 0);
                    if (TypeParameter.IsCovariantTypeParameter)
                    {
                        VarianceDirection = 1; // derived types/implementations
                    }
                    else  // must be contravariant because of EnableVariance assignment further up
                    {
                        VarianceDirection = -1; //bases/interfaces
                    }
                }
            }
        }

        public void AddResult(TypeIndexEntry entry, bool? variant = null)
        {
            Results.Add((entry, variant ?? ResultsAreVariant));
        }

        public void AddResults(IEnumerable<TypeIndexEntry> entries, bool? variant = null)
        {
            var defaultVariant = ResultsAreVariant;
            Results.AddRange(entries.Select(e => (e, variant ?? defaultVariant)));
        }
    }
}
