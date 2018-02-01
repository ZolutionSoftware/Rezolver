// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    internal static class ReadOnlyEnumerableExtensions
    {
        /// <summary>
        /// readonly wrapper for generic IEnumerables
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class ReadOnlyCollection<T> : IList<T>
        {
            private T[] _range;

            public ReadOnlyCollection(IEnumerable<T> range)
            {
                this._range = range.ToArray();
            }

            public T this[int index]
            {
                get
                {
                    return this._range[index];
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            public int Count
            {
                get
                {
                    return this._range.Length;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public void Add(T item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(T item)
            {
                return this._range.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                this._range.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this._range.Cast<T>().GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return Array.IndexOf(this._range, item, 0, this._range.Length);
            }

            public void Insert(int index, T item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public static IList<T> AsReadOnly<T>(this IEnumerable<T> range)
        {
            range.MustNotBeNull(nameof(range));
            return new ReadOnlyCollection<T>(range);
        }
    }
}
