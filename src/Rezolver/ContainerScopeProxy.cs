// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// A scope which proxies another, but with a different Container
    /// </summary>
    internal sealed class ContainerScopeProxy : ContainerScope
    {
        private readonly ContainerScope _inner;
        public ContainerScopeProxy(ContainerScope inner, Container newContainer)
            : base(newContainer)
        {
            _inner = inner;
            _canActivate = inner._canActivate;
        }

        internal override T ActivateImplicit<T>(T instance)
        {
            return _inner.ActivateImplicit(instance);
        }

        internal override T ActivateExplicit<T>(ResolveContext context, int targetId, Func<ResolveContext, T> instanceFactory)
        {
            return _inner.ActivateExplicit(context, targetId, instanceFactory);
        }
    }
}
