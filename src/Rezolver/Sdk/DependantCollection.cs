using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Sdk
{
    /// <summary>
    /// A collection of objects in which one or more objects can be dependent on others in the 
    /// same collection (via the <see cref="IDependant"/> interface).
    /// </summary>
    /// <typeparam name="T">The type of object in the collection</typeparam>
    /// <remarks>
    /// To iterate the collection in order of least dependant to most dependant, enumerate the
    /// result of the <see cref="Ordered"/> function - which uses a typical DAG topological
    /// sort to organise the objects by least dependent to most dependent.
    /// 
    /// In order for this to work - one or more objects in the collection must implement the <see cref="IDependant"/>
    /// interface.</remarks>
    public class DependantCollection<T> : IList<T>
        where T : class
    {
        private readonly List<T> _inner;

        /// <summary>
        /// Constructs a new empty <see cref="DependantCollection{T}"/> instance
        /// </summary>
        public DependantCollection()
        {
            _inner = new List<T>();
        }

        /// <summary>
        /// Constructs a new <see cref="DependantCollection{T}"/> with the passed range of objects as the initial
        /// set.
        /// </summary>
        /// <param name="range">The items to be added to the collection on construction.  If null, then the collection simply
        /// starts off empty.</param>
        public DependantCollection(IEnumerable<T> range)
        {
            _inner = new List<T>(range ?? Enumerable.Empty<T>());
        }

        /// <summary>
        /// Provides a convenient way to initialise a <see cref="DependantCollection{T}"/> using a variable argument
        /// list.
        /// </summary>
        /// <param name="dependants">The items to be added to the collection on construction.</param>
        public DependantCollection(params T[] dependants)
            : this((IEnumerable<T>)dependants)
        {

        }

        /// <summary>
        /// Gets an enumerable which will return the items in the collection in order of least dependent
        /// to most dependent.  Therefore, if item 1 depends on item 2, this enumerable will return item 2
        /// first, followed by item 1.
        /// 
        /// Items which have no dependencies within the same collection will be sorted earlier in the collection.
        /// 
        /// The sort is stable with respect to the order in which items are added; so equally dependant objects will
        /// retain their original order.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Ordered
        {
            get
            {
                // TODO: consider caching this instance and throwing it away when collection is modified
                return new DependantSorter<T>(this);
            }
        }

        /// <summary>
        /// Replaces the <paramref name="original"/> object with the passed <paramref name="replacement"/> object, if
        /// <paramref name="original"/> is found within the collection.  The original object is returned if it is found
        /// and the replacement operation succeeds.
        /// </summary>
        /// <param name="original">Required. The object to be replaced.</param>
        /// <param name="replacement">Required. The replacement object.</param>
        public T Replace(T original, T replacement)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            var index = IndexOf(original);
            if (index >= 0)
            {
                var toReturn = this[index];
                this[index] = replacement;
                return toReturn;
            }
            return null;
        }

        /// <summary>
        /// Replaces the <paramref name="original"/> with the <paramref name="replacement"/>, or
        /// adds the <paramref name="replacement"/> to the end of the collection if the <paramref name="original"/>
        /// does not exist.
        /// </summary>
        /// <param name="original">The item to be replaced.  If found, it will be swapped with 
        /// the <paramref name="replacement"/>.</param>
        /// <param name="replacement">The item to be swapped with <paramref name="original"/> or added to
        /// the end of the collection if the <paramref name="original"/> can't be found.</param>
        public void ReplaceOrAdd(T original, T replacement)
        {
            var result = Replace(original, replacement);
            if (result == null)
                Add(replacement);
        }

        /// <summary>
        /// Replaces any objects of the type <typeparamref name="TOriginal"/> with the single object
        /// passed for the <paramref name="replacement"/> argument.  Note - the replacement will be inserted at the 
        /// first index at which a matching object was found.  If none is found, then the replacement will be added to
        /// the end of the collection.
        /// </summary>
        /// <typeparam name="TOriginal">The type of object to be removed.  Note that any object equal to, derived from,
        /// or which implements the type will be removed.</typeparam>
        /// <param name="replacement">The object to be added to the collection at the position where the first object 
        /// of the type <typeparamref name="TOriginal"/> is found.  Note that all will still be removed.</param>
        public void ReplaceAnyOrAdd<TOriginal>(T replacement)
            where TOriginal : T
        {
            var toRemoveIndices = this.Select((o, i) => new { obj = o, index = i })
                .Where(o => o.obj is TOriginal)
                .ToArray();
            var insertIndex = -1;
            for(var f = toRemoveIndices.Length; f > 0; f--)
            {
                insertIndex = toRemoveIndices[f - 1].index;
                RemoveAt(insertIndex);
            }
            if (insertIndex < 0 || insertIndex == Count)
                Add(replacement);
            else
                Insert(insertIndex, replacement);
        }

        /// <summary>
        /// Implementation of <see cref="IList{T}.this[int]"/>
        /// </summary>
        /// <param name="index">Index of the item to be read or written.</param>
        public T this[int index] { get => (T)_inner[index]; set => _inner[index] = value; }

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Count"/>
        /// </summary>
        public int Count => _inner.Count;

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.IsReadOnly"/>
        /// </summary>
        public bool IsReadOnly => ((ICollection<T>)_inner).IsReadOnly;

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Clear"/>.  Empties the collection.
        /// </summary>
        public void Clear() => _inner.Clear();
       
        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Contains(T)"/>.  Returns a boolean indicating whether
        /// the passed <paramref name="item"/> is found in the collection.
        /// </summary>
        /// <param name="item">The item to be checked.</param>
        /// <returns></returns>
        public bool Contains(T item) => _inner.Contains(item);

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.CopyTo(T[], int)"/>.
        /// </summary>
        /// <param name="array">The target array into which the collection will be copied.</param>
        /// <param name="arrayIndex">The starting index in the array </param>
        public void CopyTo(T[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => _inner.Cast<T>().GetEnumerator();

        /// <summary>
        /// Implementation of <see cref="IList{T}.IndexOf(T)"/>.
        /// </summary>
        /// <param name="item">The item whose index in the collection is to be returned.</param>
        /// <returns></returns>
        public int IndexOf(T item) => _inner.IndexOf(item);

        /// <summary>
        /// Inserts the passed <paramref name="item"/> at the given <paramref name="index"/> in the collection
        /// </summary>
        /// <param name="index">The index at which the new item is to be inserted.</param>
        /// <param name="item">Required.  The item to be inserted.</param>
        public void Insert(int index, T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _inner.Insert(index, item);
        }
        
        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Remove(T)"/>
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns></returns>
        public bool Remove(T item) => _inner.Remove(item);

        /// <summary>
        /// Implementation of <see cref="IList{T}.RemoveAt(int)"/>
        /// </summary>
        /// <param name="index">The index of the item to be removed.</param>
        public void RemoveAt(int index) => _inner.RemoveAt(index);

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Add(T)"/>
        /// </summary>
        /// <param name="item">Required.  The item to be added to the collection.</param>
        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _inner.Add(item);
        }

        /// <summary>
        /// Adds all the passed items to the collection.
        /// </summary>
        /// <param name="range">Required - the items to be added.  None of the items can be <c>null</c></param>
        public void AddAll(IEnumerable<T> range)
        {
            if ((range?.Any(b => b == null)) ?? false)
                throw new ArgumentException("All items must be non-null");

            _inner.AddRange(range);
        }

        /// <summary>
        /// Alternative to <see cref="AddAll(IEnumerable{T})"/> which allows you to pass individual items as arguments
        /// </summary>
        /// <param name="items">The items to be added</param>
        public void AddAll(params T[] items)
        {
            AddAll((IEnumerable<T>)items);
        }

        /// <summary>
        /// Removes all the <paramref name="items"/> from the collection that can be found within it.
        /// </summary>
        /// <param name="items">Required.  The items to be removed.</param>
        /// <returns><c>true</c> if all items were removed (note - if <paramref name="items"/> is empty this is
        /// also the case); <c>false</c> otherwise.</returns>
        public bool RemoveAll(IEnumerable<T> items)
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

        /// <summary>
        /// Implementation of <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
