using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

#if PORTABLE
namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// TODO: Might be able to remove this class once the transition away from the PCL project is complete.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal class CallerMemberNameAttribute : Attribute
	{
	}
}
#endif

namespace Rezolver
{

	public interface IRezolverLogger
	{
		int CallStart(object callee, dynamic arguments, [CallerMemberName]string method = null);
		void CallResult<TResult>(int reqId, TResult result);
		void CallEnd(int reqId);
		void Exception(int reqId, Exception ex);
	}
}
