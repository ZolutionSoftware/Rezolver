using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    public interface IActivator
    {
        object Activate(Activation activation, Func<ResolveContext, object> factory, ResolveContext context);
    }
}
