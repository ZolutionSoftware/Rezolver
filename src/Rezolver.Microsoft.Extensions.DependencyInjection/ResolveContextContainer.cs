using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Converts a ResolveContext instance into a container/service provider.
    /// </summary>
    /// <seealso cref="Rezolver.IContainer" />
    internal class ResolveContextContainer : IContainer
    {
        private readonly ResolveContext _context;
        public ResolveContextContainer(ResolveContext context)
        {
            _context = context;
        }
        public bool CanResolve(ResolveContext context)
        {
            //the only property we're actually interested in is the requested type
            return _context.Container.CanResolve(context.RequestedType);
        }

        public IContainerScope CreateScope()
        {
            //handles the logic of creating the scope from an existing scope or the container
            return _context.CreateScope();
        }

        public ICompiledTarget FetchCompiled(ResolveContext context)
        {
            return _context.Container.FetchCompiled(new ResolveContext(_context.Container, context.RequestedType, context.Scope ?? _context.Scope));
        }

        public object GetService(Type serviceType)
        {
            object result = null;
            _context.Container.TryResolve(new ResolveContext(_context.Container, serviceType, _context.Scope), out result);
            return result;
        }

        public object Resolve(ResolveContext context)
        {
            //allowing here to resolve from an existing container in a new scope.
            return new ResolveContext(_context.Container, context.RequestedType, context.Scope ?? _context.Scope).Resolve(context.RequestedType);
        }

        public bool TryResolve(ResolveContext context, out object result)
        {
            return _context.Container.TryResolve(new ResolveContext(_context.Container, context.RequestedType, context.Scope ?? _context.Scope), out result);
        }
    }
}
