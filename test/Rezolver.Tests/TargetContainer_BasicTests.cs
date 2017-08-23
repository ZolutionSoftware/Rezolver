using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainer_BasicTests
    {
        [Fact]
        public void ShouldRegisterNullObjectTarget()
        {
            // <example1>
            ITarget t = new ObjectTarget(null);
            ITargetContainer r = new TargetContainer();
            r.Register(t, serviceType: typeof(object));
            var t2 = r.Fetch(typeof(object));
            Assert.Same(t, t2);
            // </example1>
        }

        [Fact]
        public void ShouldNotRegisterIfTypesDontMatch()
        {
            ITarget t = new ObjectTarget("hello world");
            ITargetContainer r = new TargetContainer();
            Assert.Throws<ArgumentException>(() => r.Register(t, serviceType: typeof(int)));
        }

        [Fact]
        public void ShouldNotAllowNullTypeOnFetch()
        {
            ITargetContainer r = new TargetContainer();
            Assert.Throws<ArgumentNullException>(() => r.Fetch(null));
        }

        [Fact]
        public void ShouldRegisterForImplicitType()
        {
            ITarget t = new ObjectTarget("hello word");
            ITargetContainer rezolverBuilder = new TargetContainer();
            rezolverBuilder.Register(t);
            var t2 = rezolverBuilder.Fetch(typeof(string));
            Assert.Same(t, t2);
        }

        [Fact]
        public void ShouldSupportTwoRegistrations()
        {
            ITargetContainer targets = new TargetContainer();
            var simpleType = new NoCtor();

            ITarget target1 = Target.ForObject("hello world");
            ITarget target2 = Target.ForObject(new NoCtor());
            targets.Register(target1);
            targets.Register(target2);
            Assert.Same(target1, targets.Fetch(typeof(string)));
            Assert.Same(target2, targets.Fetch(typeof(NoCtor)));
        }
    }
}
