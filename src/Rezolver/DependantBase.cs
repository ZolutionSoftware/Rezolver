using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Suggested base class for implementations of <see cref="IDependant{TDependency}"/>.
    /// 
    /// Provides functionality such as adding required as well as optional dependencies; plus shortcut 
    /// filters for declaring dependencies on all objects of a certain type, etc.
    /// </summary>
    /// <typeparam name="T">The type of object on which the object depends.  Should be equal to 
    /// the derived type - meaning that the object is dependent upon other objects of the same type.</typeparam>
    /// <remarks>In addition to providing the recommended implementation of <see cref="IDependant{TDependency}"/>,
    /// this class also provides numerous APIs for expressing required or optional dependencies between objects 
    /// of the same type.
    /// 
    /// When <see cref="ResolveDependencies(IEnumerable{T})"/> is called (this class' implementation of 
    /// <see cref="IDependant{TDependency}.ResolveDependencies(IEnumerable{TDependency})"/>), the dependencies that have
    /// been added through methods such as <see cref="Requires(T)"/> and <see cref="RequiresAny{TOther}"/> 
    /// </remarks>
    public class DependantBase<T> : IDependant<T>
        where T : class
    {
        private readonly DependencyCollection<T> _dependencies = new DependencyCollection<T>();
        DependencyCollection<T> IDependant<T>.Dependencies { get { return _dependencies; } }

        public IEnumerable<T> ResolveDependencies(IEnumerable<T> objects)
        {
            return _dependencies.Resolve(objects);
        }
    }
}
