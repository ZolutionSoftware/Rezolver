// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
namespace Rezolver.Configuration
{
	/// <summary>
	/// Interface for Singleton metadata.
	/// </summary>
	public interface ISingletonTargetMetadata : IRezolveTargetMetadata
	{
		/// <summary>
		/// Metadata representing the inner target for the singleton
		/// </summary>
		IRezolveTargetMetadata Inner { get; }
		/// <summary>
		/// If true, then the created singleton target should be a scoped singleton; i.e. with a lifetime tied to the lifetime
		/// of a parent ILifetimeScopeRezolver, not the whole application.
		/// </summary>
		bool Scoped { get; }
	}
}
