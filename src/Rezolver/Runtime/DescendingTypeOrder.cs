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
                return 0;

            // if x is a base of y then it's greater than y (and vice-versa)
            // if x is an interface of y then it's greater than y (and vice-versa)

            if (TypeHelpers.IsClass(x))
            {
                if (TypeHelpers.IsClass(y))
                {
                    if (TypeHelpers.IsSubclassOf(x, y))
                        return -1;
                    else if (TypeHelpers.IsSubclassOf(y, x))
                        return 1;
                }
                else if (TypeHelpers.IsInterface(y))
                {
                    // y is an interface of x
                    if (TypeHelpers.IsAssignableFrom(y, x))
                        return -1;
                }
            }
            else if (TypeHelpers.IsInterface(x))
            {
                if (TypeHelpers.IsClass(y))
                {
                    // x is an interface of y
                    if (TypeHelpers.IsAssignableFrom(x, y))
                        return 1;
                }
                else if (TypeHelpers.IsInterface(y))
                {
                    if (TypeHelpers.GetInterfaces(x).Contains(y))
                        return -1;
                    else if (TypeHelpers.GetInterfaces(y).Contains(x))
                        return 1;
                }
            }

            if (TypeHelpers.IsGenericType(x))
            {
                if (TypeHelpers.IsGenericType(y))
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
                if (TypeHelpers.IsGenericType(y)) //place nongeneric types after generic types.
                {
                    return 1;
                }

            }

            return 0;
        }
    }
}
