using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class OneCtor : NoCtor
	{
		//some expected values
		public const int ExpectedRezolvedValue = 101;
		public const int ExpectedValue = 100;
		public const int ExpectedDynamicExpressionMultiplier = 5;
		public OneCtor(int value)
		{
			Value = value;
		}
	}
}
