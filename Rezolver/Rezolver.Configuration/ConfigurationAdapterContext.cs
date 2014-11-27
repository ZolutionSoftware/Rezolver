using System;
using System.Collections.Generic;
using System.Linq;
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
			if(afterIndex == -1)
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
	}
}
