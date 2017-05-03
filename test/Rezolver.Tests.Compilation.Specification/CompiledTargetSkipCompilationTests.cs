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
        // These feature tests came from Issue #31

        [Fact]
        public void ICompiledTarget_ShouldResolveCustomCompileTargetDirectly()
        {
            // First test just registers the custom compiled target in the container
            // and then directly resolves it.  The compiler *shouldn't* even be required because
            // the default container classes have shortcircuiting logic to avoid compiling
            // targets which are already compiled.  Even so - we still test it just in case
            // that part of the container's behaviour changes in the future (or is customised).
            var targets = CreateTargetContainer();

            // will register against 'int'
            targets.Register(new CustomTargetAndCompiledTarget(1));

            var container = CreateContainer(targets);

            Assert.Equal(1, container.Resolve<int>());
        }

        [Fact]
        public void ICompiledTarget_ShouldUseCustomCompiledTargetAsDependency()
        {
            // This test does the same, except this time the Custom target is used to
            // satisfy a dependency in another object.  This is different because the compiler 
            // should be directly involved in 'compiling' the target for the purposes of 
            // satisfying the constructor parameter.  And since we haven't told the compiler
            // anything about our custom target, it should be forced to consider its ICompiledTarget
            // implementation.
            var targets = CreateTargetContainer();

            targets.Register(new CustomTargetAndCompiledTarget(2));
            targets.RegisterType<RequiresInt>();

            var container = CreateContainer(targets);

            var result = container.Resolve<RequiresInt>();
            Assert.Equal(2, result.IntValue);
        }
    }
}
