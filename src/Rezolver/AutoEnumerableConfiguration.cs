using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    public class AutoEnumerableConfiguration : ITargetContainerConfiguration
    {
        public static AutoEnumerableConfiguration Instance { get; } = new AutoEnumerableConfiguration();
        private AutoEnumerableConfiguration()
        {
        }

        public void Configure(ITargetContainer targets)
        {
            targets.MustNotBeNull(nameof(targets));
            if (targets.FetchContainer(typeof(IEnumerable<>)) == null && targets.Fetch(typeof(IEnumerable<>)) == null)
                targets.RegisterContainer(typeof(IEnumerable<>), new EnumerableTargetContainer(targets));
        }
    }
}
