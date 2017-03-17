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
        IEnumerable<string> Roles { get; }
    }

    public class CustomerActionsService : IRoleBasedActionsService
    {
        public IEnumerable<string> Roles { get; } = new[] { "Customer" };
        public IEnumerable<string> GetActions() =>
            new[] {
                "View Products"
            };
    }

    public class SalesActionsService : IRoleBasedActionsService
    {
        public IEnumerable<string> Roles { get; } = new[] { "Sales" };
        public IEnumerable<string> GetActions() =>
            new[] {
                "Create Customers",
                "View Products"
            };
    }

    public class AdminActionsService : IRoleBasedActionsService
    {
        public IEnumerable<string> Roles { get; } = new[] { "Admin" };
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
