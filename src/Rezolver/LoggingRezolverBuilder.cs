using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class LoggingRezolverBuilder : RezolverBuilder
	{
		private static int _loggingBuilderID = 1;

		internal static int GetNextLoggingBuilderID()
		{
			return ++_loggingBuilderID;
		}

		private readonly int _id = GetNextLoggingBuilderID();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		public IRezolverLogger Logger { get; private set; }
		/// <summary>
		/// Creates a new instance that wraps around the passed IRezolverBuilder.
		/// </summary>
		/// <param name="logger">The logger that is to receive logging calls from this instance.</param>
		public LoggingRezolverBuilder(IRezolverLogger logger)
		{
			Logger = logger;
		}

		protected override IRezolveTargetEntry CreateEntry(Type type, params IRezolveTarget[] targets)
		{
			return Logger.LogCallWithResult(this, () => base.CreateEntry(type, targets), new { type = type, targets = targets });
		}

		protected override INamedRezolverBuilder CreateNamedBuilder(string name, IRezolveTarget target)
		{
			return base.CreateNamedBuilder(name, target);
		}

		public override IRezolveTargetEntry Fetch(Type type, string name)
		{
			return base.Fetch(type, name);
		}

		public override INamedRezolverBuilder GetBestNamedBuilder(RezolverPath path)
		{
			return base.GetBestNamedBuilder(path);
		}

		public override INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
		{
			return base.GetNamedBuilder(path, create);
		}

		public override void Register(IRezolveTarget target, Type type = null, RezolverPath path = null)
		{
			base.Register(target, type, path);
		}
	}
}
