using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
    public class OptionDependentConfigurationTests
    {
        // new class - a configuration object which is optionally dependant on others which configure
        // options of a particular type.  This will be used to build the class that we will swap in 
        // as the new base for the AutoEnumerables configuration object - which will be optionally dependant
        // on ITargetContainerConfiguration<Options.EnableAutoEnumerables>
    }
}
