using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Creates an IRezolverBuilder instance from an IConfiguration instance.
	/// </summary>
	public interface IConfigurationAdapter
	{
		IRezolverBuilder CreateBuilder(IConfiguration configuration);
	}
}
