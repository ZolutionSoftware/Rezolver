// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
#if PORTABLE
    /// <summary>
    /// simple implementation of the ConcurrentDictionary class from the wider .Net framework.  Uses locking over a private
    /// dictionary to guarantee thread safety.  Performance is clearly nowhere near ConcurrentDictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        private readonly Dictionary<TKey, TValue> _wrappedDictionary;
        private readonly object _locker = new object();

        public ConcurrentDictionary()
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>();
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>();
            LoadFrom(collection);
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>(comparer);
            LoadFrom(collection);
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity)
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>(capacity);

        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>(comparer);
            LoadFrom(collection);
        }

        private void LoadFrom(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            //only valid to be called from the constructor - so no lock.
            foreach (var entry in collection)
            {
                //important to allow any ArgumentException raised here to bubble back up and out.
                _wrappedDictionary.Add(entry.Key, entry.Value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_locker)
                {
                    return _wrappedDictionary[key];
                }
            }

            set
            {
                lock (_locker)
                {
                    _wrappedDictionary[key] = value;
                }
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (key is TKey)
                {
                    TValue result;
                    if (TryGetValue((TKey)key, out result))
                        return result;
                }
                return null;
            }

            set
            {
                if (key is TKey)
                {
                    if (value is TValue || (typeof(TValue).IsClass && value == null))
                        this[(TKey)key] = (TValue)value;
                    else
                        throw new ArgumentException("Invalid type for value", nameof(value));
                }
                else
                    throw new ArgumentException("Invalid type for key", nameof(key));
            }
        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _wrappedDictionary.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_locker)
                {
                    return _wrappedDictionary.Keys.AsReadOnly();
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (_locker)
                {
                    return _wrappedDictionary.Values.AsReadOnly();
                }
            }
        }

        int ICollection.Count
        {
            get
            {
                return Count;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return Keys.ToArray();
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return _locker;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return Values.ToArray();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            lock (_locker)
            {
                _wrappedDictionary.Add(key, value);
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _wrappedDictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_locker)
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)_wrappedDictionary).Contains(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_locker)
            {
                return _wrappedDictionary.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_locker)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)_wrappedDictionary).CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock(_locker)
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)_wrappedDictionary).GetEnumerator();
            }
        }

        public bool Remove(TKey key)
        {
            lock(_locker)
            {
                return _wrappedDictionary.Remove(key);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock(_locker)
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)_wrappedDictionary).Remove(item);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result;
            lock(_locker)
            {
                result = _wrappedDictionary.TryGetValue(key, out value);
            }
            return result;
        }

        void IDictionary.Add(object key, object value)
        {
            lock(_locker)
            {
                ((IDictionary)_wrappedDictionary).Add(key, value);
            }
        }

        void IDictionary.Clear()
        {
            Clear();
        }

        bool IDictionary.Contains(object key)
        {
            lock(_locker)
            {
                return ((IDictionary)_wrappedDictionary).Contains(key);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            lock(_locker)
            {
                ((ICollection)_wrappedDictionary).CopyTo(array, index);
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            lock(_locker)
            {
                return ((IDictionary)_wrappedDictionary).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_locker)
            {
                return ((IEnumerable)_wrappedDictionary).GetEnumerator();
            }
        }

        void IDictionary.Remove(object key)
        {
            lock(_locker)
            {
                ((IDictionary)_wrappedDictionary).Remove(key);
            }
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            lock(_locker)
            {
                TValue toReturn;
                if (!_wrappedDictionary.TryGetValue(key, out toReturn))
                    _wrappedDictionary.Add(key, toReturn = addValue);
                else
                    _wrappedDictionary[key] = toReturn = updateValueFactory(key, toReturn);
                return toReturn;
            }
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            lock (_locker)
            {
                TValue toReturn;
                if (!_wrappedDictionary.TryGetValue(key, out toReturn))
                    _wrappedDictionary.Add(key, toReturn = addValueFactory(key));
                else
                    _wrappedDictionary[key] = toReturn = updateValueFactory(key, toReturn);
                return toReturn;
            }
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            lock(_locker)
            {
                TValue toReturn;
                if (!_wrappedDictionary.TryGetValue(key, out toReturn))
                    _wrappedDictionary.Add(key, toReturn = value);
                return toReturn;
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            lock (_locker)
            {
                TValue toReturn;
                if (!_wrappedDictionary.TryGetValue(key, out toReturn))
                    _wrappedDictionary.Add(key, toReturn = valueFactory(key));
                return toReturn;
            }
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            lock(_locker)
            {
                return _wrappedDictionary.ToArray();
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock(_locker)
            {
                if (_wrappedDictionary.ContainsKey(key))
                    return false;

                _wrappedDictionary.Add(key, value);
                return true;
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock(_locker)
            {
                if (!_wrappedDictionary.TryGetValue(key, out value))
                    return false;
                _wrappedDictionary.Remove(key);
                return true;
            }
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            lock(_locker)
            {
                TValue oldValue;
                if (!_wrappedDictionary.TryGetValue(key, out oldValue))
                    return false;
                if(EqualityComparer<TValue>.Default.Equals(comparisonValue, oldValue))
                {
                    _wrappedDictionary[key] = newValue;
                    return true;
                }
                return false;
            }
        }
    }
#endif
}
