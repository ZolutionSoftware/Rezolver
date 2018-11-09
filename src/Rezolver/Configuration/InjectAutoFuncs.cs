using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Controls whether the injection of <see cref="Func{TResult}"/> will automatically be
    /// available *without* having to use the <see cref="AutoFactoryRegistrationExtensions.RegisterAutoFunc{TResult}(IRootTargetContainer)"/>
    /// method or explicitly register <see cref="AutoFactoryTarget"/> targets.
    /// </summary>
    /// <remarks>
    /// If this is applied to an <see cref="IRootTargetContainer"/> and the <see cref="Options.EnableAutoFuncInjection"/>
    /// option has been configured to be <c>true</c>, then whenever a target is registered against a particular service type,
    /// a second registration will automatically be made against a <see cref="Func{TResult}"/> type with TResult equal
    /// to the registered service type.</remarks>
    public class InjectAutoFuncs : OptionDependentConfig<Options.EnableAutoFuncInjection>
    {
        public InjectAutoFuncs() : base(false)
        {
        }

        public override void Configure(IRootTargetContainer targets)
        {
            throw new NotImplementedException();
        }
    }
}
