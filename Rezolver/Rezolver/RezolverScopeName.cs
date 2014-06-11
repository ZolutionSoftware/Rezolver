using System;

namespace Rezolver
{
	public class RezolverScopeName
	{
		public string Path { get; private set; }

		public RezolverScopeName(string path)
		{
			path.MustNotBeNull("path");

			Path = path;
		}
	}
}