// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
  public static class ReadOnlyEnumerableExtensions
  {
    //#if PORTABLE
    private class ReadOnlyCollection<T> : IList<T>
    {
      private T[] _range;
      public ReadOnlyCollection(IEnumerable<T> range)
      {
        _range = range.ToArray();
      }
      public T this[int index]
      {
        get
        {
          return _range[index];
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
          return _range.Length;
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
        return _range.Contains(item);
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
        _range.CopyTo(array, arrayIndex);
      }

      public IEnumerator<T> GetEnumerator()
      {
        return _range.Cast<T>().GetEnumerator();
      }

      public int IndexOf(T item)
      {
        return Array.IndexOf(_range, item, 0, _range.Length);
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

    public static IList<T> AsReadOnly<T>(this IEnumerable<T> range)
    {
      range.MustNotBeNull(nameof(range));
      return new ReadOnlyCollection<T>(range);
    }
    //#endif
  }
}
