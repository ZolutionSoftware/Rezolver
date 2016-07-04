// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Types of configuration entry that can be parsed from a configuration file
	/// </summary>
	public enum ConfigurationEntryType
	{
		Unknown = 0,
		/// <summary>
		/// An instruction to load one or more assemblies before any type registrations are processed
		/// 
		/// Depending on the runtime environment, to be sure of the type binder finding
		/// namespace-qualified types mentioned in a configuration file, then any assemblies you might be
		/// referencing should be included in your configuration file.
		/// 
		/// Equally, you can use this to load assemblies that wouldn't normally be found (e.g. a la unity)
		/// 
		/// Note that types which would usually be found with a simple call to Type.GetType do not need their
		/// parent assemblies explicitly referenced.
		/// </summary>
		AssemblyReference = 1,
		/// <summary>
		/// Just like 'using' or 'import' in C# or VB, this allows you to specify namespaces that you will be using in your
		/// type names elsewhere in a configuration file.
		/// </summary>
		NamespaceImport = 2,
		/// <summary>
		/// The most common type - an instruction to generate one or more type registrations in the target builder
		/// </summary>
		TypeRegistration = 10,
		/// <summary>
		/// An instruction to create a named builder in the target builder, within which further named builders or type registrations
		/// might be performed.
		/// </summary>
		NamedBuilder = 11,
		/// <summary>
		/// A custom entry - the instance should also have the interface IConfigurationExtensionEntry
		/// </summary>
		Extension = System.Int32.MaxValue //requires IConfigurationExtensionEntry
	}

	/// <summary>
	/// Types of IRezolveTargetMetadata that can be expressed in configuration
	/// </summary>
	public enum RezolveTargetMetadataType
	{
		Unknown = 0,
		/// <summary>
		/// A physical instance to be returned when a resolve operation is performed
		/// </summary>
		Object = 1,
		/// <summary>
		/// Binding to a constructor of a type to create new instances of that type when the object is resolved
		/// </summary>
		Constructor = 2,
		/// <summary>
		/// A singleton - only one object will ever be created from the target that this metadata builds.
		/// A singleton might also be scoped - i.e. that the lifetime is limited to the lifetime of an external scope.
		/// </summary>
		Singleton = 3,
		/// <summary>
		/// A list or array whose items are individually specified as resolve targets and eventually loaded into 
		/// a ListTarget.  This enables configuration files to hand-crank a list directly and when multiple registration
		/// is not applicable.
		/// </summary>
		List = 4, 
		/// <summary>
		/// Describes a list of IRezolveTargetMetadata instances - to be used for metadata objects that require multiple
		/// metadata objects.
		/// 
		/// Can be created to feed a multiple instance registration
		/// for a single type (e.g. registering multiple instances of IFoo to a rezolver, so that you can resolve all
		/// of them by resolving IEnumerable&lt;IFoo&gt;), or simply as a collection of targets that are to be used to build an 
		/// array that is to be passed as a constructor argument or property.
		/// </summary>
		MetadataList = 5,
		/// <summary>
		/// A custom metadata - the instance should also have the interface IRezolveTargetMetadataExtension
		/// </summary>
		Extension = System.Int32.MaxValue,
	}
}
