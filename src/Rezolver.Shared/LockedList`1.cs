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
			private LockedList<T> _list;
			internal ListLock(LockedList<T> list)
			{
				_list = list;
				Monitor.Enter(_list._locker);
			}
			public void Dispose()
			{
				if (Monitor.IsEntered(_list._locker))
					Monitor.Exit(_list._locker);
			}
		}

		private readonly object _locker = new object();
		private readonly List<T> _inner;

		public int Count
		{
			get
			{
				lock (_locker)
				{
					return _inner.Count;
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				lock (_locker)
				{
					return ((IList<T>)_inner).IsReadOnly;
				}
			}
		}

		public T this[int index]
		{
			get
			{
				lock (_locker)
				{
					return _inner[index];
				}
			}

			set
			{
				lock (_locker)
				{
					_inner[index] = value;
				}
			}
		}

		public LockedList(IEnumerable<T> enumerable)
		{
			_inner = new List<T>(enumerable);
		}

		public LockedList(int capacity)
		{
			_inner = new List<T>(capacity);
		}

		public LockedList()
		{
			_inner = new List<T>();
		}

		public IDisposable Lock()
		{
			return new ListLock(this);
		}

		public int IndexOf(T item)
		{
			lock (_locker)
			{
				return _inner.IndexOf(item);
			}
		}

		public void Insert(int index, T item)
		{
			lock (_locker)
			{
				_inner.Insert(index, item);
			}
		}

		public void RemoveAt(int index)
		{
			lock (_locker)
			{
				_inner.RemoveAt(index);
			}
		}

		public void Add(T item)
		{
			lock (_locker)
			{
				_inner.Add(item);
			}
		}

		public void Clear()
		{
			lock (_locker)
			{
				_inner.Clear();
			}
		}

		public bool Contains(T item)
		{
			lock (_locker)
			{
				return _inner.Contains(item);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_locker)
			{
				_inner.CopyTo(array, arrayIndex);
			}
		}

		public bool Remove(T item)
		{
			lock (_locker)
			{
				return _inner.Remove(item);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			lock (_locker)
			{
				var clone = _inner.ToArray();
				return clone.AsEnumerable().GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
