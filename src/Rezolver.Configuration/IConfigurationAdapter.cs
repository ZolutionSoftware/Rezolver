using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Creates an IRezolveTargetContainer instance from an IConfiguration instance.
	/// </summary>
	public interface IConfigurationAdapter
	{
		ITargetContainer CreateBuilder(IConfiguration configuration);
	}
}
