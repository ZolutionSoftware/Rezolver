using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
	public static class ListExtensions
	{
#if PORTABLE
		private class ReadOnlyCollection<T> : IList<T>
		{
			private IList<T> _list;
			public ReadOnlyCollection(IList<T> list)
			{
				_list = list;
			}
			public T this[int index]
			{
				get
				{
					return _list[index];
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
					return _list.Count;
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
				return _list.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				_list.CopyTo(array, arrayIndex);
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public int IndexOf(T item)
			{
				return _list.IndexOf(item);
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
				return GetEnumerator();
			}
		}

		public static IEnumerable<T> AsReadOnly<T>(this IList<T> list)
		{
			list.MustNotBeNull(nameof(list));
			return new ReadOnlyCollection<T>(list);
		}
#endif
	}
}
