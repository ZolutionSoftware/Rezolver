using Rezolver.Targets;
using Rezolver.Tests.Types;
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

        private static ITargetContainer CreateTargets(ITargetContainerConfig configOverride = null)
        {
            return new TargetContainer(configOverride ?? Configuration.InjectEnumerables.Instance);
        }

        [Fact]
        public void ShouldGetEnumerableTargetWhenNoRegistrations()
        {
            ITargetContainer targets = CreateTargets();

            var result = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<int>)));
            Assert.True(result.UseFallback);
        }

        [Fact]
        public void ShouldGetEnumerableTargetWithOneRegistration()
        {
            ITargetContainer targets = CreateTargets();
            targets.RegisterType<NoCtor>();

            var result = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<NoCtor>)));
            Assert.False(result.UseFallback);
            Assert.Single(result.Targets);
        }

        [Fact]
        public void ShouldGetEnumerableTargetWithThreeRegistrations()
        {
            ITargetContainer targets = CreateTargets();
            // yes - this does register three separate instances of the same type
            targets.RegisterType<NoCtor>();
            targets.RegisterType<NoCtor>();
            targets.RegisterType<NoCtor>();

            var result = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<NoCtor>)));
            Assert.False(result.UseFallback);
            Assert.Equal(3, result.Targets.Count());
        }

        [Fact]
        public void ShouldGetEnumerableTargetAfterRegisterMultipleOfSameType()
        {
            ITargetContainer targets = CreateTargets();
            targets.RegisterMultiple(new[] { Target.ForType<NoCtor>(), Target.ForType<NoCtor>() });

            var result = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<NoCtor>)));
            Assert.False(result.UseFallback);
            Assert.Equal(2, result.Targets.Count());
        }

        [Fact]
        public void ShouldGetEnumerableTargetAfterRegisterMultipleOfDifferentTypesWithCommonBase()
        {
            ITargetContainer targets = CreateTargets();
            targets.RegisterMultiple(new[] { Target.ForType<NoCtor>(), Target.ForType<DefaultCtor>() }, typeof(NoCtor));

            var result = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<NoCtor>)));
            Assert.False(result.UseFallback);
            Assert.Equal(2, result.Targets.Count());
        }

        
    }
}
