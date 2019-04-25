using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    /// <summary>
    /// An ITarget implementation which is also an IInstanceProvider.
    /// 
    /// This is similar to the ObjectTarget type but, crucially, because it's defined in this
    /// project, a compiler implementation defined in Rezolver won't know about it.
    /// </summary>
    public class CustomTargetAndInstanceProvider : ITarget, IInstanceProvider
    {
        public int Id { get; } = TargetBase.NextId();
        public bool UseFallback => false;

        public Type DeclaredType => _obj.GetType() ?? typeof(object);

        public ScopeBehaviour ScopeBehaviour => Rezolver.ScopeBehaviour.None;

        public ScopePreference ScopePreference => Rezolver.ScopePreference.Root;

        public ITarget SourceTarget => this;

        private object _obj;

        public CustomTargetAndInstanceProvider(object obj)
        {
            _obj = obj;
        }

        public bool SupportsType(Type type)
        {
            return type.IsAssignableFrom(_obj.GetType());
        }

        public object GetInstance(ResolveContext context)
        {
            if (!context.RequestedType.IsAssignableFrom(_obj.GetType()))
                throw new ArgumentException($"The RequestedType { context.RequestedType } on the context is not compatible with the object { _obj }", nameof(context));
            return _obj;
        }
    }
}
