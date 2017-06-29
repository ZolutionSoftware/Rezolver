using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// This configuration will enable automatic injection of <see cref="List{T}"/> and <see cref="IList{T}"/>
    /// when applied to an <see cref="ITargetContainer"/>.
    /// </summary>
    public class AutoLists : OptionDependentConfig<Options.EnableAutoLists>
    {
        public AutoLists Instance { get; } = new AutoLists();
        private AutoLists() : base(false) { }

        public override void Configure(ITargetContainer targets)
        {
#error TODO: implement.
        }
    }
}
