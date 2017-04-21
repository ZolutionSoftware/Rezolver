using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    public class ContainerBehaviours : IList<IContainerBehaviour>, IContainerBehaviour
    {
        private readonly List<IContainerBehaviour> _inner;

        public ContainerBehaviours()
        {
            _inner = new List<IContainerBehaviour>();
        }

        public ContainerBehaviours(IEnumerable<IContainerBehaviour> range)
        {
            _inner = new List<IContainerBehaviour>(range);
        }
        public void Configure(IContainer container, ITargetContainer targets)
        {
            foreach (var behaviour in this)
            {
                behaviour.Configure(container, targets);
            }
        }

        public IEnumerable<IContainerBehaviour> GetDependencies(IEnumerable<IContainerBehaviour> behaviours)
        {
            // this collection type does not support declaring dependencies (yet) - so never return any
            return Enumerable.Empty<IContainerBehaviour>();
        }

        public void Replace(IContainerBehaviour original, IContainerBehaviour replacement)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            var index = IndexOf(original);
            if (index >= 0) this[index] = replacement;
        }

        public IContainerBehaviour this[int index] { get => _inner[index]; set => _inner[index] = value; }

        public int Count => _inner.Count;

        public bool IsReadOnly => ((ICollection<IContainerBehaviour>)_inner).IsReadOnly;

        public void Clear() => _inner.Clear();

        public bool Contains(IContainerBehaviour behaviour) => _inner.Contains(behaviour);

        public void CopyTo(IContainerBehaviour[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

        public IEnumerator<IContainerBehaviour> GetEnumerator() => _inner.GetEnumerator();

        public int IndexOf(IContainerBehaviour behaviour) => _inner.IndexOf(behaviour);

        public void Insert(int index, IContainerBehaviour behaviour)
        {
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));
            _inner.Insert(index, behaviour);
        }

        public bool Remove(IContainerBehaviour behaviour) => _inner.Remove(behaviour);

        public void RemoveAt(int index) => _inner.RemoveAt(index);

        public void Add(IContainerBehaviour behaviour)
        {
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));
            _inner.Add(behaviour);
        }

        public void AddBehaviours(IEnumerable<IContainerBehaviour> behaviours)
        {
            if ((behaviours?.Any(b => b == null)) ?? false) throw new ArgumentException("All behaviours must be non-null");
            _inner.AddRange(behaviours);
        }

        public bool RemoveBehaviours(IEnumerable<IContainerBehaviour> behaviours)
        {
            if (behaviours == null) throw new ArgumentNullException(nameof(behaviours));
            bool success = true;
            foreach (var behaviour in behaviours)
            {
                if (!Remove(behaviour))
                    success = false;
            }
            return success;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
