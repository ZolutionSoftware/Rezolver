using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class ConfigurationAdapter : IConfigurationAdapter
	{
		private class RezolverBuilderRegistration
		{
			public List<Type> TargetTypes { get; set; }

			public IRezolveTarget Target { get; set; }
		}

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
			List<IConfigurationError> errors = new List<IConfigurationError>();
			var entries = ParseTypeRegistrationEntries(configuration, errors);
			if (errors.Count != 0)
				throw new ConfigurationException(configuration, errors);

			foreach(var entry in entries)
			{
				foreach(var targetType in entry.TargetTypes)
				{
					try
					{
						toReturn.Register(entry.Target, targetType);
					}
					catch (Exception ex)
					{
						errors.Add(new ConfigurationError(ex, null));
					}
				}
			}

			if (errors.Count != 0)
				throw new ConfigurationException(configuration, errors);

			return toReturn;
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

		private List<RezolverBuilderRegistration> ParseTypeRegistrationEntries(IConfiguration configuration, List<IConfigurationError> errorsTarget)
		{
			List<RezolverBuilderRegistration> toReturn = new List<RezolverBuilderRegistration>();
			List<Type> targetTypes = null;
			foreach (var entry in configuration.Entries.OfType<ITypeRegistrationEntry>())
			{
				if (!TryParseTypeReferences(entry.Types, entry, errorsTarget, out targetTypes))
					continue; //can't proceed, but any errors will have been placed in errorsTarget

				var target = CreateTarget(configuration, entry, entry.TargetMetadata, targetTypes, errorsTarget);

				if (target != null)
				{
					toReturn.Add(new RezolverBuilderRegistration() { Target = target, TargetTypes = targetTypes });
				}
			}

			return toReturn;
		}

		private IRezolveTarget CreateTarget(IConfiguration configuration, IConfigurationLineInfo lineInfo, IRezolveTargetMetadata metadata, List<Type> targetTypes, List<IConfigurationError> errorsTarget)
		{
			//TODO: I don't really like the pattern I have here.  Should have something more like
			switch (metadata.Type)
			{
				case RezolveTargetMetadataType.Constructor:
					{
						IConstructorTargetMetadata constructorMeta = metadata as IConstructorTargetMetadata;
						if (constructorMeta == null)
							errorsTarget.Add(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Constructor, typeof(IConstructorTargetMetadata), metadata.GetType(), lineInfo));
						else
							return CreateConstructorTarget(configuration, lineInfo, constructorMeta, targetTypes, errorsTarget);
						break;
					}
				case RezolveTargetMetadataType.Object:
					{
						IObjectTargetMetadata objectMeta = metadata as IObjectTargetMetadata;
						if (objectMeta == null)
							errorsTarget.Add(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Object, typeof(IObjectTargetMetadata), metadata.GetType(), lineInfo));
						else
							return CreateObjectTarget(configuration, lineInfo, objectMeta, targetTypes, errorsTarget);
						break;
					}
				case RezolveTargetMetadataType.Singleton:
					{
						ISingletonTargetMetadata singletonMeta = metadata as ISingletonTargetMetadata;
						if (singletonMeta == null)
							errorsTarget.Add(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Singleton, typeof(ISingletonTargetMetadata), metadata.GetType(), lineInfo));
						else
							return CreateSingletonTarget(configuration, lineInfo, singletonMeta, targetTypes, errorsTarget);
						break;
					}
				case RezolveTargetMetadataType.Extension:
					{
						//double-check that the metadata has IRezolveTargetMetadataExtension interface
						IRezolveTargetMetadataExtension extensionMeta = metadata as IRezolveTargetMetadataExtension;
						if (extensionMeta == null)
							errorsTarget.Add(ConfigurationError.UnexpectedMetadataType(RezolveTargetMetadataType.Singleton, typeof(ISingletonTargetMetadata), metadata.GetType(), lineInfo));
						else
							return CreateExtensionTarget(configuration, lineInfo, extensionMeta, targetTypes, errorsTarget);
						break;
					}
			}

			return null;
		}

		private IRezolveTarget CreateExtensionTarget(IConfiguration configuration, IConfigurationLineInfo lineInfo, IRezolveTargetMetadataExtension extensionMeta, List<Type> targetTypes, List<IConfigurationError> errorsTarget)
		{
			throw new NotImplementedException();
		}

		private IRezolveTarget CreateSingletonTarget(IConfiguration configuration, IConfigurationLineInfo lineInfo, ISingletonTargetMetadata singletonMeta, List<Type> targetTypes, List<IConfigurationError> errorsTarget)
		{
			throw new NotImplementedException();
		}

		private IRezolveTarget CreateObjectTarget(IConfiguration configuration, IConfigurationLineInfo lineInfo, IObjectTargetMetadata metadata, List<Type> targetTypes, List<IConfigurationError> errorsTarget)
		{
			//all the types in the list must have a common base which is in the list.
			var commonBase = targetTypes.FirstOrDefault(t => targetTypes.All(tt => tt != t || t.IsAssignableFrom(tt)));

			if (commonBase == null)
			{
				errorsTarget.Add(new ConfigurationError("If multiple types are provided for a target, they must all be part of a common hierarchy", lineInfo));
				return null;
			}

			//find the first type that the object target metadata object will dish out
			List<IConfigurationError> tempErrors = new List<IConfigurationError>();
			object theObject = null;
			foreach(var type in targetTypes)
			{
				try
				{
					theObject = metadata.GetObject(type);
				}
				catch(ArgumentException aex)
				{
					tempErrors.Add(new ConfigurationError(aex.Message, lineInfo));
				}
			}

			//can't check for null on the theObject because it's a valid return.  But, if every
			//target type we tried yielded an error, then it's broke...
			if(tempErrors.Count == targetTypes.Count)
			{
				errorsTarget.AddRange(tempErrors);
			}

			return new ObjectTarget(theObject);
		}

		private IRezolveTarget CreateConstructorTarget(IConfiguration configuration, IConfigurationLineInfo lineInfo, IConstructorTargetMetadata metadata, List<Type> targetTypes, List<IConfigurationError> errorsTarget)
		{
			//note that we don't validate the metadata's types are compatible with the target types - this should already be handled
			//by any IRezolverBuilder implementation.
			List<Type> typesToBuild;
			Type typeToBuild = null;
			//translate the target's types to build
			if(!TryParseTypeReferences(metadata.TypesToBuild, lineInfo, errorsTarget, out typesToBuild))
				return null;

			if (typesToBuild.Count > 1)
			{
				//all the types in the list must have a common base which is in the list.
				var commonBase = typesToBuild.FirstOrDefault(t => typesToBuild.All(tt => tt != t || t.IsAssignableFrom(tt)));

				if(commonBase == null)
				{
					errorsTarget.Add(new ConfigurationError("If multiple types are provided for a constructor target, they must all be part of a common hierarchy", lineInfo));
					return null;
				}

				//now get most derived.  Defined as one type which cannot be assigned from any of the others.
				//if there is more than one of these, then we error unless only one of them is a non-interface, non-abstract type.
				var mostDerived = typesToBuild.Where(t => typesToBuild.All(tt => tt != t || !t.IsAssignableFrom(tt))).ToArray();

				if (mostDerived.Length == 0)
				{
					errorsTarget.Add(new ConfigurationError("Couldn't identify a most-derived type to be built from the list of target types", lineInfo));
					return null;
				}
				else if (mostDerived.Length > 1)
				{
					//get all non-abstract class types
					var nonAbstracts = typesToBuild.Where(t => t.IsClass && !t.IsAbstract).ToArray();
					if (nonAbstracts.Length > 1)
					{
						errorsTarget.Add(new ConfigurationError("More than one non-abstract class type provided in target types - can't automatically choose which one to build", lineInfo));
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

		protected virtual bool TryParseTypeReference(ITypeReference typeReference, IConfigurationLineInfo lineInfo, List<IConfigurationError> errorsTarget, out Type type)
		{
			type = null;
			Type baseType = System.Type.GetType(typeReference.TypeName, false);
			if (baseType == null)
			{
				//kick off some overridable fallbaack procedure
				errorsTarget.Add(ConfigurationError.UnresolvedType(typeReference.TypeName, lineInfo));
				return false;
			}

			//todo: process generics

			type = baseType;
			return true;
		}

		protected bool TryParseTypeReferences(IEnumerable<ITypeReference> typeReferences, IConfigurationLineInfo lineInfo, List<IConfigurationError> errorsTarget, out List<Type> types)
		{
			bool result = true;
			Type tempType;
			List<Type> tempTypes = new List<Type>();
			types = tempTypes;
			foreach (var typeRef in typeReferences)
			{
				if (!TryParseTypeReference(typeRef, lineInfo, errorsTarget, out tempType))
					result = false;
				else
					tempTypes.Add(tempType);
			}
			return result;
		}
	}
}
