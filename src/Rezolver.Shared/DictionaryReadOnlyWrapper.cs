using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	internal class DictionaryReadOnlyWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private static readonly IDictionary<TKey, TValue> _emptyDictionary = new Dictionary<TKey, TValue>();

		private IDictionary<TKey, TValue> _dictionary;

		public DictionaryReadOnlyWrapper(IDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary ?? _emptyDictionary;
		}

		public TValue this[TKey key]
		{
			get
			{
				return _dictionary[key];
			}
		}

		public int Count
		{
			get
			{
				return _dictionary.Count;
			}
		}

		public IEnumerable<TKey> Keys
		{
			get
			{
				return _dictionary.Keys;
			}
		}

		public IEnumerable<TValue> Values
		{
			get
			{
				return _dictionary.Values;
			}
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
