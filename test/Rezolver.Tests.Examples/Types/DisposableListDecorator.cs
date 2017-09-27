using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class DisposableListDecorator<T> : IList<T>
    {
        private readonly IList<T> _inner;

        public DisposableListDecorator(IList<T> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        private void Dispose(IEnumerable<IDisposable> items)
        {
            foreach (var obj in items.ToArray())
            {
                try
                {
                    obj.Dispose();
                }
                catch (Exception) { /* gulp! */ }
            }
        }

        private void Dispose(IEnumerable<T> items)
        {
            Dispose(items.OfType<IDisposable>());   
        }

        private void Dispose(T item)
        {
            Dispose(new[] { item });
        }

        public T this[int index]
        {
            get => _inner[index];
            set
            {
                var old = _inner[index];
                if (old != null && !old.Equals(value))
                    Dispose(old);
                _inner[index] = value;
            }
        }

        public int Count => _inner.Count;

        public bool IsReadOnly => _inner.IsReadOnly;

        public void Add(T item)
        {
            _inner.Add(item);
        }

        public void Clear()
        {
            Dispose(_inner.OfType<IDisposable>());
            _inner.Clear();
        }

        public bool Contains(T item)
        {
            return _inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _inner.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _inner.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _inner.Insert(index, item);
        }

        public bool Remove(T item)
        {
            var result = _inner.Remove(item);
            if (result) Dispose(item);
            return result;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _inner.Count)
            {
                var toRemove = _inner[index];
                if (toRemove != null)
                    Dispose(toRemove);

                _inner.RemoveAt(index);
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }
    }
    // </example>
}
