using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    public class RezolverContainer : IRezolverContainer
    {
		  private Type _t;
		  private object _obj;

        public void Register(object obj, Type type)
        {
				_obj = obj;
				_t = type;
        }


		  public object Fetch(Type type)
		  {
				if (_t.Equals(type))
					 return _obj;
				else
					 return null;
		  }
	 }
}
