﻿using Rezolver.Tests.Types;
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
        void HandleBase(BaseClass baseClass, List<string> output)
        {
            if (baseClass != null) output.Add(nameof(HandleBase));
        }

        void HandleChild(BaseClassChild baseClassChild, List<string> output)
        {
            if (baseClassChild != null) output.Add(nameof(HandleChild));
        }

        void HandleGrandchild(BaseClassGrandchild baseClassGrandchild, List<string> output)
        {
            if (baseClassGrandchild != null) output.Add(nameof(HandleGrandchild));
        }

        [Fact]
        public void Contravariance_ShouldResolveActionForBaseClass()
        {
            var targets = CreateTargetContainer();
            targets.RegisterObject(new Action<BaseClass, List<string>>(HandleBase));

            var container = CreateContainer(targets);
            var result = container.Resolve<Action<BaseClassChild, List<string>>>();

            List<string> output = new List<string>();
            result(new BaseClassChild(), output);
            Assert.Equal(new[] { nameof(HandleBase) }, output);
        }

        [Fact]
        public void Contravariance_ShouldFavourMoreDerivedAction()
        {
            var targets = CreateTargetContainer();
            // register two actions - the second should be used because it's for a type that's
            // closer to the one for which we request an action
            targets.RegisterObject(new Action<BaseClass, List<string>>(HandleBase));
            targets.RegisterObject(new Action<BaseClassChild, List<string>>(HandleChild));

            var container = CreateContainer(targets);
            var result = container.Resolve<Action<BaseClassGrandchild, List<string>>>();

            List<string> output = new List<string>();
            result(new BaseClassGrandchild(), output);
            Assert.Equal(new[] { nameof(HandleChild) }, output);
        }

        [Fact]
        public void Contravariance_ShouldReturnIEnumerableOfAllCompatibleActions()
        {
            var targets = CreateTargetContainer();
            targets.RegisterObject(new Action<BaseClass, List<string>>(HandleBase));
            targets.RegisterObject(new Action<BaseClassChild, List<string>>(HandleChild));
            targets.RegisterObject(new Action<BaseClassGrandchild, List<string>>(HandleGrandchild));

            // run this twice times - one for child (2 results) and grandchild (3 results)
            var container = CreateContainer(targets);
            var result1 = container.Resolve<IEnumerable<Action<BaseClassChild, List<string>>>>();
            var result2 = container.Resolve<IEnumerable<Action<BaseClassGrandchild, List<string>>>>();

            var output1 = new List<string>();
            var output2 = new List<string>();
            var input1 = new BaseClassChild();
            var input2 = new BaseClassGrandchild();
            foreach(var action in result1)
            {
                action(input1, output1);
            }
            foreach(var action in result2)
            {
                action(input2, output2);
            }

            // note that the order is established as most specific to most generic
            Assert.Equal(new[] { nameof(HandleChild), nameof(HandleBase) }, output1);
            Assert.Equal(new[] { nameof(HandleGrandchild), nameof(HandleChild), nameof(HandleBase) }, output2);
        }
        
    }
}
