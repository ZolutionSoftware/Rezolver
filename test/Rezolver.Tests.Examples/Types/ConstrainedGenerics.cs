using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public interface IGeneric<T>
    {
        
    }

    public class GenericAny<T> : IGeneric<T>
    {

    }

    public class GenericAnyIMyService<T> : IGeneric<T>
        where T: IMyService
    {

    }

    public class GenericAnyMyService1<T> : IGeneric<T>
        where T: MyService1
    {

    }
    // </example>
}
