using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Implementation of the IConfigurationAdapter.
	/// </summary>
	public class ConfigurationAdapter : IConfigurationAdapter
	{
		public ConfigurationAdapter() { }
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

			TransformEntriesToInstructions(context);

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
					context.AddError(new ConfigurationError(ex, null));
				}
			}

			if (context.ErrorCount != 0)
				throw new ConfigurationException(configuration, context.Errors);

			return toReturn;
		}

		protected virtual ConfigurationAdapterContext CreateContext(IConfiguration configuration)
		{
			return new ConfigurationAdapterContext(configuration);
		}

		/// <summary>
		/// Called to construct the instance of the IRezolverBuilder into which registrations are to be loaded.
		/// 
		/// No parsing of the configuration is to be done here (except, perhaps, if the actual implementation of IRezolverBuilder that
		/// is used id dependant on, say, the type of configuration object.
		/// 
		/// The base behaviour is simply to create an instance of RezolverBuilder.
		/// </summary>
		/// <param name="configuration"></param>
		/// <returns></returns>
		protected virtual IRezolverBuilder CreateBuilderInstance(IConfiguration configuration)
		{
			return new RezolverBuilder();
		}

		/// <summary>
		/// Called to project the configuration entries that are in configuration within the passed context into instructions.
		/// 
		/// Each instruction that is created is added into the context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected virtual void TransformEntriesToInstructions(ConfigurationAdapterContext context)
		{
			List<RezolverBuilderInstruction> toReturn = new List<RezolverBuilderInstruction>();
			foreach (var entry in context.Configuration.Entries)
			{
				var instruction = TransformEntry(entry, context);
				if (instruction != null)
					context.AppendInstruction(instruction);
			}
		}

		/// <summary>
		/// Called to transform a configuration entry into an instruction that will later be performed on the rezolver builder that
		/// is constructed by the configuration adapter.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		protected virtual RezolverBuilderInstruction TransformEntry(IConfigurationEntry entry, ConfigurationAdapterContext context)
		{
			if (entry.Type == ConfigurationEntryType.TypeRegistration)
			{
				return TransformTypeRegistrationEntry(entry, context);
			}
			else
				context.AddError(new ConfigurationError(string.Format("Unsupported ConfigurationEntryType: {0}", entry.Type), entry));

			return null;
		}

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

		private IRezolveTarget CreateTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
		{
			//TODO: I don't really like the pattern I have here.  Should have something more like
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

		private IRezolveTarget CreateExtensionTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
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

		private IRezolveTarget CreateSingletonTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
		{
			ISingletonTargetMetadata singletonMeta = metadata as ISingletonTargetMetadata;
			if (singletonMeta == null)
			{
				context.AddError(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Singleton, typeof(ISingletonTargetMetadata), metadata.GetType(), lineInfo));
				return null;
			}

			throw new NotImplementedException("Not finished implementing singleton targets yet.");
		}

		private IRezolveTarget CreateObjectTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
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

		private IRezolveTarget CreateConstructorTarget(IRezolveTargetMetadata metadata, IConfigurationLineInfo lineInfo, List<Type> targetTypes, ConfigurationAdapterContext context)
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

		protected virtual bool TryParseTypeReference(ITypeReference typeReference, ConfigurationAdapterContext context, out Type type)
		{
			type = null;
			Type baseType = System.Type.GetType(typeReference.TypeName, false);
			if (baseType == null)
			{
				//kick off some overridable fallback procedure
				context.AddError(ConfigurationError.UnresolvedType(typeReference));
				return false;
			}

			//todo: process generics

			type = baseType;
			return true;
		}

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
