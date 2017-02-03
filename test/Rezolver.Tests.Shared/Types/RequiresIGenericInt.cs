using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    internal class RequiresIGenericInt
    {
		/// <summary>
		/// Gets the generic value.
		/// </summary>
		/// <value>The generic value.</value>
		public IGeneric<int> GenericValue { get; }
		public RequiresIGenericInt(IGeneric<int> genericValue)
		{
			GenericValue = genericValue;
		}
    }
}
