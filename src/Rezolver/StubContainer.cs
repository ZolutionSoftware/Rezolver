// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver
{
    internal class StubContainer : Container
    {
        private static readonly TargetContainer DefaultTargetContainer = new TargetContainer(new CombinedTargetContainerConfig());

        internal static StubContainer Instance { get; } = new StubContainer();

        private StubContainer()
        : base(DefaultTargetContainer)
        {
        }
    }
}
