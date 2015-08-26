using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver.Configuration
{
	public class ConstructorTargetMetadata : RezolveTargetMetadataBase, Rezolver.Configuration.IConstructorTargetMetadata
	{
		private readonly ITypeReference[] _typesToBuild;

		/// <summary>
		/// Remember - the intention is that only one of these types will actually be bound on constructing the constructor target.
		/// </summary>
		public ITypeReference[] TypesToBuild
		{
			get
			{
				return _typesToBuild;
			}
		}

		public override ITypeReference DeclaredType
		{
			get
			{
				if (_typesToBuild.Length != 1)
					throw new InvalidOperationException("Cannot expose a DeclaredType - TypesToBuild must contain exactly one type reference");
				return _typesToBuild[0];
			}
		}

		private readonly IDictionary<string, IRezolveTargetMetadata> _arguments;

		public IDictionary<string, IRezolveTargetMetadata> Arguments
		{
			get
			{
				return _arguments;
			}
		}

		private readonly ITypeReference[] _signatureTypes;
		public ITypeReference[] SignatureTypes
		{
			get
			{
				return _signatureTypes;
			}
		}

		/// <summary>
		/// Constructs a new instance of the ConstructorTargetMetadata class.
		/// </summary>
		/// <param name="typesToBuild">The types to build.</param>
		/// <param name="signatureTypes">The types of the parameters for the constructor that is to be bound.  If null, then the constructor is
		/// to be sought by finding the best match based on the arguments (if provided).  This is typically required if you have an ambiguity
		/// when matching purely by name.</param>
		/// <param name="args">The arguments.</param>
		/// <exception cref="System.ArgumentNullException">typesToBuild</exception>
		/// <exception cref="System.ArgumentException">
		/// The array cannot be empty;typesToBuild
		/// or
		/// All entries in the array must be non-null;typesToBuild
		/// </exception>
		/// <remarks>Please note that although the typesToBuild parameter is an array, in practise only one type can ever be built.
		/// Multiple types are accepted to cover scenarios where a class, its bases and zero or more interfaces are all referenced
		/// as target registration types, and these types are passed directly to the constructor target.  In building such a target from
		/// the metadata an adapter will typically find the most derived type of the group and bind to that only.  Note that if one or
		/// more types are not related to the rest of the group, then an adapter is free to throw a runtime error.</remarks>
		public ConstructorTargetMetadata(ITypeReference[] typesToBuild, ITypeReference[] signatureTypes = null, IDictionary<string, IRezolveTargetMetadata> args = null)
			: base(RezolveTargetMetadataType.Constructor)
		{
			if (typesToBuild == null) throw new ArgumentNullException("typesToBuild");
			if (typesToBuild.Length == 0) throw new ArgumentException("The array cannot be empty", "typesToBuild");
			if (typesToBuild.Any(t => t == null)) throw new ArgumentException("All entries in the array must be non-null", "typesToBuild");
			_typesToBuild = typesToBuild;
			_arguments = args;
			_signatureTypes = signatureTypes;
		}

		protected override IRezolveTarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			//note that we don't validate the metadata's types are compatible with the target types - this should already be handled
			//by any IRezolverBuilder implementation.
			Type[] typesToBuild;
			Type typeToBuild = null;
			
			//parse the TypesToBuild property value
			if (!context.TryParseTypeReferences(TypesToBuild, out typesToBuild))
				return null;

			if (typesToBuild.Length > 1)
			{
				//all the types in the list must have a common base which is in the list.
				var commonBase = typesToBuild.FirstOrDefault(t => typesToBuild.All(tt => tt != t || TypeHelpers.IsAssignableFrom(t, tt)));

				if (commonBase == null)
				{
					context.AddError(new ConfigurationError("If multiple types are provided for a constructor target, they must all be part of a common hierarchy", entry));
					return null;
				}

				//now get most derived.  Defined as one type which cannot be assigned from any of the others.
				//if there is more than one of these, then we error unless only one of them is a non-interface, non-abstract type.
				var mostDerived = typesToBuild.Where(t => typesToBuild.All(tt => tt != t || !TypeHelpers.IsAssignableFrom(t, tt))).ToArray();

				if (mostDerived.Length == 0)
				{
					context.AddError(new ConfigurationError("Couldn't identify a most-derived type to be built from the list of target types", entry));
					return null;
				}
				else if (mostDerived.Length > 1)
				{
					//get all non-abstract class types
					var nonAbstracts = typesToBuild.Where(t => TypeHelpers.IsClass(t) && TypeHelpers.IsAbstract(t)).ToArray();
					if (nonAbstracts.Length > 1)
					{
						context.AddError(new ConfigurationError("More than one non-abstract class type provided in target types - can't automatically choose which one to build", entry));
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

			if (_arguments == null)
			{
				if (TypeHelpers.IsGenericTypeDefinition(typeToBuild))
					return GenericConstructorTarget.Auto(typeToBuild);
				else
					return ConstructorTarget.Auto(typeToBuild);
			}

			ConstructorInfo[] constructors = null;

			if(SignatureTypes != null)
			{
				//try and parse all the types in the signature.
				System.Type[] ctorParamTypes = null;

				if(!context.TryParseTypeReferences(SignatureTypes, out ctorParamTypes))
					return null;

				constructors = new[] { TypeHelpers.GetConstructor(typeToBuild, ctorParamTypes) };
				if(constructors[0] == null)
				{
					context.AddError(new ConfigurationError(string.Format("Could not locate a constructor with the signature ({0}) on the type {1}", string.Join(", ", ctorParamTypes.Select(t => t.FullName), typeToBuild.FullName)), entry));
					return null;
				}
			}
			else
			{
				constructors = TypeHelpers.GetConstructors(typeToBuild);
			}

			//we need to look for constructors that have parameters with names that are found in the dictionary
			//we will ultimately select the one that has the largest number of arguments that match, and where
			//all the arguments in the dictionary are accounted for.

			var candidates = constructors.Select(c => {
				var bindings = c.GetParameters().Select(p => { 
					IRezolveTargetMetadata found = null;
					_arguments.TryGetValue(p.Name, out found);
					Type argumentType = null;
					if (found != null)
					{
						if (found.DeclaredType.IsUnbound)
						{
							//pre-bind the metadata to the parameter type - in case of unbound types
							found = found.Bind(new[] { new RuntimeTypeReference(p.ParameterType) });
						}

						if (!context.TryParseTypeReference(found.DeclaredType, out argumentType))
						{
							//if the declared type of the target produces a type that is not compatible with the 
							//parameter type, then we 'forget' about the argument - but this will also add a compilation
							//error to the context, which will then report that issue
							found = null;
						}
					}

					return new { parameter = p, argument = found, argumentType = argumentType };
				}).ToArray();

				var boundNames = new HashSet<string>(bindings.Select(b => b.parameter.Name));
				var unusedArguments = _arguments.Keys.Where(a => !boundNames.Contains(a)).ToArray();
				if (unusedArguments.Length > 0)
					return null;

				return new { constructor = c, bindings = bindings };
			}).Where(cb => cb != null 
				&& cb.bindings.All(b => (b.argument != null 
													&& b.argumentType != null 
													&& TypeHelpers.IsAssignableFrom(b.parameter.ParameterType, b.argumentType)) 
												|| b.parameter.IsOptional))
			.OrderByDescending(cb => cb.bindings.Length).ToArray();

			//candidates now contains only those constructors where all the arguments were used up,
			//and if an argument value was not provided in the dictionary for a given parameter, then 
			//the parameter must be optional.
			//we've filtered out any with argument types that couldn't be parsed - with errors being added to
 			//cater for that.
			//we've also checked whether the parameter type is compatible with the type that the argument intends
			//to produce.
			//finally, We've ordered by the length of parameters descending, so the first entry should be 
			//our match.  However, we first check whether the second entry also has the same number of 
			//parameters.  If so, then we have an ambiguity we cannot resolve, so we error.
			//but first we check whether we have any matches - if not, then that's an error.
			
			if(candidates.Length == 0)
			{
				context.AddError(new ConfigurationError("Could not find any constructors with parameters matching the names provided", entry));
				return null;
			}

			var topMatch = candidates[0];

			if(candidates.Length > 1)
			{
				//if we can find one clear winner, then we'll use it.  A clear winner is one where
				//more of the arguments' types exactly match the expected parameter types than 
				//in any other candidate binding
				var groupedByMatchingTypes = candidates.GroupBy(cb => cb.bindings.Count(b => !b.parameter.IsOptional && b.argumentType == b.parameter.ParameterType)).OrderByDescending(g => g.Key).ToArray();

				var mostMatching = groupedByMatchingTypes[0].ToArray();
				
				if(mostMatching.Length > 1)
				{
					context.AddError(new ConfigurationError("More than one constructor found with parameters matching the name and types provided", entry));
					return null;
				}
				else
					topMatch = mostMatching[0];
			}
			//todo - add the ability to pre-select constructor in this method, since we've done all the hard work of searching for it!
			return ConstructorTarget.WithArgs(typeToBuild, 
				topMatch.constructor, 
				topMatch.bindings.Where(b => b.argument != null).ToDictionary(b => b.parameter.Name, b => b.argument.CreateRezolveTarget(new[] { b.parameter.ParameterType }, context, entry)));
		}

		protected override IRezolveTargetMetadata BindBase(ITypeReference[] targetTypes)
		{
			//this is only called by the base class if DeclaredType is unbound
			return new ConstructorTargetMetadata(targetTypes, SignatureTypes, Arguments);
		}
	}
}
