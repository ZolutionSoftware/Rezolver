using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Suggested base class for implementations of <see cref="IDependant{TDependency}"/>.
    /// 
    /// Provides functionality such as adding required as well as optional dependencies; plus shortcut 
    /// filters for declaring dependencies on all objects of a certain type, etc.
    /// </summary>
    /// <typeparam name="T">Should be equal to the derived type - describes the type of behaviours upon which this </typeparam>
    public class DependantBase<T> : IDependant<T>
    {
        public IEnumerable<T> GetDependencies(IEnumerable<T> behaviours)
        {
            throw new NotImplementedException();
        }
    }
}
