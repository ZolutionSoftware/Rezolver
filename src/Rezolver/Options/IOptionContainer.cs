// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Runtime;

namespace Rezolver.Options
{
    [Contravariance(false)]
    [ContainerType(typeof(IOptionContainer<>))]
    internal interface IOptionContainer<out TOption>
    {
        TOption Option { get; }
    }

    [Contravariance(true)]
    [ContainerType(typeof(IOptionContainer<,>))]
    internal interface IOptionContainer<in TService, out TOption> : IOptionContainer<TOption>
    {
    }

    [Contravariance(false)]
    [ContainerType(typeof(IAnyGenericOptionContainer<>))]
    internal interface IAnyGenericOptionContainer<out TOption> : IOptionContainer<IOptionForAnyGeneric, TOption>
    {
    }
}