using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// This class is used to store the intermediate state for the default <see cref="IConfigurationAdapter"/>
	/// implementation's (<see cref="ConfigurationAdapter"/>) parsing 
	/// operation on an <see cref="IConfiguration"/> instance.  If you are extending the default adapter you
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

		private static readonly string[] _emptyNamespace = new[] { "" };

		private readonly Dictionary<string, Type> _boundTypes;
		private readonly Dictionary<string, Assembly> _references;
		private readonly HashSet<string> _using;
		private readonly IConfiguration _configuration;
		private readonly List<IConfigurationError> _errors;
		private readonly List<RezolverBuilderInstruction> _instructions;
		private readonly IConfigurationAdapter _adapter;

		public IConfigurationAdapter Adapter
		{
			get
			{
				return _adapter;
			}
		}
		/// <summary>
		/// Gets the configuration that is being processed by the adapter that is working within this context.
		/// </summary>
		/// <value>The configuration.</value>
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
		/// Retrieves a snapshot of the instructions currently present in the context.
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
		/// <summary>
		/// Constructs a new instance of the <see cref="ConfigurationAdapterContext"/> class.
		/// </summary>
		/// <param name="adapter">The adapter that will create the <see cref="ITargetContainer"/> from the configuration.</param>
		/// <param name="configuration">Required. The configuration that is being processed by the adapter for which this
		/// context is being constructed</param>
		/// <param name="defaultAssemblyReferences">Optional. Default set of assemblies that are to be searched for types
		/// when type references are processed.</param>
		public ConfigurationAdapterContext(IConfigurationAdapter adapter, IConfiguration configuration, IEnumerable<Assembly> defaultAssemblyReferences = null)
	: this()
		{
			if (adapter == null)
				throw new ArgumentNullException("adapter");
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			defaultAssemblyReferences = defaultAssemblyReferences ?? Enumerable.Empty<Assembly>();
			_adapter = adapter;
			_configuration = configuration;
			//import the default references
			foreach (var reference in defaultAssemblyReferences.Where(a => a != null))
			{
				_references[reference.FullName] = reference;
			}
		}

		public void AddError(IConfigurationError error)
		{
			if (error == null)
				throw new ArgumentNullException("error");
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
				System.Diagnostics.Debugger.Break();
#endif
			_errors.Add(error);
		}

		public void AddErrors(IEnumerable<IConfigurationError> errors)
		{
			if (errors == null)
				throw new ArgumentNullException("errors");

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
				System.Diagnostics.Debugger.Break();
#endif

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
#if DOTNET
				toAdd = Assembly.Load(new AssemblyName(entry.AssemblyName));
#else
				toAdd = Assembly.Load(entry.AssemblyName);
				
#endif

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

			foreach (var a in assemblies)
			{
				AddAssemblyReferenceBase(a);
			}
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
		/// with that exact number of parameters can match.</param>
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
#if DOTNET
							found = a.GetType(t);
#else
							found = a.GetType(t, false);
#endif

							if (found != null && !foundTypes.Contains(found))
								foundTypes.Add(found);
						}
					}

					if (expectedParamCount > 0)
					{
						foundTypes.RemoveAll(t => !TypeHelpers.IsGenericTypeDefinition(t) || TypeHelpers.GetGenericArguments(t).Length != genericParameterCount);
					}
					else if (expectedParamCount == 0)
					{
						//strip out generic types as we didn't want them
						foundTypes.RemoveAll(t => TypeHelpers.IsGenericTypeDefinition(t));
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


		/// <summary>
		/// Attempts to convert the passed <paramref name="typeReference" /> into a <see cref="System.Type"/>.
		/// 
		/// Errors are added to this context's <see cref="Errors"/> if the method returns false.
		/// </summary>
		/// <param name="typeReference">The type reference.</param>
		/// <param name="type">The type that is identified, if successful.</param>
		/// <returns><c>true</c> if the type reference is successfully parsed, <c>false</c> otherwise (with errors being added
		/// to the <see cref="Errors"/> collection).</returns>
		public virtual bool TryParseTypeReference(ITypeReference typeReference, out Type type)
		{
			if (typeReference is RuntimeTypeReference)
			{
				//RuntimeTypeReference is a special case ITypeReference that can be built directly from
				//a known Type.  In this case, we simply uncrack the type that's being wrapped by the instance.
				type = ((RuntimeTypeReference)typeReference).RuntimeType;
				return true;
			}

			type = null;

			try
			{
				Type baseType = ResolveType(typeReference.TypeName, typeReference.GenericArguments == null ? (int?)null : typeReference.GenericArguments.Length);
				if (baseType == null)
				{
					AddError(ConfigurationError.UnresolvedType(typeReference));
					return false;
				}

				//now process any generics
				if (typeReference.GenericArguments != null && typeReference.GenericArguments.Length != 0)
				{
					//it is possible that the resolved type is not generic, even though we told the context
					//to find us a generic type definition with a certain number of parameters.
					//Certainly the default implementation of the context will not do this, but since it's
					//functionality is almost entirely virtual, a derived class could misbehave.  This is
					//part of the reason for the catch-all Exception handler that wraps this code.
					Type[] typeParameters = new Type[typeReference.GenericArguments.Length];
					for (int f = 0; f < typeReference.GenericArguments.Length; f++)
					{
						//if any of these fail, then errors will be added directly to the context, leaving us
						//free simply to return false.
						if (!TryParseTypeReference(typeReference.GenericArguments[f], out typeParameters[f]))
							return false;
					}

					type = baseType.MakeGenericType(typeParameters);
				}
				else
					type = baseType;

				if (typeReference.IsArray)
					type = type.MakeArrayType();

				return true;
			}
			catch (Exception ex) // yeah, okay: catch-all is bad, but I think it's relevant here.
			{
				AddError(new ConfigurationError(ex, typeReference));
				return false;
			}
		}

		/// <summary>
		/// Tries to parse all type references, returning an overall success flag, with successfully parsed types being added to a list that
		/// is returned in the <paramref name="types"/> output parameter.
		/// </summary>
		/// <param name="typeReferences">The type references.</param>
		/// <param name="types">Receives the types that are parsed.  Note that if the method returns true, 
		/// then this list will contain the same number of types as there are references in <paramref name="typeReferences"/>, in the same order.
		/// If the method returns false, however, then the number of results in this list is undefined and you will not be able to marry up the input
		/// type reference to its output type.</param>
		/// <returns><c>true</c> if all type references could be parsed, otherwise <c>false</c>.</returns>
		public bool TryParseTypeReferences(IEnumerable<ITypeReference> typeReferences, out Type[] types)
		{
			bool result = true;
			Type tempType;
			List<Type> tempTypes = new List<Type>();
			foreach (var typeRef in typeReferences)
			{
				if (!TryParseTypeReference(typeRef, out tempType))
					result = false;
				else
					tempTypes.Add(tempType);
			}
			types = tempTypes.ToArray();

			return result;
		}
	}
}
