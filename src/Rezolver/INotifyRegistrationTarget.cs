// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// A target which receives a notification from an <see cref="ITargetContainer"/>
    /// when it is registered in an <see cref="ITargetContainer"/>.  Targets which implement
    /// this interface will typically be looking to perform some additional post-registration 
    /// configuration or additional registrations.
    /// </summary>
    internal interface INotifyRegistrationTarget : ITarget
    {
        void OnRegistration(IRootTargetContainer root, Type registeredType);
    }
}