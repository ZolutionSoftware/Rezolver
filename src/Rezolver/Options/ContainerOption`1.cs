using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Options
{
    /// <summary>
    /// A suggested base class to use for custom container options to be read/written through the 
    /// <see cref="OptionsTargetContainerExtensions"/> extension methods.
    /// 
    /// The type of the option value is the argument to the <typeparamref name="TOption"/> type parameter.
    /// 
    /// Options must currently be objects - the ability to use callbacks to get options might be added at a 
    /// future date.
    /// </summary>
    /// <typeparam name="TOption">The underlying option value type - e.g. <see cref="bool"/>, <see cref="string"/>,
    /// <see cref="Uri"/> or whatever</typeparam>
    /// <remarks>
    /// Options in Rezolver are achieved by using registrations in the <see cref="ITargetContainer"/> that 
    /// is to be configured (and, in turn, which might then configure any <see cref="IContainer"/>s built from
    /// that target container).
    /// 
    /// Since options often take the form of primitive types - e.g <see cref="bool"/>, <see cref="string"/> etc - this
    /// means it's impossible to register multiple options which control different things which have the same underlying
    /// type if we registered them directly.
    /// 
    /// This class is offered as a way around this.  Options (e.g. <see cref="AllowMultiple"/>) are derived from this type,
    /// with the argument to the <typeparamref name="TOption"/> type parameter set to the underlying type of the option value.
    /// 
    /// Thus - each distinct option is a different type, which then means that an <see cref="ITargetContainer"/> can distinguish
    /// between them.
    /// 
    /// ---
    /// 
    /// ### Note:
    /// 
    /// Whilst Rezolver uses this type for all its configurable options, the options API implemented by the extension methods 
    /// (e.g. <see cref="OptionsTargetContainerExtensions.GetOption{TOption}(ITargetContainer, TOption)"/>
    /// or <see cref="OptionsTargetContainerExtensions.SetOption{TOption}(ITargetContainer, TOption)"/>) do **not** require
    /// you to use it.</remarks>
    public class ContainerOption<TOption>
    {
        public TOption Value { get; protected set; }

        protected ContainerOption() { }

        public static implicit operator TOption(ContainerOption<TOption> option)
        {
            return option != null ? option.Value : default(TOption);
        }
    }
}
