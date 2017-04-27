using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// A collection of objects in which all objects can be dependent on others in the same collection.
    /// </summary>
    /// <typeparam name="TDependency">The type of object in the collection, and the type of
    /// object upon which all objects in the collection depend.  So, `Foo` is `IDependant&lt;Foo&gt;`.</typeparam>
    /// <remarks>This type is ultimately just a list of <typeparamref name="TDependency"/> which prevents
    /// null items, with a few extra manipulation functions.
    /// 
    /// To iterate the collection in order of least dependant to most dependant, enumerate the
    /// result of the <see cref="OrderByDependency"/> function - which uses a typical DAG topological
    /// sort to organise the objects by least dependent to most dependent.</remarks>
    public class DependantCollection<TDependency> : IList<TDependency>
        where TDependency : class, IDependant<TDependency>
    {
        private readonly List<IDependant<TDependency>> _inner;

        public DependantCollection()
        {
            _inner = new List<IDependant<TDependency>>();
        }

        public DependantCollection(IEnumerable<TDependency> range)
        {
            _inner = new List<IDependant<TDependency>>(range);
        }

        /// <summary>
        /// Gets an enumerable which will return the items in the collection in order of least dependant
        /// to most dependant.  Therefore, if item 1 depends on item 2, this enumerable will return item 2
        /// first, followed by item 1.
        /// 
        /// Items which have no dependencies within the same collection will be sorted earlier in the collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TDependency> OrderByDependency()
        {
            // TODO: consider caching this instance and throwing it away when collection is modified
            return new DependantSorter<TDependency>(this);
        }

        public void Replace(TDependency original, TDependency replacement)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            var index = IndexOf(original);
            if (index >= 0) this[index] = replacement;
        }

        public TDependency this[int index] { get => (TDependency)_inner[index]; set => _inner[index] = value; }

        public int Count => _inner.Count;

        public bool IsReadOnly => ((ICollection<TDependency>)_inner).IsReadOnly;

        public void Clear() => _inner.Clear();

        public bool Contains(TDependency item) => _inner.Contains(item);

        public void CopyTo(TDependency[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

        public IEnumerator<TDependency> GetEnumerator() => _inner.Cast<TDependency>().GetEnumerator();

        public int IndexOf(TDependency item) => _inner.IndexOf(item);

        public void Insert(int index, TDependency item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _inner.Insert(index, item);
        }

        public bool Remove(TDependency item) => _inner.Remove(item);

        public void RemoveAt(int index) => _inner.RemoveAt(index);

        public void Add(TDependency item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _inner.Add(item);
        }

        public void AddAll(IEnumerable<TDependency> range)
        {
            if ((range?.Any(b => b == null)) ?? false)
                throw new ArgumentException("All items must be non-null");

            _inner.AddRange(range);
        }

        public void AddAll(params TDependency[] items)
        {
            AddAll((IEnumerable<TDependency>)items);
        }

        public bool RemoveAll(IEnumerable<TDependency> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            bool success = true;
            foreach (var item in items)
            {
                if (!Remove(item))
                    success = false;
            }
            return success;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
