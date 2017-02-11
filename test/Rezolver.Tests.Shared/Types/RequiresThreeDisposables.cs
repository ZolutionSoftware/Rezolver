using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class RequiresThreeDisposables
	{


		public RequiresThreeDisposables(
			Disposable first,
			Disposable2 second,
			Disposable3 third)
		{

		}
	}
}
