using Rezolver.Targets;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
	public class TargetContainerExamples
	{
		public void SimpleLookup()
		{
			//<example1>
			var targets = new TargetContainer();
			targets.Register("hello world".AsObjectTarget());

			var target = targets.Fetch(typeof(string));
			Assert.IsType<ObjectTarget>(target);
			//</example1>
		}

		public void LookupByBase()
		{
			//<example2>
			var targets = new TargetContainer();
			targets.Register(ConstructorTarget.Auto<MyService>(), typeof(IMyService));

			var target = targets.Fetch(typeof(IMyService));
			Assert.IsType<ConstructorTarget>(target);
			//</example2>
		}

		public void WillRejectBecauseIncompatibleType()
		{
			//<example3>
			var targets = new TargetContainer();
			//int is obviously not compatible with IMyService.
			Assert.Throws<ArgumentException>(
				() => targets.Register(50.AsObjectTarget(), typeof(IMyService)));
			//</example3>
		}
	}
}
