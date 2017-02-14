using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class RequiresThreeDisposables
	{
		public Disposable First { get; }
		public Disposable2 Second { get; }
		public Disposable3 Third { get; }

		public RequiresThreeDisposables(
			Disposable first,
			Disposable2 second,
			Disposable3 third)
		{
			First = first;
			Second = second;
			Third = third;
		}
	}
}
