using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Encapsulates information about where a particular object, parsed from a configuration
	/// file, can be found.
	/// </summary>
	public interface IConfigurationLineInfo
	{
		/// <summary>
		/// Gets the line number within the configuration source that contains the start of the text from which this object was parsed.
		/// 
		/// Used in conjunction with <see cref="StartLinePos"/>, it allows you to zero-in on the exact starting point of this parsed object.
		/// </summary>
		/// <value>The start line number.</value>
		int? StartLineNo { get; }

		/// <summary>
		/// Gets the position from the start of the line, indicated by <see cref="StartLineNo"/>, where the configuration text 
		/// begins for this parsed object.
		/// </summary>
		/// <value>The start line position.</value>
		int? StartLinePos { get; }


		/// <summary>
		/// Gets the line number within the configuration source that sees the end of the text from which this object was parsed.
		/// 
		/// Used in conjunction with <see cref="EndLinePos"/>, it allows you to zero-in on the exact ending of this parsed object.
		/// </summary>
		/// <value>The end line number.</value>
		int? EndLineNo { get; }

		/// <summary>
		/// Gets the position from the start of the line, indicated by <see cref="EndLineNo"/>, where the configuration text 
		/// ends for this parsed object.
		/// </summary>
		int? EndLinePos { get; }
	}
}
