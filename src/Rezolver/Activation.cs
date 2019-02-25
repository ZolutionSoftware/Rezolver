// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver
{
    public class Activation
    {
        public Type ActualType { get; }
        public int TargetId { get; }

        public ScopeBehaviour ScopeBehaviour { get; }

        public ScopePreference ScopePreference { get; }

        public Activation(Type actualType, int targetId, ScopePreference scopePreference, ScopeBehaviour scopeBehaviour)
        {
            ActualType = actualType;
            TargetId = targetId;
            ScopePreference = scopePreference;
            ScopeBehaviour = scopeBehaviour;
        }
    }
}