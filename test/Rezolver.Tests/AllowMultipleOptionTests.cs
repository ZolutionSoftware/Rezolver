using Rezolver.Options;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class AllowMultipleOptionTests
    {
        private static void RegisterTwoTargets(TargetContainer targets, Type type)
        {
            targets.Register(new TestTarget(type, false, true));
            targets.Register(new TestTarget(type, false, true));
        }

        [Fact]
        public void ShouldAllowMultipleTargetsByDefault()
        {
            var targets = new TargetContainer();
            RegisterTwoTargets(targets, typeof(int));

            var result = targets.FetchAll(typeof(int));
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SettingOptionToFalseShouldDisallowMultipleTargets()
        {
            var targets = new TargetContainer();
            targets.SetOption<AllowMultiple>(false);

            Assert.Throws<InvalidOperationException>(() => RegisterTwoTargets(targets, typeof(int)));
        }

        [Fact]
        public void ShouldDisallowMultipleForOneTypeOnly()
        {
            var targets = new TargetContainer();
            targets.SetOption<AllowMultiple, int>(false);

            RegisterTwoTargets(targets, typeof(string));
            Assert.Throws<InvalidOperationException>(() => RegisterTwoTargets(targets, typeof(int)));
        }

        [Fact]
        public void ShouldDisallowMultipleForAnOpenGenericAndAllClosedGenerics()
        {
            var targets = new TargetContainer();
            targets.SetOption<AllowMultiple>(typeof(Generic<>), false);

            RegisterTwoTargets(targets, typeof(int));
            Assert.Throws<InvalidOperationException>(() => RegisterTwoTargets(targets, typeof(Generic<>)));
            Assert.Throws<InvalidOperationException>(() => RegisterTwoTargets(targets, typeof(Generic<int>)));
        }

        [Fact]
        public void ShouldAllowForSpecificClosedGeneric()
        {
            var targets = new TargetContainer();
            targets.SetOption<AllowMultiple>(typeof(Generic<>), false);
            targets.SetOption<AllowMultiple, Generic<int>>(true);

            RegisterTwoTargets(targets, typeof(Generic<int>));
        }

        [Fact]
        public void ShouldOverrideDisallowingForOpenButNotForOneClosedGeneric()
        {
            var targets = new TargetContainer();
            targets.SetOption<AllowMultiple>(false);
            targets.SetOption<AllowMultiple>(typeof(Generic<>), true);
            targets.SetOption<AllowMultiple, Generic<string>>(false);
        }
    }
}
