// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
namespace Rezolver.Configuration
{
	/// <summary>
	/// Interface for metadata for constructing an ObjectTarget IRezolveTarget.
	/// </summary>
	public interface IObjectTargetMetadata : IRezolveTargetMetadata
	{
		/// <summary>
		/// Called to get the object that will be registered in the IRezolveTargetContainer to be returned when a
		/// caller requests one of its registered types. The method can construct an object anew everytime it is
		/// called, or it can always return the same instance; this behaviour is implementation-dependant.
		/// </summary>
		/// <param name="type">The type of object that is desired.  The implementation determines whether this
		/// parameter is required.  If it is, and you pass null, then an ArgumentNullException will be thrown.
		/// If you pass an argument, the implementation is not bound to check or honour the type.  Its purpose
		/// is to provide a hint only, not a guarantee that the object returned is compatible with the type.</param>
		/// <returns>An object.  Note - if the operation returns null this is not an error.</returns>
		object GetObject(Type type);
	}
}
