// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    internal class ConcatenatingEnumerableContainer : EnumerableTargetContainer
    {
        private class EnumerableConcatenator<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _concatenated;

            public EnumerableConcatenator(IResolveContext context, IEnumerable<T> baseEnumerable, IEnumerable<T> extra)
            {
                this._concatenated = baseEnumerable.Concat(extra);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this._concatenated.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private readonly OverridingContainer _owner;
        private readonly IRootTargetContainer _targets;

        public ConcatenatingEnumerableContainer(IContainer owner, IRootTargetContainer targets)
            : base(targets)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            this._owner = owner as OverridingContainer ?? throw new ArgumentException("owner must be an instance of OverridingContainer", nameof(owner));
            this._targets = targets ?? throw new ArgumentNullException(nameof(targets));
        }

        public override ITarget Fetch(Type type)
        {
            var baseCompiled = this._owner.Inner.GetCompiledTarget(new ResolveContext(this._owner.Inner, type));
            var overrideTarget = base.Fetch(type);

            // we know from above that if type is not IEnumerable<T>, then an exception will occur.
            // so this type wrangling is safe

            return Target.ForType(typeof(EnumerableConcatenator<>).MakeGenericType(TypeHelpers.GetGenericArguments(type)[0]),
                new { context = Target.Resolved<IResolveContext>(), baseEnumerable = baseCompiled.SourceTarget, extra = overrideTarget });
        }

        public override ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            if (existing is ConcatenatingEnumerableContainer)
            {
                return existing;
            }

            return this;
        }
    }
}
