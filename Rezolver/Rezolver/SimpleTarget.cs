using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	 public class SimpleTarget : IRezolveTarget
	 {
		  private readonly object _object;

		  public SimpleTarget(object obj)
		  {
				this._object = obj;
		  }
	 }
}
