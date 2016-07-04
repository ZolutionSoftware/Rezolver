// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
#if PORTABLE
    /// <summary>
    /// Simple locking implementation of ConcurrentBag for portable platforms that don't support it natively.
    /// 
    /// This implementation operates over a synchronised stack.
    /// </summary>
    internal class ConcurrentBag<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        private readonly object _locker = new object();
        private readonly Stack<T> _wrappedStack;
        public ConcurrentBag()
        {
            _wrappedStack = new Stack<T>();
        }

        public ConcurrentBag(IEnumerable<T> collection)
            : this()
        {
            _wrappedStack = new Stack<T>(collection);
        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _wrappedStack.Count;
                }
            }
        }

        public bool IsEmpty { get { return Count == 0; } }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return _locker;
            }
        }

        public void Add(T item)
        {
            lock (_locker)
            {
                _wrappedStack.Push(item);
            }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (_locker)
            {
                _wrappedStack.CopyTo(array, index);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_locker)
            {
                return _wrappedStack.ToArray().Cast<T>().GetEnumerator();
            }
        }

        public T[] ToArray()
        {
            lock (_locker)
            {
                return _wrappedStack.ToArray();
            }
        }

        public bool TryPeek(out T result)
        {
            lock (_locker)
            {
                result = default(T);
                if (_wrappedStack.Count == 0)
                    return false;
                result = _wrappedStack.Peek();
                return true;
            }
        }

        public bool TryTake(out T result)
        {
            lock (_locker)
            {
                result = default(T);
                if (_wrappedStack.Count == 0)
                    return false;
                result = _wrappedStack.Pop();
                return true;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            lock (_locker)
            {
                ((ICollection)_wrappedStack).CopyTo(array, index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
#endif
}
