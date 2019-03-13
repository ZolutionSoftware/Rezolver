// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Runtime
{

    /// <summary>
    /// Generates a reasonable ordering of types where:
    /// Derived types are sorted before bases
    /// Interface implementations are sorted before the interfaces
    /// Fully-closed generics are sorted earlier than any containing generic parameters (or which are open).
    /// Generics are also sorted before non-generics.  It's not a general-purpose type sort: it's specifically
    /// useful for our TargetTypeSelector.
    /// </summary>
    internal class DescendingTypeOrder : IComparer<Type>
    {
        public static readonly DescendingTypeOrder Instance = new DescendingTypeOrder();

        private DescendingTypeOrder() { }

        public int Compare(Type x, Type y)
        {
            if (x == y)
            {
                return 0;
            }

            // if x is a base of y then it's greater than y (and vice-versa)
            // if x is an interface of y then it's greater than y (and vice-versa)

            if (x.IsClass)
            {
                if (y.IsClass)
                {
                    if (x.IsSubclassOf(y))
                    {
                        return -1;
                    }
                    else if (y.IsSubclassOf(x))
                    {
                        return 1;
                    }
                }
                else if (y.IsInterface)
                {
                    // y is an interface of x
                    if (y.IsAssignableFrom(x))
                    {
                        return -1;
                    }
                }
            }
            else if (x.IsInterface)
            {
                if (y.IsClass)
                {
                    // x is an interface of y
                    if (x.IsAssignableFrom(y))
                    {
                        return 1;
                    }
                }
                else if (y.IsInterface)
                {
                    if (x.GetInterfaces().Contains(y))
                    {
                        return -1;
                    }
                    else if (y.GetInterfaces().Contains(x))
                    {
                        return 1;
                    }
                }
            }

            if (x.IsGenericType)
            {
                if (y.IsGenericType)
                {
                    if (x.IsConstructedGenericType)
                    {
                        if (y.IsConstructedGenericType)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (y.IsConstructedGenericType)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y.IsGenericType) // place nongeneric types after generic types.
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}
