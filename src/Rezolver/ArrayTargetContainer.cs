// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Inheriting from TargetDictionary allows the container to support direct registrations
    /// for concrete array types.
    /// </summary>
    internal class ArrayTargetContainer : TargetDictionaryContainer
    {
        internal static readonly MethodInfo LinqToArrayMethod
            = Extract.Method(() => Enumerable.ToArray<int>(null)).GetGenericMethodDefinition();

        public ArrayTargetContainer(IRootTargetContainer root)
            : base(root)
        {
        }

        public override ITarget Fetch(Type type)
        {
            if (!type.IsArray)
            {
                throw new ArgumentException($"This target container only supports array types - {type} is not an array type");
            }

            var result = base.Fetch(type);
            if (result != null && !result.UseFallback)
            {
                return result;
            }

            // check the array rank - if it's greater than 1, then we can't do anything
            if (type.GetArrayRank() != 1)
            {
                throw new ArgumentException($"Arrays of rank 2 cannot be injected automatically - {type} has a rank of {type.GetArrayRank()}");
            }

            // TODO: see also EnumerableTargetContainer's Fetch method
            // TODO: look at subclassing a version of this which 'knows' about the
            // overriding container so that we can get rid of the forking here.
            if (Root is OverridingTargetContainer overridingContainer)
            {
                // if the
                result = overridingContainer.Parent.Fetch(type);
                if (!(result is ArrayTarget) && !(result?.UseFallback ?? true))
                {
                    return result;
                }
            }

            return ArrayTarget.Create(type);
        }

        internal class ArrayTarget : DelegateTarget
        {
            public ArrayTarget(Type arrayType, Type elementType)
                : base(
                      LinqToArrayMethod.MakeGenericMethod(elementType).CreateDelegate(
                            typeof(Func<,>).MakeGenericType(
                                typeof(IEnumerable<>).MakeGenericType(elementType),
                                arrayType)),
                        arrayType)
            {
            }

            public static ArrayTarget Create(Type arrayType)
            {
                // Now, technically arrays are covariant and therefore
                // should contain any object whose type is equal to or derived from
                // the desired element type.  Need to figure that one out.
                Type elementType = TypeHelpers.GetElementType(arrayType);

                return new ArrayTarget(arrayType, elementType);
            }
        }
    }
}
