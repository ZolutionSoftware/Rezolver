using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class HasInjectableCollection
    {
        // Collection is exposed through a read-only property
        // It's also important that it is initialised otherwise a
        // NullReferenceException *will* occur!
        public IList<IMyService> Services { get; } = new List<IMyService>();
    }
    // </example>
}
