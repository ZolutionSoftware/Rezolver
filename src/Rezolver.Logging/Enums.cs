using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// Different types (levels) of message that can be sent to an <see cref="ICallTracker"/>
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Trace level (very verbose!) message
		/// </summary>
		Trace = 0,
		/// <summary>
		/// Debug-level message designed to help developers debug their application's use of Rezolver
		/// </summary>
		Debug,
		/// <summary>
		/// Informational message that will be of use to developers during normal application operation
		/// </summary>
		Information,
		/// <summary>
		/// A warning message indicating that something is not ideal, but not preventing Rezolver from 
		/// operating.
		/// </summary>
		Warning,
		/// <summary>
		/// Message containing details of an which has prevented Rezolver from performing a requested operation
		/// 
		/// Sometimes precedes an exception message
		/// </summary>
		Error
	}
}
