// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public interface IUserActionsService
    {
        IEnumerable<string> GetActions();
    }

    public interface IRoleBasedActionsService : IUserActionsService
    {

    }

    public class CustomerActionsService : IRoleBasedActionsService
    {
        public IEnumerable<string> GetActions() =>
            new[] {
                "View Products"
            };
    }

    public class SalesActionsService : IRoleBasedActionsService
    {
        public IEnumerable<string> GetActions() =>
            new[] {
                "Create Customers",
                "View Products"
            };
    }

    public class AdminActionsService : IRoleBasedActionsService
    {
        public IEnumerable<string> GetActions() =>
            new[] {
                "Manage Users",
                "Manage Products",
                "Create Customers",
                "View Products"
            };
    }

    public class UserControlPanel
    {
        public IUserActionsService ActionsService { get; }
        public UserControlPanel(IUserActionsService actionsService)
        {
            ActionsService = actionsService;
        }
    }
    //</example>
}
