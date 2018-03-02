using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Types
{
    /// <summary>
    /// Testing custom list types for collection initialisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomListType<T> : IEnumerable<T>
    {
        private List<T> _list = new List<T>();

        public void Add(T item)
        {
            _list.Add(item);
        }

        public int Count { get => _list.Count; }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }
    }
}
