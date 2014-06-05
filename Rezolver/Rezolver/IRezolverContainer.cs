using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	 public interface IRezolverContainer
	 {
		  void Register(IRezolveTarget target, Type type);

		  IRezolveTarget Fetch(Type type);
	 }

	 public static class IRezolverContainerExtensions
	 {
		  public static void Register(IRezolverContainer container, object obj, Type type)
		  {
				container.MustNotBeNull("container");
				//container.Register(new SimpleResolveTarget())
		  }
	 }
}
