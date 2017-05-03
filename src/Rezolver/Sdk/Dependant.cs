using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Sdk
{
    /// <summary>
    /// Simple base class for implementations of <see cref="IDependant"/> - the <see cref="IDependant.Dependencies"/>
    /// property is implemented explictly.
    /// </summary>
    public class Dependant : IDependant
    {
        DependencyMetadataCollection IDependant.Dependencies { get; } = new DependencyMetadataCollection();
    }
}
