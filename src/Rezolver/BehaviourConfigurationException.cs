using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
#if !DOTNET
    [System.SerializableAttribute]
#endif
    public class BehaviourConfigurationException : Exception
    {
        public BehaviourConfigurationException() { }
        public BehaviourConfigurationException(string message) : base(message) { }
        public BehaviourConfigurationException(string message, Exception inner) : base(message, inner) { }
#if !DOTNET
        protected MyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
    }
}
