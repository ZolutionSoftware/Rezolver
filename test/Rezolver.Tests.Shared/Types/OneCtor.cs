using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class OneCtor : NoCtor
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

    public class OneCtorAlt1 : NoCtor
    {
        public OneCtorAlt1(int value)
        {
            Value = value;
        }
    }

    public class OneCtorAlt2 : NoCtor
    {
        public OneCtorAlt2(int value)
        {
            Value = value;
        }
    }
}
