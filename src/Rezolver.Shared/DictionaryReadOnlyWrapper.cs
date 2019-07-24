// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System.Collections;
using System.Collections.Generic;

namespace Rezolver
{
    internal class DictionaryReadOnlyWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private static readonly IDictionary<TKey, TValue> EmptyDictionary = new Dictionary<TKey, TValue>();

        private readonly IDictionary<TKey, TValue> dictionary;

        public DictionaryReadOnlyWrapper(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary ?? EmptyDictionary;
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return this.dictionary.Keys;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                return this.dictionary.Values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
