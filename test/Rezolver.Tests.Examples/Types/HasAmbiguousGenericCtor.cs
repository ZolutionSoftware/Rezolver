using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class HasAmbiguousGenericCtor<T> : IGeneric<T>
    {
        public const string DEFAULTMESSAGE = "Default Message";
        public IEnumerable<T> Objects { get; }
        public string Message { get; }

        public HasAmbiguousGenericCtor(IEnumerable<T> objects)
            : this(objects, DEFAULTMESSAGE)
        {

        }

        public HasAmbiguousGenericCtor(IEnumerable<T> objects, string message)
        {
            Objects = objects;
            Message = message;
        }
    }
    // </example>
}
