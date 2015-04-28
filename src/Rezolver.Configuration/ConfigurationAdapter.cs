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
				//not all entries yield instructions to be performed on the rezolver builder
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
		/// <item><description>If that returns a non-null target, then a <see cref="RegisterInstruction"/> is created and returned.</description></item>
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
			Type[] targetTypes;

			if (!context.TryParseTypeReferences(typeRegistrationEntry.Types, out targetTypes))
				return null; //can't proceed, but any errors will have been placed in errorsTarget

			if (typeRegistrationEntry.IsMultipleRegistration)
			{
				List<IRezolveTarget> targets = null;
				//a multiple registration should have a MetadataList as its TargetMetadata
				//if it doesn't, then we'll just take the one, of course
				IRezolveTargetMetadataList metadataList = typeRegistrationEntry.TargetMetadata as IRezolveTargetMetadataList;
				if (metadataList != null)
					targets = new List<IRezolveTarget>(metadataList.Targets.Select(t => t.CreateRezolveTarget(targetTypes, context)));
				else
					targets = new List<IRezolveTarget>() { typeRegistrationEntry.TargetMetadata.CreateRezolveTarget(targetTypes, context, entry) };

				return new RegisterMultipleInstruction(targetTypes, targets, entry);
			}
			else
			{
				var target = typeRegistrationEntry.TargetMetadata.CreateRezolveTarget(targetTypes.ToArray(), context, entry);

				if (target != null)
				{
					return new RegisterInstruction(targetTypes, target, entry);
				}
			}
			return null;
		}
	}
}
