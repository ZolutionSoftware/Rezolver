// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// 
	/// </summary>
	public class RezolverOptions
	{
		//public RezolverTrackingOptions Tracking { get; set; } = new RezolverTrackingOptions();
    }

	//public class RezolverTrackingOptions
	//{
	//	/// <summary>
	//	/// Gets or sets a value indicating whether tracking/logging is enabled for this application.
	//	/// </summary>
	//	/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
	//	public bool Enabled { get; set; } = false;

	//	/// <summary>
	//	/// Gets or sets a value indicating whether completed <see cref="TrackedCall"/> instances will be retained or thrown away.
	//	/// 
	//	/// Leave as <c>false</c> unless you have a specific need to be able to recall a full history of all calls to all tracking-enabled
	//	/// Rezolver components.
	//	/// </summary>
	//	/// <value><c>true</c> if completed calls are to be retained; otherwise, <c>false</c>.</value>
	//	public bool RetainCompletedCalls { get; set; } = false;

	//	/// <summary>
	//	/// Gets or sets the type of the messages automatically generated for the standard call events (Start/Result/Exception etc).
	//	/// 
	//	/// The default (<c>null</c>) prevents messages being generated for these events.
	//	/// </summary>
	//	public MessageType? CallEventsMessageType { get; set; } = null;

	//	/// <summary>
	//	/// Gets or sets the category that should be used for the logger from the logger factory.
	//	/// 
	//	/// If left as null, then no logger will be created, but the tracker itself will be available
	//	/// from the <see cref="CallTracker"/> property.
	//	/// </summary>
	//	public string LoggerCategory { get; set; }

	//	/// <summary>
	//	/// Gets the call tracker created by the <see cref="ConfigureRezolverOptions"/> instance if <see cref="Enabled"/> is <c>true</c>
	//	/// </summary>
	//	public ICallTracker CallTracker { get; internal set; }
	//}
}
