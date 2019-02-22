// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Decent enough class from https://stackoverflow.com/a/17853085/157701
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OrderedSet<T> : ICollection<T>
    {
        #region Reverse enumerable implementation
        private struct ReverseEnumerator : IEnumerator<T>
        {
            // code largely ripped from the LinkedList<T> enumerator

            private readonly int _version;
            private readonly OrderedSet<T> _set;
            private LinkedListNode<T> _node;
            private T _current;
            private int _index;

            public ReverseEnumerator(OrderedSet<T> set)
            {
                this._set = set;
                this._version = set._version;
                this._node = set._linkedList.Last;
                this._current = default(T);
                this._index = 0;
            }

            public T Current
            {
                get
                {
                    if (this._index == 0 || this._index == this._set.Count + 1)
                    {
                        throw new InvalidOperationException("Enumeration can't happen");
                    }

                    return this._current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this._version != this._set._version)
                {
                    throw new InvalidOperationException("OrderedSet has been modified");
                }

                if (this._node == null)
                {
                    this._index = this._set.Count + 1;
                    return false;
                }

                this._index++;
                this._current = this._node.Value;
                this._node = this._node.Previous;
                if (this._node == this._set._linkedList.Last)
                {
                    this._node = null;
                }

                return true;
            }

            public void Reset()
            {
                if (this._version != this._set._version)
                {
                    throw new InvalidOperationException("OrderedSet has been modified");
                }

                this._current = default(T);
                this._node = this._set._linkedList.Last;
                this._index = 0;
            }
        }

        private struct ReverseEnumerable : IEnumerable<T>
        {
            private readonly OrderedSet<T> _set;

            public ReverseEnumerable(OrderedSet<T> set)
            {
                this._set = set;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new ReverseEnumerator(this._set);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion

        private int _version = 0;
        private readonly IDictionary<T, LinkedListNode<T>> _dictionary;
        private readonly LinkedList<T> _linkedList;

        public OrderedSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public OrderedSet(IEqualityComparer<T> comparer)
        {
            this._dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            this._linkedList = new LinkedList<T>();
        }

        public IEnumerable<T> Reverse()
        {
            return new ReverseEnumerable(this);
        }

        public int Count
        {
            get { return this._dictionary.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return this._dictionary.IsReadOnly; }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Add(T item)
        {
            if (this._dictionary.ContainsKey(item))
            {
                return false;
            }

            LinkedListNode<T> node = this._linkedList.AddLast(item);
            this._dictionary.Add(item, node);
            ++this._version;
            return true;
        }

        public void Clear()
        {
            this._linkedList.Clear();
            this._dictionary.Clear();
            ++this._version;
        }

        public bool Remove(T item)
        {
            bool found = this._dictionary.TryGetValue(item, out var node);
            if (!found)
            {
                return false;
            }

            this._dictionary.Remove(item);
            this._linkedList.Remove(node);
            ++this._version;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._linkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return this._dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._linkedList.CopyTo(array, arrayIndex);
        }
    }
}
