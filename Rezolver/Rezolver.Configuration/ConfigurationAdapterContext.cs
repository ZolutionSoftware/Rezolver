﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// This class is used to store the intermediate state for the default <see cref="IConfigurationAdapter"/>
	/// implementation's (<see cref="ConfigurationAdapter"/>) parsing 
	/// operation on an IConfiguration instance.  If you are extending the default adapter you
	/// might need also to extend this class to ensure any additional state you require is maintained.
	/// </summary>
	public class ConfigurationAdapterContext
	{
		/// <summary>
		/// Used as a sentinel type by the <see cref="ResolveType"/> method when the type search fails.
		/// </summary>
		protected class UnresolvedType
		{

		}

		private static readonly Assembly[] _defaultReferences = new[] {
			typeof(int).Assembly,
			typeof(Stack<>).Assembly,
			typeof(HashSet<>).Assembly
		};
		private static readonly string[] _emptyNamespace = new[] { "" };

		private readonly Dictionary<string, Type> _boundTypes;
		private readonly Dictionary<string, Assembly> _references;
		private readonly HashSet<string> _using;
		private readonly IConfiguration _configuration;
		private readonly List<IConfigurationError> _errors;
		private readonly List<RezolverBuilderInstruction> _instructions;

		public IConfiguration Configuration
		{
			get
			{
				return _configuration;
			}
		}

		/// <summary>
		/// Retrieves a snapshot of the current errors list.  If further errors
		/// are added while you are enumerating the enumerable returned by this property,
		/// no exception will occur, and the newly added items will not be included.
		/// </summary>
		public IEnumerable<IConfigurationError> Errors
		{
			get
			{
				return _errors.ToArray();
			}
		}

		/// <summary>
		/// Retrieves the number of errors currently in the <see cref="Errors"/> enumerable.
		/// </summary>
		public int ErrorCount
		{
			get
			{
				return _errors.Count;
			}
		}

		/// <summary>
		/// Retrieves a snapshot of the instructions currently present in the contex.
		/// </summary>
		public IEnumerable<RezolverBuilderInstruction> Instructions
		{
			get
			{
				return _instructions.ToArray();
			}
		}
		private ConfigurationAdapterContext()
		{
			_errors = new List<IConfigurationError>();
			_instructions = new List<RezolverBuilderInstruction>();
			_boundTypes = new Dictionary<string, Type>();
			_references = new Dictionary<string, Assembly>();
			_using = new HashSet<string>();
		}
		public ConfigurationAdapterContext(IConfiguration configuration)
			: this()
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			this._configuration = configuration;
		}

		public void AddError(IConfigurationError error)
		{
			if (error == null)
				throw new ArgumentNullException("error");

			_errors.Add(error);
		}

		public void AddErrors(IEnumerable<IConfigurationError> errors)
		{
			if (errors == null)
				throw new ArgumentNullException("errors");

			_errors.AddRange(errors);
		}

		public void AppendInstruction(RezolverBuilderInstruction instruction)
		{
			if (instruction == null)
				throw new ArgumentNullException("instruction");

			_instructions.Add(instruction);
		}

		/// <summary>
		/// Allows for explicit ordering of instructions
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="after"></param>
		public void InsertAfter(RezolverBuilderInstruction instruction, RezolverBuilderInstruction after)
		{
			if (instruction == null)
				throw new ArgumentNullException("instruction");

			int afterIndex = _instructions.IndexOf(after);
			if (afterIndex == -1)
				throw new ArgumentException("Object not found", "after");

			if (afterIndex == _instructions.Count - 1)
				_instructions.Add(instruction);
			else
				_instructions.Insert(afterIndex + 1, instruction);
		}

		/// <summary>
		/// Allows for explicit ordering of instructions
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="before"></param>
		public void InsertBefore(RezolverBuilderInstruction instruction, RezolverBuilderInstruction before)
		{
			if (instruction == null)
				throw new ArgumentNullException("instruction");

			int beforeIndex = _instructions.IndexOf(before);
			if (beforeIndex == -1)
				throw new ArgumentException("Object not found", "before");

			_instructions.Insert(beforeIndex, instruction);
		}

		public void InsertRangeAfter(IEnumerable<RezolverBuilderInstruction> instructions, RezolverBuilderInstruction after)
		{
			if (instructions == null)
				throw new ArgumentNullException("instructions");

			int afterIndex = _instructions.IndexOf(after);
			if (afterIndex == -1)
				throw new ArgumentException("Object not found", "after");

			if (afterIndex == _instructions.Count - 1)
				_instructions.AddRange(instructions);
			else
				_instructions.InsertRange(afterIndex + 1, instructions);
		}

		public void InsertRangeBefore(IEnumerable<RezolverBuilderInstruction> instructions, RezolverBuilderInstruction before)
		{
			if (instructions == null)
				throw new ArgumentNullException("instructions");

			int beforeIndex = _instructions.IndexOf(before);
			if (beforeIndex == -1)
				throw new ArgumentException("Object not found", "before");

			_instructions.InsertRange(beforeIndex, instructions);
		}

		//TODO: allow removal of instructions

		/// <summary>
		/// Gets an enumerable of strings of all the namespaces (using dotted separators) that are
		/// imported for the configuration.  This is used, by default, to project a list of type names
		/// to search for during type resolution.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<string> GetUsingNamespaces()
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		protected IEnumerable<string> GetNamespacePrefixedTypeNames(string typeName)
		{
			return _emptyNamespace.Concat(GetUsingNamespaces()).Select(p => string.Concat(p, ".", typeName));
		}

		/// <summary>
		/// Adds an assembly reference from an IAssemblyReferenceEntry obtained directly from an IConfiguration instance.
		/// 
		/// The default behaviour is to attempt to load an assembly with the given name and, if found, add that using the 
		/// overload that accepts an Assembly reference.
		/// </summary>
		/// <param name="entry"></param>
		public virtual void AddAssemblyReference(IAssemblyReferenceEntry entry)
		{
			//try and load assembly
			Assembly toAdd = null;
			try
			{
				toAdd = Assembly.Load(entry.AssemblyName);
			}
			catch (Exception ex)
			{
				AddError(new ConfigurationError(ex, entry));
				return;
			}

			AddAssemblyReferenceBase(toAdd);
		}

		/// <summary>
		/// Called to add an assembly reference to this context - the assembly will then be included in the search
		/// for types that are not fully qualified.
		/// </summary>
		/// <param name="assembly">Required - the assembly to be added as a reference.</param>
		public void AddAssemblyReference(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");
			AddAssemblyReferenceBase(assembly);
		}

		/// <summary>
		/// The core method for adding an assembly reference.  The method will never be called (by the base
		/// at least) with a null argument.
		/// </summary>
		/// <param name="assembly"></param>
		protected virtual void AddAssemblyReferenceBase(Assembly assembly)
		{
			//we only want a given assembly referenced once.
			//that said, it doesn't stop multiple versions of the same assembly being referenced more than once.
			_references[assembly.FullName] = assembly;
		}

		public virtual void AddAssemblyReferences(IEnumerable<Assembly> assemblies)
		{
			if (assemblies == null)
				throw new ArgumentNullException("assemblies");
			if (assemblies.Any(a => a == null))
				throw new ArgumentException("All assemblies must be non-null", "assemblies");
			
			foreach(var a in assemblies)
			{
				AddAssemblyReferenceBase(a);
			}
		}

		protected virtual IEnumerable<Assembly> GetDefaultAssemblyReferences()
		{
			return _defaultReferences;
		}

		/// <summary>
		/// Called to add the default set of referenced assemblies to this context.
		/// 
		/// In addition to overriding this method directly, a derived type can extend this list 
		/// simply by overriding the GetDefaultAssemblyReferences method.
		/// </summary>
		public virtual void AddDefaultAssemblyReferences()
		{
			AddAssemblyReferences(GetDefaultAssemblyReferences());
		} 

		/// <summary>
		/// Gets an enumerable of all the assemblies that are referenced by this configuration file.
		/// 
		/// This is used when resolving types.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Assembly> GetReferencedAssemblies()
		{
			return _references.Values;
		}

		/// <summary>
		/// Resolves a particular type name given this context's assembly references and, potentially, any namespace imports.
		/// 
		/// This is designed to be used by a configuration adapter during the process of creating a rezolver builder from a set of
		/// configuration entries.
		/// </summary>
		/// <param name="typeName">The typename </param>
		/// <param name="genericParameterCount">Used as a hint when multiple versions of the same type exist with open generic parameters
		/// and potentially with no generic parameters.  Null means that either a generic or non-generic type can match. Zero means that
		/// only a non-generic type (or a closed generic type) can match.  Any other positive value means that only an open generic type
		/// with that exact number of parameters can match.
		/// <returns>A Type reference if the type is located, otherwise null.</returns>
		/// <exception cref="System.Reflection.AmbiguousMatchException">If more than one type could be matched with the given name,
		/// typically due to namespace imports being used and more than type being available which has the same </exception>
		public virtual Type ResolveType(string typeName, int? genericParameterCount = null)
		{
			if (typeName == null)
				throw new ArgumentNullException("typeName");

			Type found = null;
			if (!_boundTypes.TryGetValue(typeName, out found))
			{
				//the search is done across all possible variants of the type name across all referenced assemblies..
				//no results = unresolved type
				//more than one result = ambiguous type
				//exacly one result = match (and is cached)
				int expectedParamCount = genericParameterCount.GetValueOrDefault(-1);

				//if the caller is after a generic type, but the typename doesn't end with the generic parameter suffix (`[num-params])
				//then append it and recurse.  Note that we don't cache this result - the result is already cached for the completed typename
				//as part of the child call.  Note - if the type name contains any commas, implying an assembly name, then this behaviour is
				//aborted - meaning that assembly-qualified names must include the generic parameter suffix on the type in order to work at all.
				if (expectedParamCount > 0 && !typeName.Contains(",") && !typeName.EndsWith(string.Concat("`", expectedParamCount.ToString())))
					found = ResolveType(string.Concat(typeName, "`", expectedParamCount.ToString()), genericParameterCount);
				else
				{
					string[] typeNames = GetNamespacePrefixedTypeNames(typeName).ToArray();
					List<Type> foundTypes = new List<Type>();
					foreach (var t in typeNames)
					{
						found = System.Type.GetType(typeName, false);

						if (found != null)
							foundTypes.Add(found);
						//and look in each referenced assembly
						foreach (var a in GetReferencedAssemblies())
						{
							found = a.GetType(t, false);

							if (found != null && !foundTypes.Contains(found))
								foundTypes.Add(found);
						}
					}

					if (expectedParamCount > 0)
					{
						foundTypes.RemoveAll(t => !t.IsGenericTypeDefinition || t.GetGenericArguments().Length != genericParameterCount);
					}
					else if (expectedParamCount == 0)
					{
						//strip out generic types as we didn't want them
						foundTypes.RemoveAll(t => t.IsGenericTypeDefinition);
					}

					//todo filter according to generic/non-generic expected.
					if (foundTypes.Count == 0)
						found = null;
					else if (foundTypes.Count > 1)
						throw new AmbiguousMatchException(string.Format("{0} types found that could match \"{1}\": {2}", foundTypes.Count, typeName, string.Join(", ", foundTypes.Select(t => t.FullName))));
					else
					{
						//if the caller didn't care about generic/non-generic, then return the single match
						//if they expected a non-generic type, then return the single match because we've stripped out any generic types already
						//if they expected a generic type, then return the single match because any false-positives have already been filtered out
						// this, because of that filtering performed earlier, means that we have found our type...
						found = foundTypes[0];
					}
					//referring to earlier note - we only cache the result of a proper search; the earlier recursion call is not cached at this level.
					_boundTypes.Add(typeName, found ?? typeof(UnresolvedType));
				}
			}
			return typeof(UnresolvedType) == found ? null : found;
		}

	}
}
