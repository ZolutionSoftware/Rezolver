// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Simple implementation of a locked list - used in situations where random access is required over
    /// a collection of objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LockedList<T> : IList<T>
    {
        public class ListLock : IDisposable
        {
            private readonly LockedList<T> _list;

            internal ListLock(LockedList<T> list)
            {
                this._list = list;
                Monitor.Enter(this._list._locker);
            }

            public void Dispose()
            {
                if (Monitor.IsEntered(this._list._locker))
                {
                    Monitor.Exit(this._list._locker);
                }
            }
        }

        private readonly object _locker = new object();
        private readonly List<T> _inner;

        public int Count
        {
            get
            {
                lock (this._locker)
                {
                    return this._inner.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock (this._locker)
                {
                    return ((IList<T>)this._inner).IsReadOnly;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (this._locker)
                {
                    return this._inner[index];
                }
            }

            set
            {
                lock (this._locker)
                {
                    this._inner[index] = value;
                }
            }
        }

        public LockedList(IEnumerable<T> enumerable)
        {
            this._inner = new List<T>(enumerable);
        }

        public LockedList(int capacity)
        {
            this._inner = new List<T>(capacity);
        }

        public LockedList()
        {
            this._inner = new List<T>();
        }

        public IDisposable Lock()
        {
            return new ListLock(this);
        }

        public int IndexOf(T item)
        {
            lock (this._locker)
            {
                return this._inner.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (this._locker)
            {
                this._inner.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (this._locker)
            {
                this._inner.RemoveAt(index);
            }
        }

        public void Add(T item)
        {
            lock (this._locker)
            {
                this._inner.Add(item);
            }
        }

        public void Clear()
        {
            lock (this._locker)
            {
                this._inner.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (this._locker)
            {
                return this._inner.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this._locker)
            {
                this._inner.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (this._locker)
            {
                return this._inner.Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this._locker)
            {
                var clone = this._inner.ToArray();
                return clone.AsEnumerable().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
