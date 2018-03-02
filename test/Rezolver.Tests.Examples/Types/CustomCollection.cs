using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class CustomCollection<T> : IEnumerable<T>
    {
        List<T> innerList = new List<T>();

        public void Add(T item)
        {
            innerList.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    // </example>
}
