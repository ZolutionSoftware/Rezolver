using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class CallsYouBackOnCreate
    {
        public CallsYouBackOnCreate(Action<CallsYouBackOnCreate> callback)
        {
            callback(this);
        }
    }

    public class CallsYouBackOnCreate2
    {
        public CallsYouBackOnCreate2(Action<CallsYouBackOnCreate2> callback)
        {
            callback(this);
        }
    }
    // </example>
}
