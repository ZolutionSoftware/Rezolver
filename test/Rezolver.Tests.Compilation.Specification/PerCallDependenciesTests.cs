using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        [Fact]
        public void ShouldUseAdHocContainer_ForDependency()
        {
            var container = new Container();
            container.RegisterType<RequiresInt, IRequiresInt>();

            container.With(10).Resolve<IRequiresInt>();
        }
    }

    public static class AdHocContainerExtensions
    {
        public static IContainer With<TService>(this IContainer container, TService service)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (container is AdHocOverridingContainer adhoc)
            {
                adhoc.Add<TService>(service);
                return adhoc;
            }
            else
            {
                adhoc = new AdHocOverridingContainer(container);
                adhoc.Add<TService>(service);
                return adhoc;
            }
        }
    }

    internal class AdHocOverridingContainer : IContainer
    {
        private ITargetContainer _registrations;
        private IContainer _inner;

        public AdHocOverridingContainer(IContainer inner)
        {
            _registrations = new TargetContainer();
            _inner = inner;
        }

        internal void Add<TService>(TService service)
        {
            _registrations.RegisterObject(service, serviceType: typeof(TService));
        }

        bool IContainer.CanResolve(IResolveContext context)
        {
            var target = _registrations.Fetch(context.RequestedType) as Targets.ObjectTarget;
            return (target != null && !target.UseFallback) || _inner.CanResolve(context);
        }

        IContainerScope IScopeFactory.CreateScope()
        {
            return new ContainerScope(this);
        }

        ICompiledTarget IContainer.GetCompiledTarget(IResolveContext context)
        {
            // the registrations only allow ObjectTargets or other objects which support ICompiledTarget or direct resolving,
            // so if it gets a result, we know we can use it.
            return (_registrations.Fetch(context.RequestedType) as ICompiledTarget) ?? _inner.GetCompiledTarget(context);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            var target = _registrations.Fetch(serviceType) as Targets.ObjectTarget;
            return target != null ? target.Value : _inner.GetService(serviceType);
        }

        object IContainer.Resolve(IResolveContext context)
        {
            var target = _registrations.Fetch(context.RequestedType) as Targets.ObjectTarget;
            return target != null ? target.Value : _inner.Resolve(context);
        }

        bool IContainer.TryResolve(IResolveContext context, out object result)
        {
            result = null;
            var target = _registrations.Fetch(context.RequestedType) as Targets.ObjectTarget;
            if (target != null)
            {
                result = target.Value;
                return true;
            }
            else
                return _inner.TryResolve(context, out result);
        }
    }
}
