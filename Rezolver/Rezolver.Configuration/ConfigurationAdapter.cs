using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Standard implementation of the <see cref="IConfigurationAdapter"/> interface.
	/// </summary>
	public class ConfigurationAdapter : IConfigurationAdapter
	{
		private static IConfigurationAdapterContextFactory _defaultContextFactory = ConfigurationAdapterContextFactory.Instance;

		/// <summary>
		/// Gets or sets the default context factory.  The uninitialised default is <see cref="ConfigurationAdapterContextFactory.Instance"/>.
		/// 
		/// Note - this can never be a null reference.
		/// </summary>
		/// <value>The default context factory.</value>
		/// <exception cref="System.ArgumentNullException">If you try to set the property to null.</exception>
		public static IConfigurationAdapterContextFactory DefaultContextFactory
		{
			get
			{
				return _defaultContextFactory;
			}
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				_defaultContextFactory = value;
			}
		}

		/// <summary>
		/// Used by the <see cref="ConfigurationAdapter"/> class to sort configuration entries for processing.
		/// 
		/// It ensures that Assembly Reference entries are given priority, followed by Namespace Imports, and then
		/// finally all the rest.
		/// </summary>
		protected class ConfigurationEntryProcessOrderer : IComparer<IConfigurationEntry>
		{
			public int Compare(IConfigurationEntry x, IConfigurationEntry y)
			{
				//basically - we prioritise assembly reference entries over namespace entries, and
				//namespace entries over type reference entries.
				return GetSortPos(x.Type).CompareTo(GetSortPos(y.Type));
			}

			private int GetSortPos(ConfigurationEntryType type)
			{
				switch (type)
				{
					case ConfigurationEntryType.AssemblyReference:
						return 0;
					case ConfigurationEntryType.NamespaceImport:
						return 1;
					default:
						return 2;
				}
			}
		}

		private readonly IConfigurationAdapterContextFactory _contextFactory;

		protected IConfigurationAdapterContextFactory ContextFactory
		{
			get
			{
				return _contextFactory;
			}
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ConfigurationAdapter"/> class.
		/// </summary>
		/// <param name="contextFactory">The factory that is, by default, used to create a new 
		/// context to be used while transforming an IConfiguration instance.  If you pass null, then
		/// the <see cref="DefaultContextFactory"/> will be used.</param>
		public ConfigurationAdapter(IConfigurationAdapterContextFactory contextFactory = null) 
		{
			_contextFactory = contextFactory ?? DefaultContextFactory;
		}
		/// <summary>
		/// Attempts to create an IRezolverBuilder instance from the passed configuration object.
		/// 
		/// If the operation succeeds, then a builder will be returned, which you can then use to construct a new
		/// Rezolver.  If the operation fails, then a <see cref="ConfigurationException"/> will be thrown.
		/// </summary>
		/// <param name="configuration">The parsed configuration to be loaded.</param>
		/// <returns>An IRezolverBuilder instance ready to be used to construct a new IRezolver.</returns>
		/// <exception cref="ConfigurationException">If any part of the passed configuration is invalid (e.g.
		/// bad type references) or cannot be handled by this adapter (e.g. custom IConfigurationEntry instances or
		/// custom IRezolveTargetMetadata instances).</exception>
		public virtual IRezolverBuilder CreateBuilder(IConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}

			IRezolverBuilder toReturn = CreateBuilderInstance(configuration);

			ConfigurationAdapterContext context = CreateContext(configuration);

			AppendInstructions(context);

			if (context.ErrorCount != 0)
				throw new ConfigurationException(context);

			foreach (var instruction in context.Instructions)
			{

				try
				{
					instruction.Apply(toReturn);
				}
				catch (Exception ex)
				{
					context.AddError(new ConfigurationError(ex, instruction.Entry));
				}
			}

			if (context.ErrorCount != 0)
				throw new ConfigurationException(configuration, context.Errors);

			return toReturn;
		}

		/// <summary>
		/// Creates the context that will be used while the passed configuration is processed.
		/// 
		/// The default implementation forwards this call onto the context factory that was supplied on construction.
		/// </summary>
		/// <param name="configuration"></param>
		/// <returns></returns>
		protected virtual ConfigurationAdapterContext CreateContext(IConfiguration configuration)
		{
			return _contextFactory.CreateContext(this, configuration);
		}

		/// <summary>
		/// Called to construct the instance of the <see cref="IRezolverBuilder"/> into which registrations are to be loaded.
		/// </summary>
		/// <remarks>
		/// No parsing of the configuration is to be done here (except, perhaps, if the actual implementation of 
		/// <see cref="IRezolverBuilder"/> that is used is dependant upon, say, the type of configuration object.
		/// 
		/// The base behaviour is simply to create an instance of <see cref="RezolverBuilder"/>.
		/// </remarks>
		/// <param name="configuration">The configuration instance for which a builder is to be created.</param>
		protected virtual IRezolverBuilder CreateBuilderInstance(IConfiguration configuration)
		{
			return new RezolverBuilder();
		}

		/// <summary>
		/// Called to add instructions into the context from the configuration entries in the configuration within the passed context.
		/// </summary>
		/// <param name="context">The context for this operation - provides access to the configuration whose entries are to be processed,
		/// and acts as the target for the processing instructions.</param>
		protected virtual void AppendInstructions(ConfigurationAdapterContext context)
		{
			List<RezolverBuilderInstruction> toReturn = new List<RezolverBuilderInstruction>();
			//we have to do certain entries first
			foreach (var entry in context.Configuration.Entries.OrderBy(e => e, new ConfigurationEntryProcessOrderer()))
			{
				var instruction = TransformEntry(entry, context);
				if (instruction != null)
					context.AppendInstruction(instruction);
			}
		}

		/// <summary>
		/// Called to transform a configuration entry into an instruction that will later be performed on the builder that
		/// is constructed by the configuration adapter.
		/// </summary>
		/// <param name="entry">The entry to be transformed into an instruction.</param>
		/// <param name="context">The context for the operation.</param>
		/// <returns>An instance of <see cref="RezolverBuilderInstruction"/> if successful, otherwise null.
		/// 
		/// If errors occur, they are added to the context.</returns>
		protected virtual RezolverBuilderInstruction TransformEntry(IConfigurationEntry entry, ConfigurationAdapterContext context)
		{
			if (entry.Type == ConfigurationEntryType.TypeRegistration)
			{
				return TransformTypeRegistrationEntry(entry, context);
			}
			else if (entry.Type == ConfigurationEntryType.AssemblyReference)
			{
				return TransformAssemblyReferenceEntry(entry, context);
			}
			else
				context.AddError(new ConfigurationError(string.Format("Unsupported ConfigurationEntryType: {0}", entry.Type), entry));

			return null;
		}

		/// <summary>
		/// Transforms an <see cref="IConfigurationEntry"/> with a <see cref="IConfigurationEntry.Type"/> of 
		/// <see cref="ConfigurationEntryType.AssemblyReference"/> by attempting to convert the entry
		/// to an <see cref="IAssemblyReferenceEntry"/>, and then passing that to the current context as an assembly reference to be added.
		/// </summary>
		/// <remarks>
		/// The default behaviour of this method is not to return anything - instead the entry is passed to the context to be
		/// treated as an Assembly Reference.  
		/// 
		/// The function signature still allows the returning of an instruction, however, in case derived classes want to tie
		/// this operation to an action being performed on the <see cref="IRezolverBuilder"/> later on.
		/// </remarks>
		/// <param name="entry">The entry to be processed.</param>
		/// <param name="context">The context for the operation</param>
		/// <returns>The default implementation returns null</returns>
		protected virtual RezolverBuilderInstruction TransformAssemblyReferenceEntry(IConfigurationEntry entry, ConfigurationAdapterContext context)
		{
			//no instruction to append here - we simply add the assembly to the context
			IAssemblyReferenceEntry assemblyReferenceEntry = entry as IAssemblyReferenceEntry;
			if (assemblyReferenceEntry == null)
			{
				context.AddError(new ConfigurationError("IAssemblyReferenceEntry was expected", entry));
				return null;
			}
			//if this operation fails, then one or more errors is expected to be added to the context.
			context.AddAssemblyReference(assemblyReferenceEntry);
			return null;
		}

		/// <summary>
		/// Transforms an <see cref="IConfigurationEntry"/> with a <see cref="IConfigurationEntry.Type"/> of 
		/// <see cref="ConfigurationEntryType.TypeRegistration"/> into a <see cref="RezolverBuilderInstruction"/>.
		/// </summary>
		/// <remarks>The default behaviour is to:
		/// <list type="number">
		/// <item><description>Attempt to convert the entry to an <see cref="ITypeRegistrationEntry"/></description></item>
		/// <item><description>Parsing its type references in <see cref="ITypeRegistrationEntry.Types"/></description></item>
		/// <item><description>Constructing an <see cref="IRezolveTarget"/> from the entry's <see cref="ITypeRegistrationEntry.TargetMetadata"/> through
		/// a call to <see cref="CreateTarget"/>.</description></item>
		/// <item><description>If that returns a non-null target, then a <see cref="TypeRegistrationInstruction"/> is created and returned.</description></item>
		/// </list>
		/// </remarks>
		/// <param name="entry">The entry to be transformed.</param>
		/// <param name="context">The context for the operation.</param>
		/// <returns>If the entry can be converted into a <see cref="RezolverBuilderInstruction"/>, then an instance of that type, otherwise null.</returns>
		protected virtual RezolverBuilderInstruction TransformTypeRegistrationEntry(IConfigurationEntry entry, ConfigurationAdapterContext context)
		{
			ITypeRegistrationEntry typeRegistrationEntry = entry as ITypeRegistrationEntry;
			if (typeRegistrationEntry == null)
			{
				context.AddError(new ConfigurationError("ITypeRegistrationEntry was expected", entry));
				return null;
			}
			List<Type> targetTypes;

			if (!TryParseTypeReferences(typeRegistrationEntry.Types, context, out targetTypes))
				return null; //can't proceed, but any errors will have been placed in errorsTarget

			var target = CreateTarget(typeRegistrationEntry.TargetMetadata, entry, targetTypes, context);

			if (target != null)
			{
				return new TypeRegistrationInstruction(targetTypes, target, entry);
			}
			return null;
		}

		/// <summary>
		/// Called to create an <see cref="T:Rezolver.IRezolveTarget"/> from the passed <paramref name="metadata"/>.
		/// </summary>
		/// <param name="metadata">The metadata describing the type of target to be created.</param>
		/// <param name="lineInfo">Contains information about where in the configuration file the metadata is being referenced.</param>
		/// <param name="targetTypes">The target types for the target when it is added to an <see cref="IRezolverBuilder"/>.</param>
		/// <param name="context">The context for the operation.</param>
		/// <returns>An instance of an <see cref="IRezolveTarget"/> if one can be created from the passed metadata, otherwise null.</returns>
		protected virtual IRezolveTarget CreateTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
		{
			//TODO: I don't really like the pattern I have here.  Should have something more like a visitor or somesuch.
			switch (metadata.Type)
			{
				case RezolveTargetMetadataType.Constructor:
					{
						return CreateConstructorTarget(metadata, lineInfo, targetTypes, context);
					}
				case RezolveTargetMetadataType.Object:
					{
						return CreateObjectTarget(metadata, lineInfo, targetTypes, context);
					}
				case RezolveTargetMetadataType.Singleton:
					{
						return CreateSingletonTarget(metadata, lineInfo, targetTypes, context);
					}
				case RezolveTargetMetadataType.Extension:
					{
						return CreateExtensionTarget(metadata, lineInfo, targetTypes, context);
					}
			}

			return null;
		}

		/// <summary>
		/// Called by <see cref="CreateTarget"/> if the metadata's <see cref="IRezolveTargetMetadata.Type"/> is equal to
		/// <see cref="RezolveTargetMetadataType.Extension"/>.  The base implementation always throws a <see cref="System.NotSupportedException"/>.
		/// </summary>
		/// <param name="metadata">See <see cref="CreateTarget"/>.</param>
		/// <param name="lineInfo">See <see cref="CreateTarget"/>.</param>
		/// <param name="targetTypes">See <see cref="CreateTarget"/>.</param>
		/// <param name="context">See <see cref="CreateTarget"/>.</param>
		/// <returns>See <see cref="CreateTarget"/>.</returns>
		/// <exception cref="System.NotSupportedException">Extension metadata cannot currently be transformed by the default configuration adapter.</exception>
		protected virtual IRezolveTarget CreateExtensionTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
		{
			//double-check that the metadata has IRezolveTargetMetadataExtension interface
			IRezolveTargetMetadataExtension extensionMeta = metadata as IRezolveTargetMetadataExtension;
			if (extensionMeta == null)
			{
				context.AddError(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Singleton, typeof(ISingletonTargetMetadata), metadata.GetType(), lineInfo));
				return null;
			}

			throw new NotSupportedException("Extension metadata cannot currently be transformed by the default configuration adapter.");
		}

		/// <summary>
		/// Called by <see cref="CreateTarget" /> if the metadata's <see cref="IRezolveTargetMetadata.Type" /> is equal to
		/// <see cref="RezolveTargetMetadataType.Singleton" />.  The base implementation always throws a <see cref="System.NotImplementedException" />.
		/// </summary>
		/// <param name="metadata">See <see cref="CreateTarget" />.</param>
		/// <param name="lineInfo">See <see cref="CreateTarget" />.</param>
		/// <param name="targetTypes">See <see cref="CreateTarget" />.</param>
		/// <param name="context">See <see cref="CreateTarget" />.</param>
		/// <returns>See <see cref="CreateTarget" />.</returns>
		/// <exception cref="System.NotImplementedException">Not finished implementing singleton targets yet.</exception>		
		protected virtual IRezolveTarget CreateSingletonTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
		{
			ISingletonTargetMetadata singletonMeta = metadata as ISingletonTargetMetadata;
			if (singletonMeta == null)
			{
				context.AddError(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Singleton, typeof(ISingletonTargetMetadata), metadata.GetType(), lineInfo));
				return null;
			}

			return singletonMeta.Scoped ? (IRezolveTarget)new ScopedSingletonTarget(CreateTarget(singletonMeta.Inner, lineInfo, targetTypes, context))
				: new SingletonTarget(CreateTarget(singletonMeta.Inner, lineInfo, targetTypes, context));
		}

		/// <summary>
		/// Called by <see cref="CreateTarget"/> if the metadata's <see cref="IRezolveTargetMetadata.Type"/> is equal to
		/// <see cref="RezolveTargetMetadataType.Object"/>.
		/// </summary>
		/// <param name="metadata">See <see cref="CreateTarget"/>.</param>
		/// <param name="lineInfo">See <see cref="CreateTarget"/>.</param>
		/// <param name="targetTypes">See <see cref="CreateTarget"/>.</param>
		/// <param name="context">See <see cref="CreateTarget"/>.</param>
		/// <returns>See <see cref="CreateTarget"/>.</returns>
		/// <remarks>After converting the <paramref name="metadata"/> into an <see cref="IObjectTargetMetadata"/> instance,
		/// the base implementation looks for the first type in <paramref name="targetTypes"/> that the metadata will support
		/// when that the type is passed to the <see cref="IObjectTargetMetadata.GetObject"/> method.
		/// 
		/// As soon as a type yields a return value from that method, without an <see cref="ArgumentException"/> being thrown, then that
		/// return value will be used as the value to wrapped in an <see cref="ObjectTarget"/> and returned.</remarks>
		protected virtual IRezolveTarget CreateObjectTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
		{
			IObjectTargetMetadata objectMeta = metadata as IObjectTargetMetadata;
			if (objectMeta == null)
			{
				context.AddError(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Object, typeof(IObjectTargetMetadata), metadata.GetType(), lineInfo));
				return null;
			}

			//all the types in the list must have a common base which is in the list.
			var commonBase = targetTypes.FirstOrDefault(t => targetTypes.All(tt => tt != t || t.IsAssignableFrom(tt)));

			if (commonBase == null)
			{
				context.AddError(new ConfigurationError("If multiple types are provided for a target, they must all be part of a common hierarchy", lineInfo));
				return null;
			}

			//find the first type that the object target metadata object will dish out
			List<IConfigurationError> tempErrors = new List<IConfigurationError>();
			object theObject = null;
			foreach (var type in targetTypes)
			{
				try
				{
					theObject = objectMeta.GetObject(type);
					break;
				}
				catch (ArgumentException aex)
				{
					tempErrors.Add(new ConfigurationError(aex.Message, lineInfo));
				}
			}

			//can't check for null on the theObject because it's a valid return.  But, if every
			//target type we tried yielded an error, then it's broke...
			if (tempErrors.Count == targetTypes.Count)
			{
				context.AddErrors(tempErrors);
				return null;
			}

			return new ObjectTarget(theObject);
		}

		/// <summary>
		/// Called by <see cref="CreateTarget"/> if the metadata's <see cref="IRezolveTargetMetadata.Type"/> is equal to
		/// <see cref="RezolveTargetMetadataType.Constructor"/>.
		/// </summary>
		/// <param name="metadata">See <see cref="CreateTarget"/>.</param>
		/// <param name="lineInfo">See <see cref="CreateTarget"/>.</param>
		/// <param name="targetTypes">See <see cref="CreateTarget"/>.</param>
		/// <param name="context">See <see cref="CreateTarget"/>.</param>
		/// <returns>See <see cref="CreateTarget"/>.</returns>
		/// <remarks>After converting the <paramref name="metadata"/> into an <see cref="IConstructorTargetMetadata"/> instance,
		/// the method attempts to identify the type that is to be build by the constructor target.  If found, then a <see cref="ConstructorTarget"/>
		/// or <see cref="GenericConstructorTarget"/> is created depending on whether the target type is generic or not.  Note - a closed generic type
		/// will be handled by a <see cref="ConstructorTarget"/>.</remarks>
		protected virtual IRezolveTarget CreateConstructorTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
		{
			IConstructorTargetMetadata constructorMeta = metadata as IConstructorTargetMetadata;
			if (constructorMeta == null)
				context.AddError(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Constructor, typeof(IConstructorTargetMetadata), metadata.GetType(), lineInfo));

			//note that we don't validate the metadata's types are compatible with the target types - this should already be handled
			//by any IRezolverBuilder implementation.
			List<Type> typesToBuild;
			Type typeToBuild = null;
			//translate the target's types to build
			if (!TryParseTypeReferences(constructorMeta.TypesToBuild, context, out typesToBuild))
				return null;

			if (typesToBuild.Count > 1)
			{
				//all the types in the list must have a common base which is in the list.
				var commonBase = typesToBuild.FirstOrDefault(t => typesToBuild.All(tt => tt != t || t.IsAssignableFrom(tt)));

				if (commonBase == null)
				{
					context.AddError(new ConfigurationError("If multiple types are provided for a constructor target, they must all be part of a common hierarchy", lineInfo));
					return null;
				}

				//now get most derived.  Defined as one type which cannot be assigned from any of the others.
				//if there is more than one of these, then we error unless only one of them is a non-interface, non-abstract type.
				var mostDerived = typesToBuild.Where(t => typesToBuild.All(tt => tt != t || !t.IsAssignableFrom(tt))).ToArray();

				if (mostDerived.Length == 0)
				{
					context.AddError(new ConfigurationError("Couldn't identify a most-derived type to be built from the list of target types", lineInfo));
					return null;
				}
				else if (mostDerived.Length > 1)
				{
					//get all non-abstract class types
					var nonAbstracts = typesToBuild.Where(t => t.IsClass && !t.IsAbstract).ToArray();
					if (nonAbstracts.Length > 1)
					{
						context.AddError(new ConfigurationError("More than one non-abstract class type provided in target types - can't automatically choose which one to build", lineInfo));
						return null;
					}
					else
						typeToBuild = nonAbstracts[0];
				}
				else
					typeToBuild = mostDerived[0];
			}
			else
				typeToBuild = typesToBuild[0];

			if (typeToBuild.IsGenericTypeDefinition)
				return GenericConstructorTarget.Auto(typeToBuild);
			else
				return ConstructorTarget.Auto(typeToBuild);
		}

		/// <summary>
		/// Attempts to convert the passed <paramref name="typeReference" /> into a <see cref="System.Type"/>.
		/// 
		/// Errors are added to the <paramref name="context"/> if the method returns false.
		/// </summary>
		/// <param name="typeReference">The type reference.</param>
		/// <param name="context">The context for the operation.</param>
		/// <param name="type">The type that is identified, if successful.</param>
		/// <returns><c>true</c> if the type reference is successfully parsed, <c>false</c> otherwise (with errors being added
		/// to the <paramref name="context"/>).</returns>
		protected virtual bool TryParseTypeReference(ITypeReference typeReference, ConfigurationAdapterContext context, out Type type)
		{
			type = null;
			try
			{
				Type baseType = context.ResolveType(typeReference.TypeName, typeReference.GenericArguments == null ? (int?)null : typeReference.GenericArguments.Length);
				if (baseType == null)
				{
					context.AddError(ConfigurationError.UnresolvedType(typeReference));
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
						if(!TryParseTypeReference(typeReference.GenericArguments[f], context, out typeParameters[f]))
							return false;
					}

					type = baseType.MakeGenericType(typeParameters);
				}
				else
					type = baseType;

				return true;
			}
			catch(Exception ex) // yeah, okay: catch-all is bad, but I think it's relevant here.
			{
				context.AddError(new ConfigurationError(ex, typeReference));
				return false;
			}
		}

		/// <summary>
		/// Tries to parse all type references, returning an overall success flag, with successfully parsed types being added to a list that
		/// is returned in the <paramref name="types"/> output parameter.
		/// </summary>
		/// <param name="typeReferences">The type references.</param>
		/// <param name="context">The context for the operation.</param>
		/// <param name="types">Receives the types that are parsed.  Note that if the method returns true, 
		/// then this list will contain the same number of types as there are references in <paramref name="typeReferences"/>, in the same order.
		/// If the method returns false, however, then the number of results in this list is undefined and you will not be able to marry up the input
		/// type reference to its output type.</param>
		/// <returns><c>true</c> if all type references could be parsed, otherwise <c>false</c>.</returns>
		protected bool TryParseTypeReferences(IEnumerable<ITypeReference> typeReferences, ConfigurationAdapterContext context, out List<Type> types)
		{
			bool result = true;
			Type tempType;
			List<Type> tempTypes = new List<Type>();
			types = tempTypes;
			foreach (var typeRef in typeReferences)
			{
				if (!TryParseTypeReference(typeRef, context, out tempType))
					result = false;
				else
					tempTypes.Add(tempType);
			}
			return result;
		}
	}
}
