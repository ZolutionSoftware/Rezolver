using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
#if DOTNET || PORTABLE
    /// <summary>
    /// Rezolver-local definition of ICloneable as the System version is not supported in CoreCLR or in the sliced up version of 
    /// portable that we use.
    /// </summary>
    public interface ICloneable
	{
		object Clone();
	}
#endif
}
