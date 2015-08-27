using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class LoggingRezolverBuilder : IRezolverBuilder
	{
		/// <summary>
		/// Creates a new instance that wraps around the passed IRezolverBuilder.
		/// </summary>
		/// <param name="inner">The builder to wrapped.</param>
		/// <param name="logger">The logger that is to receive logging calls from this instance.</param>
		public LoggingRezolverBuilder(IRezolverBuilder inner, IRezolverLogger logger)
		{

		}

		public IEnumerable<KeyValuePair<RezolveContext, IRezolveTarget>> AllRegistrations
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IRezolveTargetEntry Fetch(Type type, string name = null)
		{
			throw new NotImplementedException();
		}

		public INamedRezolverBuilder GetBestNamedBuilder(RezolverPath path)
		{
			throw new NotImplementedException();
		}

		public INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
		{
			throw new NotImplementedException();
		}

		public void Register(IRezolveTarget target, Type serviceType = null, RezolverPath path = null)
		{
			throw new NotImplementedException();
		}
	}
}
