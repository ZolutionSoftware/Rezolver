using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
#pragma warning disable IDE1006 // Naming Styles
    public interface SimpleGenericInterface<T>

    {
        
    }

    public interface SimpleGenericInterface<T1, T2>
    {

    }

    public interface SimpleGenericInterface<T1, T2, T3>
    {

    }
#pragma warning restore IDE1006 // Naming Styles
}
