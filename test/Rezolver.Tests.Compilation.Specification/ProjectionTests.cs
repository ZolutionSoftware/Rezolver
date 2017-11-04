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
        [Fact]
        public void ShouldCreateEnumerableOfSimplePageProjection()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var container = CreateContainer(targets);
            targets.RegisterType<From1, From>();
            targets.RegisterType<From2, From>();
            // TODO: Make extension methods => RegisterProjection
            throw new NotImplementedException("Test needs to be finished.");
            //targets.RegisterContainer(typeof(IEnumerable<SimplePageProjection>),
            //    new ProjectionTargetContainer(targets, typeof(Page), typeof(SimplePageProjection), t => Target.ForType<SimplePageProjection>()));

            //// Act
            //var result = container.ResolveMany<SimplePageProjection>();

            //// Assert
            //Assert.Collection(result,
            //    r => Assert.IsType<HomePage>(r.Page),
            //    r => Assert.IsType<AboutPage>(r.Page));
        }
    }
}
