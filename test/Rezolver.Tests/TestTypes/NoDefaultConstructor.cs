using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
	public class NoDefaultConstructor : NoExplicitConstructor
	{
		public const int ExpectedRezolvedValue = 101;
		public const int ExpectedComplexNamedRezolveCall = 102;
		public const int ExpectedComplexNamedRezolveCallDynamic = 103;
		public const int ExpectedValue = 100;
		public const int ExpectedDynamicExpressionMultiplier = 5;
		public NoDefaultConstructor(int value)
		{
			Value = value;
		}
	}
}
