// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="ITargetContainer"/> that stores multiple targets in a list.
    ///
    /// This is not a type that you would typically use directly in your code, unless you are writing
    /// custom logic/behaviour for <see cref="ITarget"/> registration.
    /// </summary>
    /// <remarks>
    /// This type is not thread-safe, nor does it perform any type checking on the targets
    /// that are added to it.
    /// </remarks>
    public class TargetListContainer : ITargetContainer, IList<ITarget>
    {
        private readonly List<ITarget> _targets;

        /// <summary>
        /// Gets the type against which this list container is registered in its <see cref="ITargetContainer"/>.
        /// </summary>
        public Type RegisteredType { get; }

        /// <summary>
        /// Gets the default target for this list - which will always be the last target added to the list, or
        /// <c>null</c> if no targets have been added yet.
        /// </summary>
        public ITarget DefaultTarget
        {
            get
            {
                if (this._targets.Count == 0)
                {
                    return null;
                }

                return this._targets[this._targets.Count - 1];
            }
        }

        /// <summary>
        /// Gets the number of targets which have been added to the list.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get { return this._targets.Count; } }

        bool ICollection<ITarget>.IsReadOnly => ((IList<ITarget>)this._targets).IsReadOnly;

        ITarget IList<ITarget>.this[int index] { get => ((IList<ITarget>)this._targets)[index]; set => ((IList<ITarget>)this._targets)[index] = value; }

        public IRootTargetContainer Root { get; }

        private bool AllowMultiple { get; }

        private bool CanAdd => this.AllowMultiple || this.Count == 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetListContainer"/> class.
        /// </summary>
        /// <param name="root">The root target container in which this container is registered.</param>
        /// <param name="registeredType">Required - the type against which this list will be registered.</param>
        /// <param name="targets">Optional array of targets with which to initialise the list.</param>
        public TargetListContainer(IRootTargetContainer root, Type registeredType, params ITarget[] targets)
        {
            this.Root = root ?? throw new ArgumentNullException(nameof(root));
            this.RegisteredType = registeredType ?? throw new ArgumentNullException(nameof(registeredType));
            this.AllowMultiple = this.Root.GetOption(registeredType, Options.AllowMultiple.Default);

            if (this.AllowMultiple || targets?.Length <= 1)
            {
                this._targets = new List<ITarget>(targets ?? new ITarget[0]);
            }
            else
            {
                throw new ArgumentException($"Too many targets provided - only one target can be registered for the type {registeredType}", nameof(targets));
            }
        }

        /// <summary>
        /// Registers the specified target into the list.  Note - the target is not checked to see
        /// if it supports this list's <see cref="RegisteredType"/>.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="registeredType">Ignored.</param>
        public virtual void Register(ITarget target, Type registeredType = null)
        {
            this.Add(target ?? throw new ArgumentNullException(nameof(target)));
        }

        /// <summary>
        /// Returns the first target which supports the passed <paramref name="type"/>
        /// </summary>
        /// <param name="type">The type for which a target is sought.</param>
        public virtual ITarget Fetch(Type type)
        {
            // TODO: Allow specifying which way we search.  For now we're searching most recently
            // registered first.
            // TODO: allow caching of per-type lookups.  Throw cache away if the list changes.
            ITarget temp;
            for (var f = this.Count; f != 0; f--)
            {
                temp = this._targets[f-1];
                if (temp.SupportsType(type))
                {
                    return temp;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves an enumerable of all targets that have been registered to this list.
        /// </summary>
        /// <param name="type">Ignored.</param>
        public virtual IEnumerable<ITarget> FetchAll(Type type)
        {
            // always returns in order of registration
            return this.Where(t => t.SupportsType(type)).AsReadOnly();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="existing">Ignored</param>
        /// <param name="type">Ignored.</param>
        /// <exception cref="NotSupportedException">Always</exception>
        public virtual ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            // clearly - we could actually do this - if the other container is a list, too, we
            // could merge its targets into this one and return this one.
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported by this target container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITargetContainer FetchContainer(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported by this target container
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        public void RegisterContainer(Type type, ITargetContainer container)
        {
            throw new NotSupportedException();
        }

        private void IfCanAdd(Action action)
        {
            if (this.CanAdd)
            {
                action();
            }
            else
            {
                throw new InvalidOperationException($"Only one target can be registered for the type {this.RegisteredType}");
            }
        }

        /// <summary>
        /// Implementation of <see cref="IList{T}.IndexOf(T)"/>
        /// </summary>
        /// <param name="item">The item whose index is to be found</param>
        /// <returns></returns>
        public int IndexOf(ITarget item)
        {
            return ((IList<ITarget>)this._targets).IndexOf(item);
        }

        /// <summary>
        /// Implementation of <see cref="IList{T}.Insert(int, T)"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, ITarget item)
        {
            this.IfCanAdd(() => ((IList<ITarget>)this._targets).Insert(index, item));
        }

        /// <summary>
        /// Implementation of <see cref="IList{T}.RemoveAt(int)"/>
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            ((IList<ITarget>)this._targets).RemoveAt(index);
        }

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Add(T)"/>
        /// </summary>
        /// <param name="item"></param>
        public void Add(ITarget item)
        {
            this.IfCanAdd(() => ((IList<ITarget>)this._targets).Add(item));
        }

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Clear"/>
        /// </summary>
        public void Clear()
        {
            ((IList<ITarget>)this._targets).Clear();
        }

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Contains(T)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(ITarget item)
        {
            return ((IList<ITarget>)this._targets).Contains(item);
        }

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.CopyTo(T[], int)"/>
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(ITarget[] array, int arrayIndex)
        {
            ((IList<ITarget>)this._targets).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Implementation of <see cref="ICollection{T}.Remove(T)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(ITarget item)
        {
            return ((IList<ITarget>)this._targets).Remove(item);
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ITarget> GetEnumerator()
        {
            return ((IList<ITarget>)this._targets).GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<ITarget>)this._targets).GetEnumerator();
        }
    }
}
