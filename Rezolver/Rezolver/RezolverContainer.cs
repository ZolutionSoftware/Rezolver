using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	 public class RezolverContainer : IRezolverContainer
	 {
		  private Type _type;
		  private IRezolveTarget _target;

		  public void Register(IRezolveTarget target, Type type)
		  {
				if (target.SupportsType(type))
				{
					 _target = target;
					 _type = type;
				}
				else
					 throw new ArgumentException(string.Format(Resources.Exceptions.TargetDoesntSupportType_Format, type), "target");
		  }

		  public IRezolveTarget Fetch(Type type)
		  {
				if (_type.Equals(type))
					 return _target;
				else
					 return null;
		  }
	 }
}
