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
                _set = set;
                _version = set._version;
                _node = set._linkedList.Last;
                _current = default(T);
                _index = 0;
            }

            public T Current
            {
                get
                {
                    if (_index == 0 || _index == _set.Count + 1)
                        throw new InvalidOperationException("Enumeration can't happen");
                    return _current;
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
                if (_version != _set._version)
                    throw new InvalidOperationException("OrderedSet has been modified");
                if(_node == null)
                {
                    _index = _set.Count + 1;
                    return false;
                }
                _index++;
                _current = _node.Value;
                _node = _node.Previous;
                if(_node == _set._linkedList.Last)
                    _node = null;
                return true;
            }

            public void Reset()
            {
                if (_version != _set._version)
                    throw new InvalidOperationException("OrderedSet has been modified");
                _current = default(T);
                _node = _set._linkedList.Last;
                _index = 0;
            }
        }

        private struct ReverseEnumerable : IEnumerable<T>
        {
            private OrderedSet<T> _set;
            public ReverseEnumerable(OrderedSet<T> set)
            {
                _set = set;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new ReverseEnumerator(_set);
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
            _dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            _linkedList = new LinkedList<T>();
        }

        public IEnumerable<T> Reverse()
        {
            return new ReverseEnumerable(this);
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Add(T item)
        {
            if (_dictionary.ContainsKey(item)) return false;
            LinkedListNode<T> node = _linkedList.AddLast(item);
            _dictionary.Add(item, node);
            ++_version;
            return true;
        }

        public void Clear()
        {
            _linkedList.Clear();
            _dictionary.Clear();
            ++_version;
        }

        public bool Remove(T item)
        {
            bool found = _dictionary.TryGetValue(item, out var node);
            if (!found) return false;
            _dictionary.Remove(item);
            _linkedList.Remove(node);
            ++_version;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _linkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _linkedList.CopyTo(array, arrayIndex);
        }
    }
}
