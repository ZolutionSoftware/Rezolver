using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    // Tests for the InjectEnumerables configuration 
    public class InjectEnumerablesConfigTests
    {
        public interface IMultipleRegistration
        {

        }

        public class MultipleRegistration1 : IMultipleRegistration
        {
        }

        public class MultipleRegistration2 : IMultipleRegistration
        {

        }

        [Fact]
        public void ShouldSupportRegisteringMultipleTargetsOfTheSameType()
        {
            ITargetContainer targets = new TargetContainer(Configuration.InjectEnumerables.Instance);
            targets.RegisterMultiple(new[] { Target.ForType<MultipleRegistration1>(), Target.ForType<MultipleRegistration1>() }, typeof(IMultipleRegistration));

            var fetched = targets.Fetch(typeof(IEnumerable<IMultipleRegistration>));
            Assert.NotNull(fetched);
            Assert.False(fetched.UseFallback);
        }

        [Fact]
        public void ShouldSupportRegisteringMultipleTargetsOfDifferentTypeWithCommonInterface()
        {
            ITargetContainer targets = new TargetContainer(Configuration.InjectEnumerables.Instance);

            targets.RegisterMultiple(new[] { Target.ForType<MultipleRegistration1>(), Target.ForType<MultipleRegistration2>() }, typeof(IMultipleRegistration));

            var fetched = targets.Fetch(typeof(IEnumerable<IMultipleRegistration>));
            Assert.NotNull(fetched);
            Assert.False(fetched.UseFallback);
        }

        [Fact]
        public void ShouldReturnFallbackTargetForUnregisteredIEnumerable()
        {
            ITargetContainer targets = new TargetContainer(Configuration.InjectEnumerables.Instance);
            var result = targets.Fetch(typeof(IEnumerable<int>));
            Assert.NotNull(result);
            Assert.True(result.UseFallback);
        }
    }
}
