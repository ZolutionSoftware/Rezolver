using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public class AppIdentity : IIdentity
    {
        public string AuthenticationType { get; } = "Magic";
        public bool IsAuthenticated { get; } = true;
        public string Name { get; } = "Joe Bloggs";
    }

    public class AppPrincipal : IPrincipal
    {
        public IIdentity Identity { get; }
        private string[] Roles { get; }
        public AppPrincipal(IIdentity identity, string[] roles)
        {
            Identity = identity;
            Roles = roles ?? new string[0];
        }
        public bool IsInRole(string role)
        {
            return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
    }
    //</example>
}
