using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Compilation
{
    /// <summary>
    /// An implementation of <see cref="ICompiledTarget"/> which simply wraps an instance and a known target.
    /// </summary>
    public class ConstantCompiledTarget : ICompiledTarget
    {
        private readonly object _obj;

        /// <summary>
        /// The target for which this compiled target was created.
        /// </summary>
        public ITarget SourceTarget { get; }
        /// <summary>
        /// Constructs a new instance of the <see cref="ConstantCompiledTarget"/>
        /// </summary>
        /// <param name="obj">The constant object to be returned by <see cref="GetObject(IResolveContext)"/></param>
        /// <param name="sourceTarget">The <see cref="ITarget"/> from which this compiled target is created.</param>
        public ConstantCompiledTarget(object obj, ITarget sourceTarget)
        {
            SourceTarget = sourceTarget ?? throw new ArgumentNullException(nameof(sourceTarget));
            _obj = obj;
        }

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.GetObject(IResolveContext)"/> - simply returns the
        /// target with which this instance was constructed.
        /// </summary>
        /// <param name="context">ignored</param>
        /// <returns></returns>
        public object GetObject(IResolveContext context)
        {
            return _obj;
        }
    }
}
